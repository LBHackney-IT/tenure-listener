using System;

namespace TenureListener.Infrastructure.Exceptions
{
    public class TenureNotFoundException : EntityNotFoundException
    {
        public TenureNotFoundException(Guid id)
            : base("Tenure", id)
        { }
    }
}
