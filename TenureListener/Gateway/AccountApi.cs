using Hackney.Core.Http;
using Hackney.Core.Logging;
using System;
using System.Threading.Tasks;
using Polly;
using Polly.Registry;
using TenureListener.Domain.Account;
using TenureListener.Gateway.Interfaces;
using TenureListener.Infrastructure;

namespace TenureListener.Gateway
{
    public class AccountApi : IAccountApi
    {
        private const string ApiName = "Account";
        private const string AccountApiUrl = "AccountApiUrl";
        private const string AccountApiToken = "AccountApiToken";

        private readonly IApiGateway _apiGateway;
        private readonly IReadOnlyPolicyRegistry<string> _policyRegistry;

        public AccountApi(IApiGateway apiGateway, IReadOnlyPolicyRegistry<string> policyRegistry)
        {
            _apiGateway = apiGateway;
            _policyRegistry = policyRegistry;

            _apiGateway.Initialise(ApiName, AccountApiUrl, AccountApiToken);
        }

        [LogCall]
        public async Task<AccountResponseObject> GetAccountByIdAsync(Guid id, Guid correlationId)
        {
            var route = $"{_apiGateway.ApiRoute}/accounts/{id}";

            return await _policyRegistry
                .Get<IAsyncPolicy>(PolicyConstants.WaitAndRetry)
                .ExecuteAsync(() => _apiGateway.GetByIdAsync<AccountResponseObject>(route, id, correlationId));
        }
    }
}
