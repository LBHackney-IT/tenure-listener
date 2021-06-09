using System;
using System.Linq;
using System.Threading.Tasks;
using TenureListener.Boundary;
using TenureListener.Domain;
using TenureListener.Gateway.Interfaces;
using TenureListener.Infrastructure.Exceptions;
using TenureListener.UseCase.Interfaces;

namespace TenureListener.UseCase
{
    public class AddNewPersonToTenure : IAddNewPersonToTenure
    {
        private readonly IPersonApi _personApi;
        private readonly ITenureInfoGateway _gateway;

        public AddNewPersonToTenure(IPersonApi personApi, ITenureInfoGateway gateway)
        {
            _personApi = personApi;
            _gateway = gateway;
        }

        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            // 1. Get Person from Person service API
            var person = await _personApi.GetPersonByIdAsync(message.EntityId)
                                         .ConfigureAwait(false);
            if (person is null) throw new PersonNotFoundException(message.EntityId);
            if (!person.Tenures.Any()) throw new PersonHasNoTenuresException(person.Id);

            // 2. Get the tenure
            var tenureId = person.Tenures.First().Id;
            var tenure = await _gateway.GetTenureInfoByIdAsync(tenureId)
                                       .ConfigureAwait(false);
            if (tenure is null) throw new TenureNotFoundException(tenureId);

            // 3. Update the tenure with the person details
            var membersList = tenure.HouseholdMembers.ToList();
            membersList.Add(new HouseholdMembers()
            {
                Id = person.Id,
                Type = HouseholdMembersType.Person,
                FullName = person.FullName
            });
            tenure.HouseholdMembers = membersList;

            // 4. Save updated tenure
            await _gateway.UpdateTenureInfoAsync(tenure).ConfigureAwait(false);
        }
    }
}
