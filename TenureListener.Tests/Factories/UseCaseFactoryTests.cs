using AutoFixture;
using FluentAssertions;
using Moq;
using System;
using TenureListener.Boundary;
using TenureListener.Factories;
using TenureListener.UseCase.Interfaces;
using Xunit;

namespace TenureListener.Tests.Factories
{
    public class UseCaseFactoryTests
    {
        private readonly Fixture _fixture = new Fixture();
        private EntityEventSns _event;
        private readonly Mock<IServiceProvider> _mockServiceProvider;

        public UseCaseFactoryTests()
        {
            _mockServiceProvider = new Mock<IServiceProvider>();

            _event = ConstructEvent(EventTypes.PersonCreatedEvent);
        }

        private EntityEventSns ConstructEvent(string eventType, string version = EventVersions.V1)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.Version, version)
                           .Create();
        }

        [Fact]
        public void CreateUseCaseForMessageTestNullEventThrows()
        {
            Action act = () => UseCaseFactory.CreateUseCaseForMessage(null, _mockServiceProvider.Object);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateUseCaseForMessageTestNullServiceProviderThrows()
        {
            Action act = () => UseCaseFactory.CreateUseCaseForMessage(_event, null);
            act.Should().Throw<ArgumentNullException>();
        }

        private void TestMessageProcessingCreation<T>(EntityEventSns eventObj) where T : class, IMessageProcessing
        {
            var mockProcessor = new Mock<T>();
            _mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(mockProcessor.Object);

            var result = UseCaseFactory.CreateUseCaseForMessage(eventObj, _mockServiceProvider.Object);
            result.Should().NotBeNull();
            _mockServiceProvider.Verify(x => x.GetService(typeof(T)), Times.Once);
        }

        [Fact]
        public void CreateUseCaseForMessageTestUnknownEventThrows()
        {
            _event = ConstructEvent("UnknownEvent");

            Action act = () => UseCaseFactory.CreateUseCaseForMessage(_event, _mockServiceProvider.Object);
            act.Should().Throw<ArgumentException>().WithMessage($"Unknown event type: {_event.EventType}");
            _mockServiceProvider.Verify(x => x.GetService(typeof(IAddNewPersonToTenure)), Times.Never);
        }

        [Fact]
        public void CreateUseCaseForMessageTestPersonCreatedEvent()
        {
            _event = ConstructEvent(EventTypes.PersonCreatedEvent);
            TestMessageProcessingCreation<IAddNewPersonToTenure>(_event);
        }

        [Fact]
        public void CreateUseCaseForMessageTestPersonCreatedEventV2ReturnsNull()
        {
            _event = ConstructEvent(EventTypes.PersonCreatedEvent, EventVersions.V2);

            var result = UseCaseFactory.CreateUseCaseForMessage(_event, _mockServiceProvider.Object);
            result.Should().BeNull();
            _mockServiceProvider.Verify(x => x.GetService(typeof(IAddNewPersonToTenure)), Times.Never);
        }

        [Fact]
        public void CreateUseCaseForMessageTestPersonUpdatedEvent()
        {
            _event = ConstructEvent(EventTypes.PersonUpdatedEvent);
            TestMessageProcessingCreation<IUpdatePersonDetailsOnTenure>(_event);
        }
    }
}
