using FluentAssertions;
using System;
using TenureListener.Infrastructure.Exceptions;
using Xunit;

namespace TenureListener.Tests.Infrastructure.Exceptions
{
    public class AccountNotFoundExceptionTests
    {
        [Fact]
        public void AccountNotFoundExceptionConstructorTest()
        {
            var id = Guid.NewGuid();

            var ex = new AccountNotFoundException(id);
            ex.Id.Should().Be(id);
            ex.EntityName.Should().Be("Account");
            ex.Message.Should().Be($"Account with id {id} not found.");
        }
    }
}
