using AutoFixture;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using TenureListener.Boundary;
using TenureListener.Domain;
using TenureListener.Domain.Person;
using TenureListener.Gateway.Interfaces;
using TenureListener.Infrastructure.Exceptions;
using TenureListener.UseCase;
using Xunit;

namespace TenureListener.Tests.UseCase
{
    [Collection("LogCall collection")]
    public class AddNewPersonToTenureTests
    {
        private readonly Mock<IPersonApi> _mockPersonApi;
        private readonly Mock<ITenureInfoGateway> _mockGateway;
        private readonly AddNewPersonToTenure _sut;

        private readonly EntityEventSns _message;
        private readonly PersonResponseObject _person;
        private readonly TenureInformation _tenure;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();

        public AddNewPersonToTenureTests()
        {
            _fixture = new Fixture();

            _mockPersonApi = new Mock<IPersonApi>();
            _mockGateway = new Mock<ITenureInfoGateway>();
            _sut = new AddNewPersonToTenure(_mockPersonApi.Object, _mockGateway.Object);

            _message = CreateMessage();
            _person = CreatePerson(_message.EntityId);
            _tenure = CreateTenure(_person.Tenures.First().Id);
        }

        private PersonResponseObject CreatePerson(Guid entityId)
        {
            var tenures = _fixture.CreateMany<Tenure>(1);
            return _fixture.Build<PersonResponseObject>()
                           .With(x => x.Id, entityId)
                           .With(x => x.Tenures, tenures)
                           .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-30).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ"))
                           .Create();
        }

        private TenureInformation CreateTenure(Guid entityId)
        {
            return _fixture.Build<TenureInformation>()
                           .With(x => x.Id, entityId)
                           .Create();
        }

        private EntityEventSns CreateMessage(string eventType = EventTypes.PersonCreatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.CorrelationId, _correlationId)
                           .Create();
        }

        [Fact]
        public void ProcessMessageAsyncTestNullMessageThrows()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void ProcessMessageAsyncTestGetPersonExceptionThrown()
        {
            var exMsg = "This is an error";
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Fact]
        public void ProcessMessageAsyncTestGetPersonReturnsNullThrows()
        {
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync((PersonResponseObject) null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<PersonNotFoundException>();
        }

        [Fact]
        public void ProcessMessageAsyncTestPersonHasNoTenuresThrows()
        {
            _person.Tenures = Enumerable.Empty<Tenure>();
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_person);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<PersonHasNoTenuresException>();
        }

        [Fact]
        public void ProcessMessageAsyncTestGetTenureExceptionThrows()
        {
            var exMsg = "This is an new error";
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_person);
            _mockGateway.Setup(x => x.GetTenureInfoByIdAsync(_person.Tenures.First().Id))
                        .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Fact]
        public void ProcessMessageAsyncTestGetTenureReturnsNullExceptionThrows()
        {
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_person);
            _mockGateway.Setup(x => x.GetTenureInfoByIdAsync(_person.Tenures.First().Id))
                        .ReturnsAsync((TenureInformation) null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<TenureNotFoundException>();
        }

        [Fact]
        public void ProcessMessageAsyncTestUpdateTenureExceptionThrows()
        {
            var exMsg = "This is the last error";
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_person);
            _mockGateway.Setup(x => x.GetTenureInfoByIdAsync(_person.Tenures.First().Id))
                        .ReturnsAsync(_tenure);
            _mockGateway.Setup(x => x.UpdateTenureInfoAsync(It.IsAny<TenureInformation>()))
                        .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Theory]
        [InlineData(PersonType.Tenant, true)]
        [InlineData(PersonType.Tenant, false)]
        [InlineData(PersonType.HouseholdMember, true)]
        [InlineData(PersonType.HouseholdMember, false)]
        public async Task ProcessMessageAsyncTestSuccess(PersonType personType, bool nullableEnums)
        {
            _person.PersonTypes = new[] { personType };
            if (nullableEnums)
            {
                _person.Gender = null;
                _person.PreferredTitle = null;
            }

            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_person);
            _mockGateway.Setup(x => x.GetTenureInfoByIdAsync(_person.Tenures.First().Id))
                        .ReturnsAsync(_tenure);

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockGateway.Verify(x => x.UpdateTenureInfoAsync(It.Is<TenureInformation>(y => VerifyUpdatedTenure(y, _person))),
                                Times.Once);
        }

        private bool VerifyUpdatedTenure(TenureInformation updated, PersonResponseObject person)
        {
            var isResponsible = person.PersonTypes.First() == PersonType.Tenant;
            var expected = new HouseholdMembers()
            {
                Id = person.Id,
                Type = HouseholdMembersType.Person,
                FullName = person.FullName,
                DateOfBirth = DateTime.Parse(person.DateOfBirth),
                IsResponsible = isResponsible,
                PersonTenureType = updated.TenureType.GetPersonTenureType(isResponsible)
            };
            updated.HouseholdMembers.Last().Should().BeEquivalentTo(expected);
            return true;
        }
    }
}
