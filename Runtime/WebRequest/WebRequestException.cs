// WARNING: Auto generated code. Modifications will be lost!
using System;

namespace Unity.Services.RemoteConfig.WebRequest
{
    /// <summary>
    /// Class to represent a response from the HTTP Client.
    /// </summary>
    class WebRequestException : Exception
    {
        public WebRequestResponse Response { get; }

        public WebRequestException(WebRequestResponse response) : base(response.ErrorMessage)
        {
            Response = response;
        }

        public WebRequestException(WebRequestResponse response, string message, Exception innerException) : base(message, innerException)
        {
            Response = response;
        }
    }
}
