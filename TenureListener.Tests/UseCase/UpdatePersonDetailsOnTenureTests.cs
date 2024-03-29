using AutoFixture;
using FluentAssertions;
using Hackney.Core.Testing.Shared;
using Hackney.Shared.Person.Boundary.Response;
using Hackney.Shared.Tenure.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenureListener.Boundary;
using TenureListener.Gateway.Interfaces;
using TenureListener.Infrastructure;
using TenureListener.Infrastructure.Exceptions;
using TenureListener.UseCase;
using Xunit;

namespace TenureListener.Tests.UseCase
{
    [Collection("LogCall collection")]
    public class UpdatePersonDetailsOnTenureTests
    {
        private readonly Mock<IPersonApi> _mockPersonApi;
        private readonly Mock<ITenureInfoGateway> _mockGateway;
        private readonly Mock<ILogger<UpdatePersonDetailsOnTenure>> _mockLogger;
        private readonly UpdatePersonDetailsOnTenure _sut;

        private readonly EntityEventSns _message;
        private readonly PersonResponseObject _person;
        private readonly TenureInformation _tenure;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();
        private const string DateTimeFormat = "yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ";

        public UpdatePersonDetailsOnTenureTests()
        {
            _fixture = new Fixture();

            _mockPersonApi = new Mock<IPersonApi>();
            _mockGateway = new Mock<ITenureInfoGateway>();
            _mockLogger = new Mock<ILogger<UpdatePersonDetailsOnTenure>>();
            _sut = new UpdatePersonDetailsOnTenure(_mockPersonApi.Object, _mockGateway.Object, _mockLogger.Object);

            _message = CreateMessage();
            _person = CreatePerson(_message.EntityId);
            _tenure = CreateTenure(_person.Tenures.First().Id, _person);
        }

        private PersonResponseObject CreatePerson(Guid entityId)
        {
            var tenures = _fixture.CreateMany<TenureResponseObject>(1);
            return _fixture.Build<PersonResponseObject>()
                           .With(x => x.Id, entityId)
                           .With(x => x.Tenures, tenures)
                           .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-30).ToString(DateTimeFormat))
                           .Create();
        }

        private TenureInformation CreateTenure(Guid entityId, PersonResponseObject person, bool isActive, bool isTenant)
        {
            var householdMembers = _fixture.Build<HouseholdMembers>()
                                           .With(x => x.Id, person.Id)
                                           .With(x => x.DateOfBirth, DateTime.Parse(person.DateOfBirth))
                                           .With(x => x.FullName, person.GetFullName())
                                           .With(x => x.PersonTenureType, isTenant ? PersonTenureType.Tenant : PersonTenureType.HouseholdMember)
                                           .CreateMany(1);
            return _fixture.Build<TenureInformation>()
                           .With(x => x.Id, entityId)
                           .With(x => x.HouseholdMembers, householdMembers)
                           .With(x => x.EndOfTenureDate, isActive ? DateTime.UtcNow.AddDays(10) : DateTime.UtcNow.AddDays(-10))
                           .Create();
        }

        private TenureInformation CreateTenure(Guid entityId, PersonResponseObject person)
        {
            return CreateTenure(entityId, person, true, true);
        }

        private EntityEventSns CreateMessage(string eventType = EventTypes.PersonUpdatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.CorrelationId, _correlationId)
                           .Create();
        }

        private bool VerifyUpdatedTenure(TenureInformation updated, PersonResponseObject person)
        {
            var hm = updated.HouseholdMembers.First(x => x.Id == person.Id);
            hm.FullName.Should().Be(person.GetFullName());
            hm.DateOfBirth.Should().Be(DateTime.Parse(person.DateOfBirth));
            return true;
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
        public async Task ProcessMessageAsyncTestPersonHasNullTenuresDoesNothing()
        {
            _person.Tenures = null;
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_person);

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockPersonApi.Verify(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId), Times.Once);
            _mockGateway.Verify(x => x.GetTenureInfoByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ProcessMessageAsyncTestPersonHasNoTenuresDoesNothing()
        {
            _person.Tenures = Enumerable.Empty<TenureResponseObject>();
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_person);

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockPersonApi.Verify(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId), Times.Once);
            _mockGateway.Verify(x => x.GetTenureInfoByIdAsync(It.IsAny<Guid>()), Times.Never);
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
        public async Task ProcessMessageAsyncTestGetTenureReturnsNullIngoresTenure()
        {
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_person);
            _mockGateway.Setup(x => x.GetTenureInfoByIdAsync(_person.Tenures.First().Id))
                        .ReturnsAsync((TenureInformation) null);

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockPersonApi.Verify(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId), Times.Once);
            _mockGateway.Verify(x => x.GetTenureInfoByIdAsync(It.IsAny<Guid>()), Times.Once);
            _mockLogger.VerifyAny(LogLevel.Warning, Times.Once());
            _mockGateway.Verify(x => x.UpdateTenureInfoAsync(It.IsAny<TenureInformation>()), Times.Never);
        }

        [Fact]
        public async Task ProcessMessageAsyncTestPersonNotAHouseholdMemberIngoresTenure()
        {
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                          .ReturnsAsync(_person);

            foreach (var hm in _tenure.HouseholdMembers)
                hm.Id = Guid.NewGuid();
            _mockGateway.Setup(x => x.GetTenureInfoByIdAsync(_person.Tenures.First().Id))
                        .ReturnsAsync(_tenure);

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockPersonApi.Verify(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId), Times.Once);
            _mockGateway.Verify(x => x.GetTenureInfoByIdAsync(It.IsAny<Guid>()), Times.Once);
            _mockLogger.VerifyAny(LogLevel.Warning, Times.Once());
            _mockGateway.Verify(x => x.UpdateTenureInfoAsync(It.IsAny<TenureInformation>()), Times.Never);
        }

        [Fact]
        public async Task ProcessMessageAsyncTestOneTenureNoChangesDoesNothing()
        {
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                          .ReturnsAsync(_person);
            _mockGateway.Setup(x => x.GetTenureInfoByIdAsync(_person.Tenures.First().Id))
                        .ReturnsAsync(_tenure);

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockPersonApi.Verify(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId), Times.Once);
            _mockGateway.Verify(x => x.GetTenureInfoByIdAsync(It.IsAny<Guid>()), Times.Once);
            _mockLogger.VerifyAny(LogLevel.Warning, Times.Never());
            _mockGateway.Verify(x => x.UpdateTenureInfoAsync(It.IsAny<TenureInformation>()), Times.Never);
        }

        [Fact]
        public void ProcessMessageAsyncTestOneTenureUpdateTenureExceptionThrows()
        {
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                          .ReturnsAsync(_person);

            _person.DateOfBirth = DateTime.UtcNow.ToString(DateTimeFormat);
            _person.FirstName = "Bob";
            _person.Surname = "Roberts";

            _mockGateway.Setup(x => x.GetTenureInfoByIdAsync(_person.Tenures.First().Id))
                        .ReturnsAsync(_tenure);
            var exMsg = "This is the last error";
            _mockGateway.Setup(x => x.UpdateTenureInfoAsync(It.IsAny<TenureInformation>()))
                        .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);

            _mockPersonApi.Verify(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId), Times.Once);
            _mockGateway.Verify(x => x.GetTenureInfoByIdAsync(It.IsAny<Guid>()), Times.Once);
            _mockLogger.VerifyAny(LogLevel.Warning, Times.Never());
            _mockGateway.Verify(x => x.UpdateTenureInfoAsync(It.Is<TenureInformation>(y => VerifyUpdatedTenure(y, _person))),
                                Times.Once);
        }

        [Fact]
        public async Task ProcessMessageAsyncTestOneTenureTenureUpdatedWithChanges()
        {
            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                          .ReturnsAsync(_person);

            _person.DateOfBirth = DateTime.UtcNow.ToString(DateTimeFormat);
            _person.FirstName = "Bob";
            _person.Surname = "Roberts";

            _mockGateway.Setup(x => x.GetTenureInfoByIdAsync(_person.Tenures.First().Id))
                        .ReturnsAsync(_tenure);

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockPersonApi.Verify(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId), Times.Once);
            _mockGateway.Verify(x => x.GetTenureInfoByIdAsync(It.IsAny<Guid>()), Times.Once);
            _mockLogger.VerifyAny(LogLevel.Warning, Times.Never());
            _mockGateway.Verify(x => x.UpdateTenureInfoAsync(It.Is<TenureInformation>(y => VerifyUpdatedTenure(y, _person))),
                                Times.Once);
        }

        [Fact]
        public async Task ProcessMessageAsyncTestManyTenuresUpdatedWithChanges()
        {
            int numTenures = 5;
            _person.Tenures = _fixture.CreateMany<TenureResponseObject>(numTenures);
            var tenures = new List<TenureInformation>();
            foreach (var personTenure in _person.Tenures)
            {
                var tenureInfo = CreateTenure(personTenure.Id, _person);
                tenures.Add(tenureInfo);
                _mockGateway.Setup(x => x.GetTenureInfoByIdAsync(personTenure.Id))
                            .ReturnsAsync(tenureInfo);
            }

            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                          .ReturnsAsync(_person);

            _person.DateOfBirth = DateTime.UtcNow.ToString(DateTimeFormat);
            _person.FirstName = "Bob";
            _person.Surname = "Roberts";


            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockPersonApi.Verify(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId), Times.Once);
            _mockGateway.Verify(x => x.GetTenureInfoByIdAsync(It.IsAny<Guid>()), Times.Exactly(numTenures));
            _mockLogger.VerifyAny(LogLevel.Warning, Times.Never());
            _mockGateway.Verify(x => x.UpdateTenureInfoAsync(It.Is<TenureInformation>(y => VerifyUpdatedTenure(y, _person))),
                                Times.Exactly(numTenures));
        }

        [Fact]
        public async Task PersonNameIsUpdatedOnAllActiveTenures()
        {
            int numActiveTenures = 5;
            _person.Tenures = _fixture.Build<TenureResponseObject>()
                                            .With(x => x.IsActive, true)
                                            .CreateMany(numActiveTenures);

            int numInactiveTenures = 2;
            _person.Tenures = _person.Tenures.Concat(_fixture.Build<TenureResponseObject>()
                                                             .With(x => x.IsActive, false)
                                                             .CreateMany(numInactiveTenures));

            foreach (var personTenure in _person.Tenures)
            {
                var tenureInfo = CreateTenure(personTenure.Id, _person, personTenure.IsActive, true);
                _mockGateway.Setup(x => x.GetTenureInfoByIdAsync(personTenure.Id))
                            .ReturnsAsync(tenureInfo);
            }

            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                          .ReturnsAsync(_person);

            _person.FirstName = "Bob";
            _person.Surname = "Roberts";

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockPersonApi.Verify(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId), Times.Once);
            _mockGateway.Verify(x => x.GetTenureInfoByIdAsync(It.IsAny<Guid>()), Times.Exactly(numActiveTenures + numInactiveTenures));
            _mockLogger.VerifyAny(LogLevel.Warning, Times.Never());
            _mockGateway.Verify(x => x.UpdateTenureInfoAsync(It.Is<TenureInformation>(y => VerifyUpdatedTenure(y, _person))),
                                Times.Exactly(numActiveTenures));
        }

        [Fact]
        public async Task PersonNameIsUpdatedOnInactiveTenuresWherePersonIsNotTenant()
        {
            int numTenantTenures = 5;
            int numNotTenantTenures = 2;
            _person.Tenures = _fixture.CreateMany<TenureResponseObject>(numTenantTenures + numNotTenantTenures);

            foreach (var item in _person.Tenures.Select((value, index) => new { index, value })) // getting index in foreach
            {
                var personTenure = item.value;
                var tenureInfo = CreateTenure(personTenure.Id, _person, false, (item.index < numTenantTenures));
                // person is tenant on first 5 tenures
                _mockGateway.Setup(x => x.GetTenureInfoByIdAsync(personTenure.Id))
                            .ReturnsAsync(tenureInfo);
            }

            _mockPersonApi.Setup(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId))
                          .ReturnsAsync(_person);

            _person.FirstName = "Bob";
            _person.Surname = "Roberts";

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockPersonApi.Verify(x => x.GetPersonByIdAsync(_message.EntityId, _message.CorrelationId), Times.Once);
            _mockGateway.Verify(x => x.GetTenureInfoByIdAsync(It.IsAny<Guid>()), Times.Exactly(numTenantTenures + numNotTenantTenures));
            _mockLogger.VerifyAny(LogLevel.Warning, Times.Never());
            _mockGateway.Verify(x => x.UpdateTenureInfoAsync(It.Is<TenureInformation>(y => VerifyUpdatedTenure(y, _person))),
                                Times.Exactly(numNotTenantTenures));
        }
    }
}
