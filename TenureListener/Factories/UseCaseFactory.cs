using Microsoft.Extensions.DependencyInjection;
using System;
using TenureListener.Boundary;
using TenureListener.UseCase.Interfaces;

namespace TenureListener.Factories
{
    public static class UseCaseFactory
    {
        public static IMessageProcessing CreateUseCaseForMessage(this EntityEventSns entityEvent, IServiceProvider serviceProvider)
        {
            if (entityEvent is null) throw new ArgumentNullException(nameof(entityEvent));
            if (serviceProvider is null) throw new ArgumentNullException(nameof(serviceProvider));

            IMessageProcessing processor = null;
            switch (entityEvent.EventType)
            {
                case EventTypes.PersonCreatedEvent:
                    // We only process this if the event is v1
                    if (entityEvent.Version == EventVersions.V1)
                        processor = serviceProvider.GetService<IAddNewPersonToTenure>();
                    break;

                case EventTypes.PersonUpdatedEvent:
                    processor = serviceProvider.GetService<IUpdatePersonDetailsOnTenure>();
                    break;

                case EventTypes.AccountCreatedEvent:
                    processor = serviceProvider.GetService<IUpdateAccountDetailsOnTenure>();
                    break;

                default:
                    throw new ArgumentException($"Unknown event type: {entityEvent.EventType}");
            }

            return processor;
        }
    }
}
