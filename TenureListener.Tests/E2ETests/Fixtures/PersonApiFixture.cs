using AutoFixture;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TenureListener.Domain.Person;

namespace TenureListener.Tests.E2ETests.Fixtures
{
    public class PersonApiFixture : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly JsonSerializerOptions _jsonOptions;
        private static HttpListener _httpListener;
        public static PersonResponseObject PersonResponse { get; private set; }

        public static string PersonApiRoute => "http://localhost:5678/api/v1/persons/";

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
            Environment.SetEnvironmentVariable("GetPersonApi", PersonApiRoute);
            Task.Run(() =>
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add(PersonApiRoute);
                _httpListener.Start();

                // GetContext method blocks while waiting for a request. 
                HttpListenerContext context = _httpListener.GetContext();
                HttpListenerResponse response = context.Response;

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
            });
        }

        public PersonResponseObject GivenThePersonExists(Guid personId)
        {
            PersonResponse = _fixture.Build<PersonResponseObject>()
                                      .With(x => x.Id, personId)
                                      .With(x => x.Tenures, _fixture.CreateMany<Tenure>(1))
                                      .Create();
            return PersonResponse;
        }
    }
}
