using System;
using System.Net;

namespace TenureListener.Infrastructure.Exceptions
{
    public class GetPersonException : Exception
    {
        public Guid PersonId { get; }
        public HttpStatusCode StatusCode { get; }
        public string ResponseBody { get; }

        public GetPersonException(Guid id, HttpStatusCode statusCode, string responseBody)
            : base($"Failed to get person details for id {id}. Status code: {statusCode}; Message: {responseBody}")
        {
            PersonId = id;
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}
