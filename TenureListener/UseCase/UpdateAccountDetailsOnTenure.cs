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

        public UpdateAccountDetailsOnTenure(IAccountApi personApi, ITenureInfoGateway gateway)
        {
            _accountApi = personApi;
            _gateway = gateway;
        }

        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Get Account from Account service API
            var account = await _accountApi.GetAccountByIdAsync(message.EntityId, message.CorrelationId)
                                           .ConfigureAwait(false);
            if (account is null) throw new AccountNotFoundException(message.EntityId);

            // 2. For the tenure specified on the account (the targetId is the tenureId)
            //      * Get the tenure object
            //      * Update the payment reference field
            //      * Save the updated tenure object if anything changed
            if (account.TargetId != Guid.Empty)
            {
                var tenure = await _gateway.GetTenureInfoByIdAsync(account.TargetId)
                                           .ConfigureAwait(false);
                if (tenure is null) throw new TenureNotFoundException(account.TargetId);

                if (tenure.PaymentReference != account.PaymentReference)
                {
                    tenure.PaymentReference = account.PaymentReference;
                    await _gateway.UpdateTenureInfoAsync(tenure).ConfigureAwait(false);
                }
            }
        }
    }
}
