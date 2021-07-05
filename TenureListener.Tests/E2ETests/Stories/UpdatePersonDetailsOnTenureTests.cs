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
        IWant = "a function to process the person updated message",
        SoThat = "The person details are updated on the tenure")]
    [Collection("Aws collection")]
    public class UpdatePersonDetailsOnTenureTests : IDisposable
    {
        private readonly AwsIntegrationTests _dbFixture;
        private readonly PersonApiFixture _personApiFixture;
        private readonly TenureFixture _tenureFixture;

        private readonly UpdatePersonDetailsOnTenureSteps _steps;

        public UpdatePersonDetailsOnTenureTests(AwsIntegrationTests dbFixture)
        {
            _dbFixture = dbFixture;

            _personApiFixture = new PersonApiFixture();
            _tenureFixture = new TenureFixture(_dbFixture.DynamoDbContext);

            _steps = new UpdatePersonDetailsOnTenureSteps();
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
            this.Given(g => _personApiFixture.GivenThePersonExists(personId))
                .And(h => _tenureFixture.GivenATenureAlreadyExists(PersonApiFixture.PersonResponse.Tenures.First().Id))
                .When(w => _steps.WhenTheFunctionIsTriggered(personId))
                .Then(t => _steps.ThenNoChangesAreMade(_dbFixture.DynamoDbContext, _tenureFixture.Tenure))
                .BDDfy();
        }

        [Fact]
        public void UpdatedPersonNotAmongTenuresHouseholdMembers()
        {
            var personId = Guid.NewGuid();
            this.Given(g => _personApiFixture.GivenThePersonExistsWithMultipleTenures(personId, 5))
                .And(h => _tenureFixture.GivenTenuresAlreadyExist(PersonApiFixture.PersonResponse, false))
                .When(w => _steps.WhenTheFunctionIsTriggered(personId))
                .Then(t => _steps.ThenNoChangesAreMade(_dbFixture.DynamoDbContext, _tenureFixture.Tenures.ToArray()))
                .BDDfy();
        }

        [Fact]
        public void UpdatedPersonTenuresUpdated()
        {
            var personId = Guid.NewGuid();
            this.Given(g => _personApiFixture.GivenThePersonExistsWithMultipleTenures(personId, 5))
                .And(h => _tenureFixture.GivenTenuresAlreadyExist(PersonApiFixture.PersonResponse, true))
                .When(w => _steps.WhenTheFunctionIsTriggered(personId))
                .Then(t => _steps.ThenAllTenuresAreUpdated(_dbFixture.DynamoDbContext, PersonApiFixture.PersonResponse))
                .BDDfy();
        }
    }
}
