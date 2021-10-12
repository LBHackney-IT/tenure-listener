using AutoFixture;
using Hackney.Core.Testing.Shared.E2E;
using System;
using TenureListener.Domain.Account;

namespace TenureListener.Tests.E2ETests.Fixtures
{
    public class AccountApiFixture : BaseApiFixture<AccountResponseObject>
    {
        private readonly Fixture _fixture = new Fixture();
        public static string AccountApiRoute => "http://localhost:5678/api/v1/";
        public static string AccountApiToken => "sdjkhfgsdkjfgsdjfgh";

        public AccountApiFixture()
            : base(AccountApiRoute, AccountApiToken)
        {
            Environment.SetEnvironmentVariable("AccountApiUrl", ApiRoute);
            Environment.SetEnvironmentVariable("AccountApiToken", ApiToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                base.Dispose(disposing);
            }
        }

        private AccountResponseObject ConstructAccountResponseObject(Guid id)
        {
            return _fixture.Build<AccountResponseObject>()
                                                 .With(x => x.Id, id)
                                                 .With(x => x.StartDate, DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ"))
                                                 .With(x => x.EndDate, "")
                                                 .With(x => x.LastUpdated, DateTime.UtcNow.AddMinutes(-60).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ"))
                                                 .Create();
        }

        public void GivenTheAccountDoesNotExist(Guid id)
        {
            // Nothing to do here
        }

        public AccountResponseObject GivenTheAccountExists(Guid id)
        {
            ResponseObject = ConstructAccountResponseObject(id);

            return ResponseObject;
        }

        public AccountResponseObject GivenTheAccountExistsWithTenure(Guid id, Guid tenureId)
        {
            ResponseObject = ConstructAccountResponseObject(id);
            ResponseObject.Tenure.TenancyId = tenureId;

            return ResponseObject;
        }

        public AccountResponseObject GivenTheAccountExistsWithNoTenure(Guid id)
        {
            ResponseObject = ConstructAccountResponseObject(id);
            ResponseObject.Tenure = null;

            return ResponseObject;
        }
    }
}
