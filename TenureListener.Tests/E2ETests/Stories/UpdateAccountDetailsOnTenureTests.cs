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
        IWant = "a function to process the account created message",
        SoThat = "The account details are updated on the tenure")]
    [Collection("Aws collection")]
    public class UpdateAccountDetailsOnTenureTests : IDisposable
    {
        private readonly AwsIntegrationTests _dbFixture;
        private readonly AccountApiFixture _accountApiFixture;
        private readonly TenureFixture _tenureFixture;

        private readonly UpdateAccountDetailsOnTenureSteps _steps;

        public UpdateAccountDetailsOnTenureTests(AwsIntegrationTests dbFixture)
        {
            _dbFixture = dbFixture;

            _accountApiFixture = new AccountApiFixture();
            _tenureFixture = new TenureFixture(_dbFixture.DynamoDbContext);

            _steps = new UpdateAccountDetailsOnTenureSteps();
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
                _accountApiFixture.Dispose();
                _tenureFixture.Dispose();

                _disposed = true;
            }
        }

        [Fact]
        public void AccountNotFound()
        {
            var accountId = Guid.NewGuid();
            this.Given(g => _accountApiFixture.GivenTheAccountDoesNotExist(accountId))
                .When(w => _steps.WhenTheFunctionIsTriggered(accountId))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_accountApiFixture.ReceivedCorrelationIds.First()))
                .Then(t => _steps.ThenAnAccountNotFoundExceptionIsThrown(accountId))
                .BDDfy();
        }

        [Fact]
        public void AccountHasNoTenure()
        {
            var accountId = Guid.NewGuid();
            this.Given(g => _accountApiFixture.GivenTheAccountExistsWithNoTenure(accountId))
                .When(w => _steps.WhenTheFunctionIsTriggered(accountId))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_accountApiFixture.ReceivedCorrelationIds.First()))
                .BDDfy();
        }

        [Fact]
        public void AccountPaymentReferenceSameAsTenures()
        {
            var accountId = Guid.NewGuid();
            this.Given(g => _accountApiFixture.GivenTheAccountExists(accountId))
                .And(h => _tenureFixture.GivenATenureAlreadyExistsWithPaymentRef(_accountApiFixture.ResponseObject.TargetId,
                                                                                 _accountApiFixture.ResponseObject.PaymentReference))
                .When(w => _steps.WhenTheFunctionIsTriggered(accountId))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_accountApiFixture.ReceivedCorrelationIds.First()))
                .Then(t => _steps.ThenNoChangesAreMade(_dbFixture.DynamoDbContext, _tenureFixture.Tenure))
                .BDDfy();
        }

        [Fact]
        public void TenureUpdatedWithAccountDetails()
        {
            var accountId = Guid.NewGuid();
            this.Given(g => _accountApiFixture.GivenTheAccountExists(accountId))
                .And(h => _tenureFixture.GivenATenureAlreadyExists(_accountApiFixture.ResponseObject.TargetId))
                .When(w => _steps.WhenTheFunctionIsTriggered(accountId))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_accountApiFixture.ReceivedCorrelationIds.First()))
                .Then(t => _steps.ThenTheTenureIsUpdatedWithTheAccountDetails(_accountApiFixture.ResponseObject, _dbFixture.DynamoDbContext))
                .BDDfy();
        }
    }
}
