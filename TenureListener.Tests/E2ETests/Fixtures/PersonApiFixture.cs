using AutoFixture;
using Hackney.Shared.Person.Boundary.Response;
using Hackney.Shared.Person.Domain;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TenureListener.Tests.E2ETests.Fixtures
{
    public class PersonApiFixture : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly JsonSerializerOptions _jsonOptions;
        private static HttpListener _httpListener;
        public static PersonResponseObject PersonResponse { get; private set; }
        public static int CallsMade { get; private set; }

        public string ReceivedCorrelationId { get; private set; }

        public static string PersonApiRoute => "http://localhost:5678/api/v1/";
        public static string PersonApiToken => "sdjkhfgsdkjfgsdjfgh";

        public PersonApiFixture()
        {
            _jsonOptions = CreateJsonOptions();
            StartPersonApiStub();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (_httpListener.IsListening)
                    _httpListener.Stop();
                PersonResponse = null;

                _disposed = true;
            }
        }

        private JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        private void StartPersonApiStub()
        {
            CallsMade = 0;

            Environment.SetEnvironmentVariable("PersonApiUrl", PersonApiRoute);
            Environment.SetEnvironmentVariable("PersonApiToken", PersonApiToken);
            ReceivedCorrelationId = null;

            Task.Run(() =>
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add(PersonApiRoute);
                _httpListener.Start();

                // GetContext method blocks while waiting for a request. 
                HttpListenerContext context = _httpListener.GetContext();
                CallsMade++;
                HttpListenerResponse response = context.Response;

                if (context.Request.Headers["Authorization"] != PersonApiToken)
                {
                    response.StatusCode = (int) HttpStatusCode.Unauthorized;
                }
                else
                {
                    ReceivedCorrelationId = context.Request.Headers["x-correlation-id"];

                    response.StatusCode = (int) ((PersonResponse is null) ? HttpStatusCode.NotFound : HttpStatusCode.OK);
                    string responseBody = string.Empty;
                    if (PersonResponse is null)
                    {
                        responseBody = context.Request.Url.Segments.Last();
                    }
                    else
                    {
                        responseBody = JsonSerializer.Serialize(PersonResponse, _jsonOptions);
                    }
                    Stream stream = response.OutputStream;
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(responseBody);
                        writer.Close();
                    }
                }
            });
        }

        public PersonResponseObject GivenThePersonExistsWithMultipleTenures(Guid personId, int numberOfTenures)
        {
            return GivenThePersonExistsWithMultipleTenures(personId, numberOfTenures, PersonType.Tenant);
        }
        public PersonResponseObject GivenThePersonExistsWithMultipleTenures(Guid personId, int numberOfTenures, PersonType personType)
        {
            PersonResponse = _fixture.Build<PersonResponseObject>()
                                      .With(x => x.Id, personId)
                                      .With(x => x.PersonTypes, new PersonType[] { personType })
                                      .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-30).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ"))
                                      .With(x => x.Tenures, _fixture.CreateMany<TenureResponseObject>(numberOfTenures))
                                      .Create();
            return PersonResponse;
        }

        public void GivenThePersonDoesNotExist(Guid personId)
        {
            // Nothing to do here
        }

        public PersonResponseObject GivenThePersonExists(Guid personId)
        {
            return GivenThePersonExists(personId, true, PersonType.Tenant);
        }
        public PersonResponseObject GivenThePersonExists(Guid personId, bool hasTenure)
        {
            return GivenThePersonExists(personId, hasTenure, PersonType.Tenant);
        }
        public PersonResponseObject GivenThePersonExists(Guid personId, PersonType personType)
        {
            return GivenThePersonExists(personId, true, personType);
        }
        public PersonResponseObject GivenThePersonExists(Guid personId, bool hasTenure, PersonType personType)
        {
            int numberOfTenures = hasTenure ? 1 : 0;
            return GivenThePersonExistsWithMultipleTenures(personId, numberOfTenures, personType);
        }
    }
}
