using System;
using System.Net.Http;
using Hackney.Core.Http.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace TenureListener.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        // can be moved to Hackney.Http.Core
        public static void AddPollyRegistry(this IServiceCollection services)
        {
            var registry = services.AddPolicyRegistry();

            // 3 repeats with 100 ms delay in between 
            registry[PolicyConstants.WaitAndRetry] = Policy
                .Handle<GetFromApiException>()
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(100));
        }
    }
}