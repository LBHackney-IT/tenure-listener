using System;
using System.Threading.Tasks;
using TenureListener.Domain.Person;

namespace TenureListener.Gateway.Interfaces
{
    public interface IPersonApi
    {
        Task<PersonResponseObject> GetPersonByIdAsync(Guid id);
    }
}
