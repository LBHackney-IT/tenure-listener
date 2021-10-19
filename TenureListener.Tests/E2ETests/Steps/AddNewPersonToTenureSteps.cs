using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using AutoFixture;
using FluentAssertions;
using Hackney.Shared.Person.Boundary.Response;
using Hackney.Shared.Person.Domain;
using Hackney.Shared.Tenure.Domain;
using Hackney.Shared.Tenure.Infrastructure;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TenureListener.Boundary;
using TenureListener.Infrastructure;
using TenureListener.Infrastructure.Exceptions;
using Xunit;

namespace TenureListener.Tests.E2ETests.Steps
{
    public class AddNewPersonToTenureSteps : BaseSteps
    {
        private readonly Fixture _fixture = new Fixture();
        private Exception _lastException;
        protected readonly Guid _correlationId = Guid.NewGuid();

        public AddNewPersonToTenureSteps()
        { }

        private SQSEvent.SQSMessage CreateMessage(Guid personId, string eventVersion = EventVersions.V1)
        {
            var personSns = _fixture.Build<EntityEventSns>()
                                    .With(x => x.EntityId, personId)
                                    .With(x => x.EventType, EventTypes.PersonCreatedEvent)
                                    .With(x => x.Version, eventVersion)
                                    .With(x => x.CorrelationId, _correlationId)
                                    .Create();

            var msgBody = JsonSerializer.Serialize(personSns, _jsonOptions);
            return _fixture.Build<SQSEvent.SQSMessage>()
                           .With(x => x.Body, msgBody)
                           .With(x => x.MessageAttributes, new Dictionary<string, SQSEvent.MessageAttribute>())
                           .Create();
        }

        public async Task WhenTheFunctionIsTriggered(Guid personId)
        {
            await WhenTheFunctionIsTriggered(personId, EventVersions.V1);
        }
        public async Task WhenTheFunctionIsTriggered(Guid personId, string eventVersion)
        {
            var mockLambdaLogger = new Mock<ILambdaLogger>();
            ILambdaContext lambdaContext = new TestLambdaContext()
            {
                Logger = mockLambdaLogger.Object
            };

            var sqsEvent = _fixture.Build<SQSEvent>()
                                   .With(x => x.Records, new List<SQSEvent.SQSMessage> { CreateMessage(personId, eventVersion) })
                                   .Create();

            Func<Task> func = async () =>
            {
                var fn = new SqsFunction();
                await fn.FunctionHandler(sqsEvent, lambdaContext).ConfigureAwait(false);
            };

            _lastException = await Record.ExceptionAsync(func);
        }

        public void ThenTheCorrelationIdWasUsedInTheApiCall(string receivedCorrelationId)
        {
            receivedCorrelationId.Should().Be(_correlationId.ToString());
        }

        public async Task ThenTheTenureIsUpdatedWithTheUserDetails(
            PersonResponseObject personResponse, IDynamoDBContext dbContext)
        {
            var tenureId = personResponse.Tenures.First().Id;
            var tenureInfo = await dbContext.LoadAsync<TenureInformationDb>(tenureId);

            var lastHouseholdMember = tenureInfo.HouseholdMembers.Last();
            lastHouseholdMember.Id.Should().Be(personResponse.Id);
            lastHouseholdMember.FullName.Should().Be(personResponse.GetFullName());
            lastHouseholdMember.Type.Should().Be(HouseholdMembersType.Person);
            lastHouseholdMember.DateOfBirth.Should().Be(DateTime.Parse(personResponse.DateOfBirth));
            var isResponsible = personResponse.PersonTypes.First() == PersonType.Tenant;
            lastHouseholdMember.IsResponsible.Should().Be(isResponsible);
            lastHouseholdMember.PersonTenureType.Should().Be(tenureInfo.TenureType.GetPersonTenureType(isResponsible));
        }

        public void ThenAPersonNotFoundExceptionIsThrown(Guid tenureId)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(PersonNotFoundException));
            (_lastException as PersonNotFoundException).Id.Should().Be(tenureId);
        }

        public void ThenAPersonHasNoTenuresExceptionIsThrown(Guid personId)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(PersonHasNoTenuresException));
            (_lastException as PersonHasNoTenuresException).Id.Should().Be(personId);
        }

        public void ThenATenureNotFoundExceptionIsThrown(Guid tenureId)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(TenureNotFoundException));
            (_lastException as TenureNotFoundException).Id.Should().Be(tenureId);
        }

        public async Task ThenTheEventIsIgnored(int personApiCallsMade, TenureInformationDb tenure, IDynamoDBContext dbContext)
        {
            _lastException.Should().BeNull();
            personApiCallsMade.Should().Be(0);

            var tenureInDb = await dbContext.LoadAsync<TenureInformationDb>(tenure.Id);
            tenureInDb.Should().BeEquivalentTo(tenure);
        }
    }
}
