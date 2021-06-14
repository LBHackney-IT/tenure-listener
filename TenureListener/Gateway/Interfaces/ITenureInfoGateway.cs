using System;
using System.Threading.Tasks;
using TenureListener.Domain;

namespace TenureListener.Gateway.Interfaces
{
    public interface ITenureInfoGateway
    {
        Task<TenureInformation> GetTenureInfoByIdAsync(Guid id);
        Task UpdateTenureInfoAsync(TenureInformation tenureInfo);
    }
}
