using FluentAssertions;
using System;
using TenureListener.Infrastructure.Exceptions;
using Xunit;

namespace TenureListener.Tests.Infrastructure.Exceptions
{
    public class TenureNotFoundExceptionTests
    {
        [Fact]
        public void TenureNotFoundExceptionConstructorTest()
        {
            var id = Guid.NewGuid();

            var ex = new TenureNotFoundException(id);
            ex.Id.Should().Be(id);
            ex.EntityName.Should().Be("Tenure");
            ex.Message.Should().Be($"Tenure with id {id} not found.");
        }
    }
}
