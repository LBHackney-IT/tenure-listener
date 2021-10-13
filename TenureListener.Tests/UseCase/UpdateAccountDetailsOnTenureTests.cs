using AutoFixture;
using FluentAssertions;
using Hackney.Shared.Person.Boundary.Response;
using Hackney.Shared.Tenure.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenureListener.Boundary;
using TenureListener.Domain.Account;
using TenureListener.Gateway.Interfaces;
using TenureListener.Infrastructure;
using TenureListener.Infrastructure.Exceptions;
using TenureListener.UseCase;
using Xunit;

namespace TenureListener.Tests.UseCase
{
    [Collection("LogCall collection")]
    public class UpdateAccountDetailsOnTenureTests
    {
        private readonly Mock<IAccountApi> _mockAccountApi;
        private readonly Mock<ITenureInfoGateway> _mockGateway;
        private readonly UpdateAccountDetailsOnTenure _sut;

        private readonly EntityEventSns _message;
        private readonly AccountResponseObject _account;
        private readonly TenureInformation _tenure;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();
        private const string DateTimeFormat = "yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ";

        public UpdateAccountDetailsOnTenureTests()
        {
            _fixture = new Fixture();

            _mockAccountApi = new Mock<IAccountApi>();
            _mockGateway = new Mock<ITenureInfoGateway>();
            _sut = new UpdateAccountDetailsOnTenure(_mockAccountApi.Object, _mockGateway.Object);

            _message = CreateMessage();
            _account = CreateAccount(_message.EntityId);
            _tenure = CreateTenure(_account.Tenure.TenancyId);
        }

        private AccountResponseObject CreateAccount(Guid entityId)
        {
            return _fixture.Build<AccountResponseObject>()
                           .With(x => x.Id, entityId)
                           .With(x => x.StartDate, DateTime.UtcNow.AddMonths(-1).ToString(DateTimeFormat))
                           .Create();
        }

        private TenureInformation CreateTenure(Guid entityId)
        {
            return _fixture.Build<TenureInformation>()
                           .With(x => x.Id, entityId)
                           .Create();
        }

        private EntityEventSns CreateMessage(string eventType = EventTypes.AccountCreatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.CorrelationId, _correlationId)
                           .Create();
        }

        private bool VerifyUpdatedTenure(TenureInformation updated, AccountResponseObject account)
        {
            updated.PaymentReference.Should().Be(account.PaymentReference);
            return true;
        }

        [Fact]
        public void ProcessMessageAsyncTestNullMessageThrows()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void ProcessMessageAsyncTestGetAccountExceptionThrown()
        {
            var exMsg = "This is an error";
            _mockAccountApi.Setup(x => x.GetAccountByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Fact]
        public void ProcessMessageAsyncTestGetAccountReturnsNullThrows()
        {
            _mockAccountApi.Setup(x => x.GetAccountByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync((AccountResponseObject) null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<AccountNotFoundException>();
        }

        [Fact]
        public async Task ProcessMessageAsyncTestAccountHasNullTenureDoesNothing()
        {
            _account.Tenure = null;
            _mockAccountApi.Setup(x => x.GetAccountByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_account);

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockAccountApi.Verify(x => x.GetAccountByIdAsync(_message.EntityId, _message.CorrelationId), Times.Once);
            _mockGateway.Verify(x => x.GetTenureInfoByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public void ProcessMessageAsyncTestGetTenureExceptionThrows()
        {
            var exMsg = "This is an new error";
            _mockAccountApi.Setup(x => x.GetAccountByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_account);
            _mockGateway.Setup(x => x.GetTenureInfoByIdAsync(_account.Tenure.TenancyId))
                        .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Fact]
        public void ProcessMessageAsyncTestGetTenureReturnsNullThrows()
        {
            _mockAccountApi.Setup(x => x.GetAccountByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_account);
            _mockGateway.Setup(x => x.GetTenureInfoByIdAsync(_account.Tenure.TenancyId))
                        .ReturnsAsync((TenureInformation) null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<TenureNotFoundException>();

            _mockAccountApi.Verify(x => x.GetAccountByIdAsync(_message.EntityId, _message.CorrelationId), Times.Once);
            _mockGateway.Verify(x => x.GetTenureInfoByIdAsync(_account.Tenure.TenancyId), Times.Once);
        }

        [Fact]
        public async Task ProcessMessageAsyncTestNoChangesDoesNothing()
        {
            _mockAccountApi.Setup(x => x.GetAccountByIdAsync(_message.EntityId, _message.CorrelationId))
                          .ReturnsAsync(_account);
            _mockGateway.Setup(x => x.GetTenureInfoByIdAsync(_account.Tenure.TenancyId))
                        .ReturnsAsync(_tenure);
            _tenure.PaymentReference = _account.PaymentReference;

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockAccountApi.Verify(x => x.GetAccountByIdAsync(_message.EntityId, _message.CorrelationId), Times.Once);
            _mockGateway.Verify(x => x.GetTenureInfoByIdAsync(_account.Tenure.TenancyId), Times.Once);
            _mockGateway.Verify(x => x.UpdateTenureInfoAsync(It.IsAny<TenureInformation>()), Times.Never);
        }

        [Fact]
        public void ProcessMessageAsyncTestTenureUpdateTenureExceptionThrows()
        {
            _mockAccountApi.Setup(x => x.GetAccountByIdAsync(_message.EntityId, _message.CorrelationId))
                          .ReturnsAsync(_account);
            _mockGateway.Setup(x => x.GetTenureInfoByIdAsync(_account.Tenure.TenancyId))
                        .ReturnsAsync(_tenure);
            var exMsg = "This is the last error";
            _mockGateway.Setup(x => x.UpdateTenureInfoAsync(It.IsAny<TenureInformation>()))
                        .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);

            _mockAccountApi.Verify(x => x.GetAccountByIdAsync(_message.EntityId, _message.CorrelationId), Times.Once);
            _mockGateway.Verify(x => x.GetTenureInfoByIdAsync(_account.Tenure.TenancyId), Times.Once);
            _mockGateway.Verify(x => x.UpdateTenureInfoAsync(It.Is<TenureInformation>(y => VerifyUpdatedTenure(y, _account))),
                                Times.Once);
        }

        [Fact]
        public async Task ProcessMessageAsyncTestTenureUpdatedWithChanges()
        {
            _mockAccountApi.Setup(x => x.GetAccountByIdAsync(_message.EntityId, _message.CorrelationId))
                          .ReturnsAsync(_account);
            _mockGateway.Setup(x => x.GetTenureInfoByIdAsync(_account.Tenure.TenancyId))
                        .ReturnsAsync(_tenure);

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            _mockAccountApi.Verify(x => x.GetAccountByIdAsync(_message.EntityId, _message.CorrelationId), Times.Once);
            _mockGateway.Verify(x => x.GetTenureInfoByIdAsync(_account.Tenure.TenancyId), Times.Once);
            _mockGateway.Verify(x => x.UpdateTenureInfoAsync(It.Is<TenureInformation>(y => VerifyUpdatedTenure(y, _account))),
                                Times.Once);
        }
    }
}
