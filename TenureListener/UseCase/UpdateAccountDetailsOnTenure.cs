using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TenureListener.Boundary;
using TenureListener.Gateway.Interfaces;
using TenureListener.Infrastructure.Exceptions;
using TenureListener.UseCase.Interfaces;

namespace TenureListener.UseCase
{
    public class UpdateAccountDetailsOnTenure : IUpdateAccountDetailsOnTenure
    {
        private readonly IAccountApi _accountApi;
        private readonly ITenureInfoGateway _gateway;
        private readonly ILogger<UpdateAccountDetailsOnTenure> _logger;

        public UpdateAccountDetailsOnTenure(IAccountApi personApi, ITenureInfoGateway gateway,
            ILogger<UpdateAccountDetailsOnTenure> logger)
        {
            _accountApi = personApi;
            _gateway = gateway;
            _logger = logger;
        }

        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Get Account from Account service API
            var account = await _accountApi.GetAccountByIdAsync(message.EntityId, message.CorrelationId)
                                           .ConfigureAwait(false);
            if (account is null) throw new AccountNotFoundException(message.EntityId);

            // 2. For the tenure listed on the account...
            //      * Get the tenure object
            //      * Update the payment reference field
            //      * Save the updated tenure object if anything changed
            if (account.Tenure != null)
            {
                var tenure = await _gateway.GetTenureInfoByIdAsync(account.Tenure.TenancyId)
                                           .ConfigureAwait(false);
                if (tenure is null) throw new TenureNotFoundException(account.Tenure.TenancyId);

                if (tenure.PaymentReference != account.PaymentReference)
                {
                    tenure.PaymentReference = account.PaymentReference;
                    await _gateway.UpdateTenureInfoAsync(tenure).ConfigureAwait(false);
                }
            }
        }
    }
}
