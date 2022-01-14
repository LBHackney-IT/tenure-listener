using Xunit;

namespace TenureListener.Tests
{
    [CollectionDefinition("AppTest collection", DisableParallelization = true)]
    public class AppTestCollection : ICollectionFixture<MockApplicationFactory>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
