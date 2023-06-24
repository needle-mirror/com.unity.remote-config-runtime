// WARNING: Auto generated code. Modifications will be lost!
using System.Collections.Generic;

namespace Unity.Services.RemoteConfig.WebRequest
{
    /// <summary>
    /// Class to represent a response from the HTTP Client.
    /// </summary>
    class WebRequestResponse
    {
        /// <summary>Response Http Headers.</summary>
        public Dictionary<string, string> Headers { get; }
        /// <summary>Response Http Status Code.</summary>
        public long StatusCode { get; }
        /// <summary>True if the request has encountered an HTTP error.</summary>
        public bool IsHttpError { get; }
        /// <summary>True if the request has encountered a Network error.</summary>
        public bool IsNetworkError { get; }
        /// <summary>String containing the response body.</summary>
        public string Data { get; }
        /// <summary>Error message if an error occurs.</summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="headers">Response Http Headers.</param>
        /// <param name="statusCode">Response Http Status Code.</param>
        /// <param name="isHttpError">True if the request has encountered an HTTP error.</param>
        /// <param name="isNetworkError">True if the request has encountered a Network error.</param>
        /// <param name="data">String containing the response body.</param>
        /// <param name="errorMessage">Error message if an error occurs.</param>
        public WebRequestResponse(Dictionary<string, string> headers, long statusCode, bool isHttpError, bool isNetworkError, string data, string errorMessage)
        {
            Headers = headers;
            StatusCode = statusCode;
            IsHttpError = isHttpError;
            IsNetworkError = isNetworkError;
            Data = data;
            ErrorMessage = errorMessage;
        }
    }
}
