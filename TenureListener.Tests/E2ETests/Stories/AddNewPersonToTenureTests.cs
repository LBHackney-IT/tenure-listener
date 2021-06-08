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

        [Fact]
        public void ListenerUpdatesTheTenure()
        {
            var personId = Guid.NewGuid();
            this.Given(g => _personApiFixture.GivenThePersonExists(personId))
                .And(h => _tenureFixture.GivenATenureAlreadyExists(PersonApiFixture.PersonResponse.Tenures.First().Id))
                .When(w => _steps.WhenTheFunctionIsTriggered(personId))
                .Then(t => _steps.ThenTheTenureisUpdatedWithTheUserDetails(
                                    PersonApiFixture.PersonResponse, _dbFixture.DynamoDbContext))
                .BDDfy();
        }
    }
}
