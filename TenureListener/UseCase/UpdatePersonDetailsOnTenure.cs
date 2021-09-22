using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TenureListener.Boundary;
using TenureListener.Gateway.Interfaces;
using TenureListener.Infrastructure.Exceptions;
using TenureListener.UseCase.Interfaces;

namespace TenureListener.UseCase
{
    public class UpdatePersonDetailsOnTenure : IUpdatePersonDetailsOnTenure
    {
        private readonly IPersonApi _personApi;
        private readonly ITenureInfoGateway _gateway;
        private readonly ILogger<UpdatePersonDetailsOnTenure> _logger;

        public UpdatePersonDetailsOnTenure(IPersonApi personApi, ITenureInfoGateway gateway,
            ILogger<UpdatePersonDetailsOnTenure> logger)
        {
            _personApi = personApi;
            _gateway = gateway;
            _logger = logger;
        }

        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Get Person from Person service API
            var person = await _personApi.GetPersonByIdAsync(message.EntityId, message.CorrelationId)
                                         .ConfigureAwait(false);
            if (person is null) throw new PersonNotFoundException(message.EntityId);

            // 2. For each tenure listed on the person...
            //      * Get the tenure object
            //      * Update any person fields if anything changed
            //      * Save the updated tenure object if anything changed
            if ((person.Tenures != null) && person.Tenures.Any())
            {
                foreach (var tenureId in person.Tenures?.Select(x => x.Id))
                {
                    var tenure = await _gateway.GetTenureInfoByIdAsync(tenureId)
                                               .ConfigureAwait(false);
                    if (tenure is null)
                    {
                        _logger.LogWarning($"Person record (id: {person.Id}) has tenure for id {tenureId} but the tenure was not found.");
                        continue;
                    }

                    var tenureHouseholdMember = tenure.HouseholdMembers.FirstOrDefault(x => x.Id == person.Id);
                    if (tenureHouseholdMember is null)
                    {
                        _logger.LogWarning($"Person record (id: {person.Id}) has tenure for id {tenureId} but is not listed in the tenure's household members.");
                        continue;
                    }

                    var personDoB = DateTime.Parse(person.DateOfBirth);
                    // Only bother if these details (name and date of birth) have changed...
                    if ((person.FullName != tenureHouseholdMember.FullName)
                        || (personDoB.Date != tenureHouseholdMember.DateOfBirth.Date))
                    {
                        tenureHouseholdMember.FullName = person.FullName;
                        tenureHouseholdMember.DateOfBirth = personDoB;

                        await _gateway.UpdateTenureInfoAsync(tenure).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
