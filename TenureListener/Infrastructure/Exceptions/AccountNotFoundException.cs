using System;

namespace TenureListener.Infrastructure.Exceptions
{
    public class AccountNotFoundException : EntityNotFoundException
    {
        public AccountNotFoundException(Guid id)
            : base("Account", id)
        { }
    }
}
