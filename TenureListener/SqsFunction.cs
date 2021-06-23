using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Hackney.Core.DynamoDb;
using Hackney.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TenureListener.Boundary;
using TenureListener.Gateway;
using TenureListener.Gateway.Interfaces;
using TenureListener.UseCase;
using TenureListener.UseCase.Interfaces;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TenureListener
{
    [ExcludeFromCodeCoverage]
    public class SqsFunction : BaseFunction
    {
        private readonly static JsonSerializerOptions _jsonOptions = CreateJsonOptions();

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public SqsFunction()
        {
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

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureDynamoDB();

            services.AddHttpClient();
            services.AddScoped<IAddNewPersonToTenure, AddNewPersonToTenure>();
            services.AddScoped<IUpdatePersonDetailsOnTenure, UpdatePersonDetailsOnTenure>();

            services.AddScoped<IPersonApi, PersonApi>();
            services.AddScoped<ITenureInfoGateway, TenureInfoGateway>();

            base.ConfigureServices(services);
        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            // Do this in parallel???
            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context).ConfigureAwait(false);
            }
        }

        [LogCall(LogLevel.Information)]
        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine($"Processing message {message.MessageId}");

            var entityEvent = JsonSerializer.Deserialize<EntityEventSns>(message.Body, _jsonOptions);

            using (Logger.BeginScope("CorrelationId: {CorrelationId}", entityEvent.CorrelationId))
            {
                try
                {
                    IMessageProcessing processor = null;
                    switch (entityEvent.EventType)
                    {
                        case EventTypes.PersonCreatedEvent:
                            {
                                processor = ServiceProvider.GetService<IAddNewPersonToTenure>();
                                break;
                            }
                        case EventTypes.PersonUpdatedEvent:
                            {
                                processor = ServiceProvider.GetService<IUpdatePersonDetailsOnTenure>();
                                break;
                            }
                        default:
                            throw new ArgumentException($"Unknown event type: {entityEvent.EventType} on message id: {message.MessageId}");
                    }

                    await processor.ProcessMessageAsync(entityEvent).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Exception processing message id: {message.MessageId}; type: {entityEvent.EventType}; entity id: {entityEvent.EntityId}");
                    throw; // AWS will handle retry/moving to the dead letter queue
                }
            }
        }
    }
}
