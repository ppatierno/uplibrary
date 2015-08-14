using System;

namespace uPLibrary.Networking.Http
{
    /// <summary>
    /// Status code for HTTP response
    /// </summary>
    public enum HttpStatusCode
    {
        OK = 200,
        Created = 201,
        NoContent = 204,
        Redirect = 302,
        BadRequest = 400,
        UnAuthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        InternalServerError = 500,
        ServiceUnavailable = 503,
    }
}
