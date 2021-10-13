using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using AutoFixture;
using FluentAssertions;
using Hackney.Shared.Tenure.Infrastructure;
using Moq;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using TenureListener.Boundary;
using TenureListener.Domain.Account;
using TenureListener.Infrastructure.Exceptions;
using Xunit;

namespace TenureListener.Tests.E2ETests.Steps
{
    public class UpdateAccountDetailsOnTenureSteps : BaseSteps
    {
        private readonly Fixture _fixture = new Fixture();
        private Exception _lastException;
        protected readonly Guid _correlationId = Guid.NewGuid();

        public UpdateAccountDetailsOnTenureSteps()
        { }

        private SQSEvent.SQSMessage CreateMessage(Guid personId, string eventType = EventTypes.AccountCreatedEvent)
        {
            var personSns = _fixture.Build<EntityEventSns>()
                                    .With(x => x.EntityId, personId)
                                    .With(x => x.EventType, eventType)
                                    .With(x => x.CorrelationId, _correlationId)
                                    .Create();

            var msgBody = JsonSerializer.Serialize(personSns, _jsonOptions);
            return _fixture.Build<SQSEvent.SQSMessage>()
                           .With(x => x.Body, msgBody)
                           .With(x => x.MessageAttributes, new Dictionary<string, SQSEvent.MessageAttribute>())
                           .Create();
        }

        public async Task WhenTheFunctionIsTriggered(Guid accountId)
        {
            var mockLambdaLogger = new Mock<ILambdaLogger>();
            ILambdaContext lambdaContext = new TestLambdaContext()
            {
                Logger = mockLambdaLogger.Object
            };

            var sqsEvent = _fixture.Build<SQSEvent>()
                                   .With(x => x.Records, new List<SQSEvent.SQSMessage> { CreateMessage(accountId) })
                                   .Create();

            Func<Task> func = async () =>
            {
                var fn = new SqsFunction();
                await fn.FunctionHandler(sqsEvent, lambdaContext).ConfigureAwait(false);
            };

            _lastException = await Record.ExceptionAsync(func);
        }

        public async Task ThenTheTenureIsUpdatedWithTheAccountDetails(
            AccountResponseObject accountResponse, IDynamoDBContext dbContext)
        {
            var tenureId = accountResponse.Tenure.TenancyId;
            var tenureInfo = await dbContext.LoadAsync<TenureInformationDb>(tenureId);

            tenureInfo.PaymentReference.Should().Be(accountResponse.PaymentReference);
        }

        public void ThenTheCorrelationIdWasUsedInTheApiCall(string receivedCorrelationId)
        {
            receivedCorrelationId.Should().Be(_correlationId.ToString());
        }

        public void ThenAnAccountNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(AccountNotFoundException));
            (_lastException as AccountNotFoundException).Id.Should().Be(id);
        }

        public async Task ThenNoChangesAreMade(IDynamoDBContext dbContext, TenureInformationDb existingTenure)
        {
            var tenure = await dbContext.LoadAsync<TenureInformationDb>(existingTenure.Id).ConfigureAwait(false);
            tenure.Should().BeEquivalentTo(existingTenure);
        }
    }
}
