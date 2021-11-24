using Hackney.Core.Testing.Shared;
using Xunit;

namespace TenureListener.Tests
{
    [CollectionDefinition("LogCall collection")]
    public class LogCallAspectFixtureCollection : ICollectionFixture<LogCallAspectFixture>
    { }
}
