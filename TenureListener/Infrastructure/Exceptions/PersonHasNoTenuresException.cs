using System;

namespace TenureListener.Infrastructure.Exceptions
{
    public class PersonHasNoTenuresException : Exception
    {
        public Guid Id { get; }

        public PersonHasNoTenuresException(Guid id)
            : base($"Person with id {id} has no tenures.")
        {
            Id = id;
        }
    }
}
