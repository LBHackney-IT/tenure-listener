using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using AutoFixture;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TenureListener.Boundary;
using TenureListener.Domain;
using TenureListener.Domain.Person;
using TenureListener.Infrastructure;

namespace TenureListener.Tests.E2ETests.Steps
{
    public class AddNewPersonToTenureSteps : BaseSteps
    {
        private readonly Fixture _fixture = new Fixture();

        public AddNewPersonToTenureSteps()
        { }

        private SQSEvent.SQSMessage CreateMessage(Guid personId, string eventType = "PersonCreatedEvent")
        {
            var personSns = _fixture.Build<PersonSns>()
                                    .With(x => x.EntityId, personId)
                                    .With(x => x.EventType, eventType)
                                    .Create();

            var msgBody = JsonSerializer.Serialize(personSns, _jsonOptions);
            return _fixture.Build<SQSEvent.SQSMessage>()
                           .With(x => x.Body, msgBody)
                           .With(x => x.MessageAttributes, new Dictionary<string, SQSEvent.MessageAttribute>())
                           .Create();
        }

        public async Task WhenTheFunctionIsTriggered(Guid personId)
        {
            var mockLambdaLogger = new Mock<ILambdaLogger>();
            ILambdaContext lambdaContext = new TestLambdaContext()
            {
                Logger = mockLambdaLogger.Object
            };

            var sqsEvent = _fixture.Build<SQSEvent>()
                                   .With(x => x.Records, new List<SQSEvent.SQSMessage> { CreateMessage(personId) })
                                   .Create();

            var fn = new SqsFunction();
            await fn.FunctionHandler(sqsEvent, lambdaContext);
        }

        public async Task ThenTheTenureIsUpdatedWithTheUserDetails(
            PersonResponseObject personResponse, IDynamoDBContext dbContext)
        {
            var tenureId = personResponse.Tenures.First().Id;
            var tenureInfo = await dbContext.LoadAsync<TenureInformationDb>(tenureId);

            var lastHouseholdMember = tenureInfo.HouseholdMembers.Last();
            lastHouseholdMember.Id.Should().Be(personResponse.Id);
            lastHouseholdMember.FullName.Should().Be(personResponse.FullName);
            lastHouseholdMember.Type.Should().Be(HouseholdMembersType.Person);
        }
    }
}
