using System.Threading.Tasks;
using TenureListener.Boundary;

namespace TenureListener.UseCase.Interfaces
{
    public interface IAddNewPersonToTenure
    {
        Task ProcessMessageAsync(EntityEventSns message);
    }
}
