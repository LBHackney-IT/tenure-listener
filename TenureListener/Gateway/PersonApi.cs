using Hackney.Core.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TenureListener.Domain.Person;
using TenureListener.Gateway.Interfaces;
using TenureListener.Infrastructure.Exceptions;

namespace TenureListener.Gateway
{
    public class PersonApi : IPersonApi
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _getPersonApiRoute;
        private readonly string _getPersonApiToken;

        private const string PersonApiRouteKey = "GetPersonApi";
        private const string PersonApiToken = "GetPersonApiToken";
        private readonly static JsonSerializerOptions _jsonOptions = CreateJsonOptions();

        public PersonApi(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _getPersonApiRoute = configuration.GetValue<string>(PersonApiRouteKey)?.TrimEnd('/');
            if (string.IsNullOrEmpty(_getPersonApiRoute) || !Uri.IsWellFormedUriString(_getPersonApiRoute, UriKind.Absolute))
                throw new ArgumentException($"Configuration does not contain a setting value for the key {PersonApiRouteKey}.");

            _getPersonApiToken = configuration.GetValue<string>(PersonApiToken);
            if (string.IsNullOrEmpty(_getPersonApiToken))
                throw new ArgumentException($"Configuration does not contain a setting value for the key {PersonApiToken}.");
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        [LogCall]
        public async Task<PersonResponseObject> GetPersonByIdAsync(Guid id)
        {
            var client = _httpClientFactory.CreateClient();
            var getPersonRoute = $"{_getPersonApiRoute}/persons/{id}";

            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(_getPersonApiToken);
            var response = await client.GetAsync(new Uri(getPersonRoute))
                                       .ConfigureAwait(false);

            if (response.StatusCode is HttpStatusCode.NotFound)
                return null;

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return JsonSerializer.Deserialize<PersonResponseObject>(responseBody, _jsonOptions);

            throw new GetPersonException(id, response.StatusCode, responseBody);
        }
    }
}
