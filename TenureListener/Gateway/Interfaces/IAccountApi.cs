using System;
using System.Threading.Tasks;
using TenureListener.Domain.Account;

namespace TenureListener.Gateway.Interfaces
{
    public interface IAccountApi
    {
        Task<AccountResponseObject> GetAccountByIdAsync(Guid id, Guid correlationId);
    }
}
