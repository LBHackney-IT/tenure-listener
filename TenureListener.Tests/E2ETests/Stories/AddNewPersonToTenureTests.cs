using Hackney.Core.Testing.DynamoDb;
using Hackney.Shared.Person.Domain;
using System;
using System.Linq;
using TenureListener.Tests.E2ETests.Fixtures;
using TenureListener.Tests.E2ETests.Steps;
using TestStack.BDDfy;
using Xunit;

namespace TenureListener.Tests.E2ETests.Stories
{
    [Story(
        AsA = "SQS Tenure Listener",
        IWant = "a function to process the person created message",
        SoThat = "The person details are set on the tenure")]
    [Collection("AppTest collection")]
    public class AddNewPersonToTenureTests : IDisposable
    {
        private readonly IDynamoDbFixture _dbFixture;
        private readonly PersonApiFixture _personApiFixture;
        private readonly TenureFixture _tenureFixture;

        private readonly AddNewPersonToTenureSteps _steps;

        public AddNewPersonToTenureTests(MockApplicationFactory appFactory)
        {
            _dbFixture = appFactory.DynamoDbFixture;

            _personApiFixture = new PersonApiFixture();
            _tenureFixture = new TenureFixture(_dbFixture.DynamoDbContext);

            _steps = new AddNewPersonToTenureSteps();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _personApiFixture.Dispose();
                _tenureFixture.Dispose();

                _disposed = true;
            }
        }

        [Theory]
        [InlineData(PersonType.Tenant, false)]
        [InlineData(PersonType.Tenant, true)]
        [InlineData(PersonType.HouseholdMember, false)]
        [InlineData(PersonType.HouseholdMember, true)]
        public void ListenerUpdatesTheTenure(PersonType personType, bool nullTenuredAssetType)
        {
            var personId = Guid.NewGuid();
            this.Given(g => _personApiFixture.GivenThePersonExists(personId, personType))
                .And(h => _tenureFixture.GivenATenureAlreadyExists(_personApiFixture.ResponseObject.Tenures.First().Id, nullTenuredAssetType))
                .When(w => _steps.WhenTheFunctionIsTriggered(personId))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_personApiFixture.ReceivedCorrelationIds.First()))
                .Then(t => _steps.ThenTheTenureIsUpdatedWithTheUserDetails(
                                    _personApiFixture.ResponseObject, _dbFixture.DynamoDbContext))
                .BDDfy();
        }

        [Fact]
        public void UpdatedPersonNotFound()
        {
            var personId = Guid.NewGuid();
            this.Given(g => _personApiFixture.GivenThePersonDoesNotExist(personId))
                .When(w => _steps.WhenTheFunctionIsTriggered(personId))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_personApiFixture.ReceivedCorrelationIds.First()))
                .Then(t => _steps.ThenAPersonNotFoundExceptionIsThrown(personId))
                .BDDfy();
        }

        [Fact]
        public void UpdatedPersonHasNoTenures()
        {
            var personId = Guid.NewGuid();
            this.Given(g => _personApiFixture.GivenThePersonExists(personId, false))
                .When(w => _steps.WhenTheFunctionIsTriggered(personId))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_personApiFixture.ReceivedCorrelationIds.First()))
                .Then(t => _steps.ThenAPersonHasNoTenuresExceptionIsThrown(personId))
                .BDDfy();
        }

        [Fact]
        public void TenureNotFound()
        {
            var personId = Guid.NewGuid();
            this.Given(g => _personApiFixture.GivenThePersonExists(personId))
                .And(h => _tenureFixture.GivenATenureDoesNotExist(_personApiFixture.ResponseObject.Tenures.First().Id))
                .When(w => _steps.WhenTheFunctionIsTriggered(personId))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_personApiFixture.ReceivedCorrelationIds.First()))
                .Then(t => _steps.ThenATenureNotFoundExceptionIsThrown(_personApiFixture.ResponseObject.Tenures.First().Id))
                .BDDfy();
        }

        [Fact]
        public void ListenerDoesNothingForV2Message()
        {
            var personId = Guid.NewGuid();
            this.Given(g => _personApiFixture.GivenThePersonExists(personId, PersonType.Tenant))
                .And(h => _tenureFixture.GivenATenureAlreadyExists(_personApiFixture.ResponseObject.Tenures.First().Id, false))
                .When(w => _steps.WhenTheFunctionIsTriggered(personId, EventVersions.V2))
                .Then(t => _steps.ThenTheEventIsIgnored(_personApiFixture.CallsMade, _tenureFixture.Tenure, _dbFixture.DynamoDbContext))
                .BDDfy();
        }
    }
}
