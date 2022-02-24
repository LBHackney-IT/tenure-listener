using AutoFixture;
using FluentAssertions;
using Hackney.Core.Http;
using Hackney.Shared.Person.Boundary.Response;
using Moq;
using System;
using System.Threading.Tasks;
using Polly;
using Polly.Registry;
using TenureListener.Gateway;
using TenureListener.Infrastructure;
using Xunit;

namespace TenureListener.Tests.Gateway
{
    [Collection("LogCall collection")]
    public class PersonApiTests
    {
        private readonly Mock<IApiGateway> _mockApiGateway;
        private readonly PolicyRegistry _mockPolicyRegistry;

        private static readonly Guid _id = Guid.NewGuid();
        private static readonly Guid _correlationId = Guid.NewGuid();

        private const string ApiName = "Person";
        private const string PersonApiUrlKey = "PersonApiUrl";
        private const string PersonApiTokenKey = "PersonApiToken";

        private const string PersonApiRoute = "https://some-domain.com/api/";
        private const string PersonApiToken = "dksfghjskueygfakseygfaskjgfsdjkgfdkjsgfdkjgf";

        public PersonApiTests()
        {
            _mockApiGateway = new Mock<IApiGateway>();

            _mockApiGateway.SetupGet(x => x.ApiName).Returns(ApiName);
            _mockApiGateway.SetupGet(x => x.ApiRoute).Returns(PersonApiRoute);
            _mockApiGateway.SetupGet(x => x.ApiToken).Returns(PersonApiToken);

            _mockPolicyRegistry = new PolicyRegistry
            {
                { PolicyConstants.WaitAndRetry, Policy.NoOpAsync() }
            };
        }

        private static string Route => $"{PersonApiRoute}/persons/{_id}";

        [Fact]
        public void ConstructorTestInitialisesApiGateway()
        {
            new PersonApi(_mockApiGateway.Object, _mockPolicyRegistry);
            _mockApiGateway.Verify(x => x.Initialise(ApiName, PersonApiUrlKey, PersonApiTokenKey, null),
                                   Times.Once);
        }

        [Fact]
        public void GetPersonByIdAsyncGetExceptionThrown()
        {
            var exMessage = "This is an exception";
            _mockApiGateway.Setup(x => x.GetByIdAsync<PersonResponseObject>(Route, _id, _correlationId))
                           .ThrowsAsync(new Exception(exMessage));

            var sut = new PersonApi(_mockApiGateway.Object, _mockPolicyRegistry);
            Func<Task<PersonResponseObject>> func =
                async () => await sut.GetPersonByIdAsync(_id, _correlationId).ConfigureAwait(false);

            func.Should().ThrowAsync<Exception>().WithMessage(exMessage);
        }

        [Fact]
        public async Task GetPersonByIdAsyncNotFoundReturnsNull()
        {
            var sut = new PersonApi(_mockApiGateway.Object, _mockPolicyRegistry);
            var result = await sut.GetPersonByIdAsync(_id, _correlationId).ConfigureAwait(false);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetPersonByIdAsyncCallReturnsPerson()
        {
            var person = new Fixture().Create<PersonResponseObject>();

            _mockApiGateway.Setup(x => x.GetByIdAsync<PersonResponseObject>(Route, _id, _correlationId))
                           .ReturnsAsync(person);

            var sut = new PersonApi(_mockApiGateway.Object, _mockPolicyRegistry);
            var result = await sut.GetPersonByIdAsync(_id, _correlationId).ConfigureAwait(false);

            result.Should().BeEquivalentTo(person);
        }

        [Fact]
        public async Task GetAccountByIdUsesWaitAndRetryPolicy()
        {
            var mockPolicy = new Mock<IAsyncPolicy>();
            _mockPolicyRegistry[PolicyConstants.WaitAndRetry] = mockPolicy.Object;

            var sut = new PersonApi(_mockApiGateway.Object, _mockPolicyRegistry);
            await sut.GetPersonByIdAsync(_id, _correlationId).ConfigureAwait(false);

            mockPolicy.Verify(policy => policy.ExecuteAsync(It.IsAny<Func<Task<PersonResponseObject>>>()));
        }
    }
}
