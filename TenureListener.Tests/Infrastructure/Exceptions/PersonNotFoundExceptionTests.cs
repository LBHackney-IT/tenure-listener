using FluentAssertions;
using System;
using TenureListener.Infrastructure.Exceptions;
using Xunit;

namespace TenureListener.Tests.Infrastructure.Exceptions
{
    public class PersonNotFoundExceptionTests
    {
        [Fact]
        public void PersonNotFoundExceptionConstructorTest()
        {
            var id = Guid.NewGuid();

            var ex = new PersonNotFoundException(id);
            ex.Id.Should().Be(id);
            ex.EntityName.Should().Be("Person");
            ex.Message.Should().Be($"Person with id {id} not found.");
        }
    }
}
