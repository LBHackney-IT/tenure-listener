using System;
using System.Threading.Tasks;
using Hackney.Shared.Tenure;

namespace TenureListener.Gateway.Interfaces
{
    public interface ITenureInfoGateway
    {
        Task<TenureInformation> GetTenureInfoByIdAsync(Guid id);
        Task UpdateTenureInfoAsync(TenureInformation tenureInfo);
    }
}
