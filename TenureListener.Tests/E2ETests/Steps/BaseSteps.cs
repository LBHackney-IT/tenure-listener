using Hackney.Core.Http;
using System.Text.Json;

namespace TenureListener.Tests.E2ETests.Steps
{
    public class BaseSteps
    {
        protected readonly JsonSerializerOptions _jsonOptions;

        public BaseSteps()
        {
            _jsonOptions = JsonOptions.Create();
        }
    }
}
