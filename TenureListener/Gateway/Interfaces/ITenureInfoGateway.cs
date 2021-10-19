using Hackney.Shared.Tenure.Domain;
using System;
using System.Threading.Tasks;

namespace TenureListener.Gateway.Interfaces
{
    public interface ITenureInfoGateway
    {
        Task<TenureInformation> GetTenureInfoByIdAsync(Guid id);
        Task UpdateTenureInfoAsync(TenureInformation tenureInfo);
    }
}
