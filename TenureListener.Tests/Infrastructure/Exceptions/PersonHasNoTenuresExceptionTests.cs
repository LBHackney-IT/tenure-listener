using FluentAssertions;
using System;
using TenureListener.Infrastructure.Exceptions;
using Xunit;

namespace TenureListener.Tests.Infrastructure.Exceptions
{
    public class PersonHasNoTenuresExceptionTests
    {
        [Fact]
        public void PersonHasNoTenuresExceptionConstructorTest()
        {
            var id = Guid.NewGuid();

            var ex = new PersonHasNoTenuresException(id);
            ex.Id.Should().Be(id);
            ex.Message.Should().Be($"Person with id {id} has no tenures.");
        }
    }
}
