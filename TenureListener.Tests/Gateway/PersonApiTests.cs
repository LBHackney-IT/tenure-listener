using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TenureListener.Domain.Person;
using TenureListener.Gateway;
using TenureListener.Infrastructure.Exceptions;
using Xunit;

namespace TenureListener.Tests.Gateway
{
    [Collection("LogCall collection")]
    public class PersonApiTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly PersonApi _sut;
        private IConfiguration _configuration;
        private readonly static JsonSerializerOptions _jsonOptions = CreateJsonOptions();

        private const string PersonApiRoute = "https://some-domain.com/api/";
        private const string PersonApiToken = "dksfghjskueygfakseygfaskjgfsdjkgfdkjsgfdkjgf";

        public PersonApiTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                                  .Returns(_httpClient);

            var inMemorySettings = new Dictionary<string, string> {
                { "GetPersonApi", PersonApiRoute },
                { "GetPersonApiToken", PersonApiToken }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _sut = new PersonApi(_mockHttpClientFactory.Object, _configuration);
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

        private static string Route(Guid id) => $"{PersonApiRoute}persons/{id}";

        private static bool ValidateRequest(string expectedRoute, HttpRequestMessage request)
        {
            return (request.RequestUri.ToString() == expectedRoute)
                && (request.Headers.Authorization.ToString() == PersonApiToken);
        }

        private void SetupHttpClientResponse(string route, PersonResponseObject response)
        {
            HttpStatusCode statusCode = (response is null) ?
                HttpStatusCode.NotFound : HttpStatusCode.OK;
            HttpContent content = (response is null) ?
                null : new StringContent(JsonSerializer.Serialize(response, _jsonOptions));
            _mockHttpMessageHandler.Protected()
                   .Setup<Task<HttpResponseMessage>>("SendAsync",
                        ItExpr.Is<HttpRequestMessage>(y => ValidateRequest(route, y)),
                        ItExpr.IsAny<CancellationToken>())
                   .ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = statusCode,
                       Content = content,
                   });
        }

        private void SetupHttpClientErrorResponse(string route, string response)
        {
            HttpContent content = (response is null) ? null : new StringContent(response);
            _mockHttpMessageHandler.Protected()
                   .Setup<Task<HttpResponseMessage>>("SendAsync",
                        ItExpr.Is<HttpRequestMessage>(y => y.RequestUri.ToString() == route),
                        ItExpr.IsAny<CancellationToken>())
                   .ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.InternalServerError,
                       Content = content,
                   });
        }

        private void SetupHttpClientException(string route, Exception ex)
        {
            _mockHttpMessageHandler.Protected()
                   .Setup<Task<HttpResponseMessage>>("SendAsync",
                        ItExpr.Is<HttpRequestMessage>(y => y.RequestUri.ToString() == route),
                        ItExpr.IsAny<CancellationToken>())
                   .ThrowsAsync(ex);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("sdrtgdfstg")]
        public void ConstructorTestInvalidRouteConfigThrows(string invalidValue)
        {
            var inMemorySettings = new Dictionary<string, string> {
                { "GetPersonApi", invalidValue }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            Action act = () => _ = new PersonApi(_mockHttpClientFactory.Object, _configuration);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ConstructorTestInvalidTokenConfigThrows(string invalidValue)
        {
            var inMemorySettings = new Dictionary<string, string> {
                { "GetPersonApi", PersonApiRoute },
                { "GetPersonApiToken", invalidValue }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            Action act = () => _ = new PersonApi(_mockHttpClientFactory.Object, _configuration);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetPersonByIdAsyncGetExceptionThrown()
        {
            var id = Guid.NewGuid();
            var exMessage = "This is an exception";
            SetupHttpClientException(Route(id), new Exception(exMessage));

            Func<Task<PersonResponseObject>> func =
                async () => await _sut.GetPersonByIdAsync(id).ConfigureAwait(false);

            func.Should().ThrowAsync<Exception>().WithMessage(exMessage);
        }

        [Fact]
        public void GetPersonByIdAsyncCallFailedExceptionThrown()
        {
            var id = Guid.NewGuid();
            var error = "This is an error message";
            SetupHttpClientErrorResponse(Route(id), error);

            Func<Task<PersonResponseObject>> func =
                async () => await _sut.GetPersonByIdAsync(id).ConfigureAwait(false);

            func.Should().ThrowAsync<GetPersonException>()
                         .WithMessage($"Failed to get person details for id {id}. " +
                         $"Status code: {HttpStatusCode.InternalServerError}; Message: {error}");
        }

        [Fact]
        public async Task GetPersonByIdAsyncNotFoundReturnsNull()
        {
            var id = Guid.NewGuid();
            SetupHttpClientResponse(Route(id), null);

            var result = await _sut.GetPersonByIdAsync(id).ConfigureAwait(false);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetPersonByIdAsyncCallReturnsPerson()
        {
            var id = Guid.NewGuid();
            var person = new Fixture().Create<PersonResponseObject>();
            SetupHttpClientResponse(Route(id), person);

            var result = await _sut.GetPersonByIdAsync(id).ConfigureAwait(false);

            result.Should().BeEquivalentTo(person);
        }
    }
}
