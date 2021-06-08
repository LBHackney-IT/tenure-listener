using Amazon.DynamoDBv2.DataModel;
using Hackney.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TenureListener.Domain;
using TenureListener.Factories;
using TenureListener.Gateway.Interfaces;
using TenureListener.Infrastructure;

namespace TenureListener.Gateway
{
    public class TenureInfoGateway : ITenureInfoGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<TenureInfoGateway> _logger;

        public TenureInfoGateway(IDynamoDBContext dynamoDbContext, ILogger<TenureInfoGateway> logger)
        {
            _logger = logger;
            _dynamoDbContext = dynamoDbContext;
        }

        [LogCall]
        public async Task<TenureInformation> GetTenureInfoByIdAsync(Guid id)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.LoadAsync for id {id}");
            var result = await _dynamoDbContext.LoadAsync<TenureInformationDb>(id).ConfigureAwait(false);
            return result?.ToDomain();
        }

        [LogCall]
        public async Task UpdateTenureInfoAsync(TenureInformation tenureInfo)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.SaveAsync for id {tenureInfo.Id}");
            await _dynamoDbContext.SaveAsync(tenureInfo.ToDatabase()).ConfigureAwait(false);
        }
    }
}
