using Hackney.Shared.Person.Boundary.Response;
using System;
using System.Threading.Tasks;

namespace TenureListener.Gateway.Interfaces
{
    public interface IPersonApi
    {
        Task<PersonResponseObject> GetPersonByIdAsync(Guid id, Guid correlationId);
    }
}
