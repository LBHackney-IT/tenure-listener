using System;
using System.Linq;
using TenureListener.Domain.Person;
using TenureListener.Tests.E2ETests.Fixtures;
using TenureListener.Tests.E2ETests.Steps;
using TestStack.BDDfy;
using Xunit;

namespace TenureListener.Tests.E2ETests.Stories
{
    [Story(
        AsA = "SQS Tenure Listener",
        IWant = "an function to process the person created message",
        SoThat = "The person details are set on the tenure")]
    [Collection("Aws collection")]
    public class AddNewPersonToTenureTests : IDisposable
    {
        private readonly AwsIntegrationTests _dbFixture;
        private readonly PersonApiFixture _personApiFixture;
        private readonly TenureFixture _tenureFixture;

        private readonly AddNewPersonToTenureSteps _steps;

        public AddNewPersonToTenureTests(AwsIntegrationTests dbFixture)
        {
            _dbFixture = dbFixture;

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
        [InlineData(PersonType.Tenant)]
        [InlineData(PersonType.HouseholdMember)]
        public void ListenerUpdatesTheTenure(PersonType personType)
        {
            var personId = Guid.NewGuid();
            this.Given(g => _personApiFixture.GivenThePersonExists(personId, personType))
                .And(h => _tenureFixture.GivenATenureAlreadyExists(PersonApiFixture.PersonResponse.Tenures.First().Id))
                .When(w => _steps.WhenTheFunctionIsTriggered(personId))
                .Then(t => _steps.ThenTheTenureIsUpdatedWithTheUserDetails(
                                    PersonApiFixture.PersonResponse, _dbFixture.DynamoDbContext))
                .BDDfy();
        }

        [Fact]
        public void UpdatedPersonNotFound()
        {
            var personId = Guid.NewGuid();
            this.Given(g => _personApiFixture.GivenThePersonDoesNotExist(personId))
                .When(w => _steps.WhenTheFunctionIsTriggered(personId))
                .Then(t => _steps.ThenAPersonNotFoundExceptionIsThrown(personId))
                .BDDfy();
        }

        [Fact]
        public void UpdatedPersonHasNoTenures()
        {
            var personId = Guid.NewGuid();
            this.Given(g => _personApiFixture.GivenThePersonExists(personId, false))
                .When(w => _steps.WhenTheFunctionIsTriggered(personId))
                .Then(t => _steps.ThenAPersonHasNoTenuresExceptionIsThrown(personId))
                .BDDfy();
        }

        [Fact]
        public void TenureNotFound()
        {
            var personId = Guid.NewGuid();
            this.Given(g => _personApiFixture.GivenThePersonExists(personId))
                .And(h => _tenureFixture.GivenATenureDoesNotExist(PersonApiFixture.PersonResponse.Tenures.First().Id))
                .When(w => _steps.WhenTheFunctionIsTriggered(personId))
                .Then(t => _steps.ThenATenureNotFoundExceptionIsThrown(PersonApiFixture.PersonResponse.Tenures.First().Id))
                .BDDfy();
        }
    }
}
