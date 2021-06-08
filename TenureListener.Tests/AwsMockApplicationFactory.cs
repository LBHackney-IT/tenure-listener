using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Hackney.Core.DynamoDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

namespace TenureListener.Tests
{
    public class AwsMockApplicationFactory
    {
        private readonly List<TableDef> _tables;

        public IAmazonDynamoDB DynamoDb { get; private set; }
        public IDynamoDBContext DynamoDbContext { get; private set; }
        //public IAmazonSQS SqsClient { get; private set; }

        public AwsMockApplicationFactory(List<TableDef> tables)
        {
            _tables = tables;
        }

        public IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
           .ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
           .ConfigureServices((hostContext, services) =>
           {
               services.ConfigureDynamoDB();

               var serviceProvider = services.BuildServiceProvider();
               DynamoDb = serviceProvider.GetRequiredService<IAmazonDynamoDB>();
               DynamoDbContext = serviceProvider.GetRequiredService<IDynamoDBContext>();

               EnsureTablesExist(DynamoDb, _tables);

               //var snsUrl = Environment.GetEnvironmentVariable("Localstack_SnsServiceUrl");
               //SqsClient = new AmazonSQSClient(new AmazonSQSConfig()
               //{
               //    ServiceURL = snsUrl
               //});
               //EnsureSqsQueueExists(SqsClient);
           });

        //private static void EnsureSqsQueueExists(IAmazonSQS sqsClient)
        //{
        //    var sqsAttrs = new Dictionary<string, string>();
        //    sqsAttrs.Add("fifo_topic", "true");
        //    sqsAttrs.Add("content_based_deduplication", "true");

        //    var request = new CreateQueueRequest()
        //    {
        //        QueueName = "person",
        //        Attributes = sqsAttrs
        //    };
        //    _ = sqsClient.CreateQueueAsync(request).GetAwaiter().GetResult();
        //}

        private static void EnsureTablesExist(IAmazonDynamoDB dynamoDb, List<TableDef> tables)
        {
            foreach (var table in tables)
            {
                try
                {
                    var request = new CreateTableRequest(table.Name,
                        new List<KeySchemaElement> { new KeySchemaElement(table.KeyName, KeyType.HASH) },
                        new List<AttributeDefinition> { new AttributeDefinition(table.KeyName, table.KeyType) },
                        new ProvisionedThroughput(3, 3));
                    _ = dynamoDb.CreateTableAsync(request).GetAwaiter().GetResult();
                }
                catch (ResourceInUseException)
                {
                    // It already exists :-)
                }
            }
        }
    }
}
