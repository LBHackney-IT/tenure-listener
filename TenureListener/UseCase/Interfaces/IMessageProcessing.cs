using System.Threading.Tasks;
using TenureListener.Boundary;

namespace TenureListener.UseCase.Interfaces
{
    public interface IMessageProcessing
    {
        Task ProcessMessageAsync(EntityEventSns message);
    }
}
