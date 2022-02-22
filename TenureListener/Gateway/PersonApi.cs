using Hackney.Core.Http;
using Hackney.Core.Logging;
using Hackney.Shared.Person.Boundary.Response;
using System;
using System.Threading.Tasks;
using Polly;
using Polly.Registry;
using TenureListener.Gateway.Interfaces;
using TenureListener.Infrastructure;

namespace TenureListener.Gateway
{
    public class PersonApi : IPersonApi
    {
        private const string ApiName = "Person";
        private const string PersonApiUrl = "PersonApiUrl";
        private const string PersonApiToken = "PersonApiToken";

        private readonly IApiGateway _apiGateway;
        private readonly IReadOnlyPolicyRegistry<string> _policyRegistry;

        public PersonApi(IApiGateway apiGateway, IReadOnlyPolicyRegistry<string> policyRegistry)
        {
            _apiGateway = apiGateway;
            _policyRegistry = policyRegistry;

            _apiGateway.Initialise(ApiName, PersonApiUrl, PersonApiToken);
        }

        [LogCall]
        public async Task<PersonResponseObject> GetPersonByIdAsync(Guid id, Guid correlationId)
        {
            var route = $"{_apiGateway.ApiRoute}/persons/{id}";

            return await _policyRegistry
                .Get<IAsyncPolicy>(PolicyConstants.WaitAndRetry)
                .ExecuteAsync(() => _apiGateway.GetByIdAsync<PersonResponseObject>(route, id, correlationId));
        }
    }
}
