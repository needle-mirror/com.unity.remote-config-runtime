using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Unity.Services.RemoteConfig
{
  public interface IRCUnityWebRequest
  {
    /// <summary>
    /// UnityWebRequest object handles the flow of HTTP communication with web servers
    /// </summary>
    UnityWebRequest unityWebRequest { get; set; }

    /// <summary>
    /// If true, any CertificateHandler attached to this UnityWebRequest will have CertificateHandler.Dispose called automatically when UnityWebRequest.Dispose is called.
    /// </summary>
    bool disposeCertificateHandlerOnDispose { get; set; }

    /// <summary>
    /// If true, any DownloadHandler attached to this UnityWebRequest will have DownloadHandler.Dispose called automatically when UnityWebRequest.Dispose is called.
    /// </summary>
    bool disposeDownloadHandlerOnDispose { get; set; }

    /// <summary>
    /// If true, any UploadHandler attached to this UnityWebRequest will have UploadHandler.Dispose called automatically when UnityWebRequest.Dispose is called.
    /// </summary>
    bool disposeUploadHandlerOnDispose { get; set; }

    /// <summary>
    /// Defines the HTTP verb used by this UnityWebRequest, such as GET or POST.
    /// </summary>
    string method { get; set; }

    /// <summary>
    /// A human-readable string describing any system errors encountered by this UnityWebRequest object while handling HTTP requests or responses. (Read Only)
    /// </summary>
    string error { get; }

    /// <summary>
    /// Determines whether this UnityWebRequest will include Expect: 100-Continue in its outgoing request headers. (Default: true).
    /// </summary>
    bool useHttpContinue { get; set; }

    ///<summary>
    /// Defines the target URL for the UnityWebRequest to communicate with.
    /// </summary>
    string url { get; set; }

    /// <summary>
    /// Defines the target URI for the UnityWebRequest to communicate with.
    /// </summary>
    Uri uri { get; set; }

    /// <summary>
    /// The numeric HTTP response code returned by the server, such as 200, 404 or 500. (Read Only)
    /// </summary>
    long responseCode
    {
      get;
    }

    /// <summary>
    /// Returns a floating-point value between 0.0 and 1.0, indicating the progress of uploading body data to the server.
    /// </summary>
    float uploadProgress { get; }

    /// <summary>
    /// Returns true while a UnityWebRequest’s configuration properties can be altered. (Read Only)
    /// </summary>
    bool isModifiable
    {
      get;
    }

    /// <summary>
    /// Returns true after the UnityWebRequest has finished communicating with the remote server. (Read Only)
    /// </summary>
    bool isDone
    {
      get;
    }

    /// <summary>
    /// Returns true after this UnityWebRequest encounters a system error. (Read Only)
    /// </summary>
    bool isNetworkError
    {
      get;
    }

    /// <summary>
    /// Returns true after this UnityWebRequest receives an HTTP response code indicating an error. (Read Only)
    /// </summary>
    bool isHttpError
    {
      get;
    }

    /// <summary>
    /// Returns a floating-point value between 0.0 and 1.0, indicating the progress of downloading body data from the server. (Read Only)
    /// </summary>
    float downloadProgress { get; }

    /// <summary>
    /// Returns the number of bytes of body data the system has uploaded to the remote server. (Read Only)
    /// </summary>
    ulong uploadedBytes
    {
      get;
    }

    /// <summary>
    /// Returns the number of bytes of body data the system has downloaded from the remote server. (Read Only)
    /// </summary>
    ulong downloadedBytes
    {
      get;
    }

    /// <summary>
    /// Indicates the number of redirects which this UnityWebRequest will follow before halting with a “Redirect Limit Exceeded” system error.
    /// </summary>
    int redirectLimit { get; set; }

    /// <summary>
    /// Holds a reference to the UploadHandler object which manages body data to be uploaded to the remote server.
    /// </summary>
    UploadHandler uploadHandler { get; set; }

    /// <summary>
    /// Holds a reference to a DownloadHandler object, which manages body data received from the remote server by this UnityWebRequest.
    /// </summary>
    DownloadHandler downloadHandler { get; set; }

    /// <summary>
    /// Holds a reference to a CertificateHandler object, which manages certificate validation for this UnityWebRequest.
    /// </summary>
    CertificateHandler certificateHandler { get; set; }

    /// <summary>
    /// Sets UnityWebRequest to attempt to abort after the number of seconds in timeout have passed.
    /// </summary>
    int timeout { get; set; }

    /// <summary>
    /// Signals that this UnityWebRequest is no longer being used, and should clean up any resources it is using.
    /// </summary>
    void Dispose();

    /// <summary>
    /// Begins communicating with the remote server.
    /// </summary>
    /// <returns>
    /// The WebRequestAsyncOperation object. Yielding the WebRequestAsyncOperation inside a coroutine will cause the coroutine to pause until the UnityWebRequest encounters a system error or finishes communicating.
    /// </returns>
    UnityWebRequestAsyncOperation SendWebRequest();

    /// <summary>
    /// If in progress, halts the UnityWebRequest as soon as possible.
    /// </summary>
    void Abort();

    /// <summary>
    /// Retrieves the value of a custom request header.
    /// </summary>
    /// <param name="name">Name of the custom request header. Case-insensitive.</param>
    /// <returns>
    /// The value of the custom request header. If no custom header with a matching name has been set, returns an empty string.
    /// </returns>
    string GetRequestHeader(string name);

    /// <summary>
    /// Set a HTTP request header to a custom value.
    /// </summary>
    /// <param name="name">The key of the header to be set. Case-sensitive.</param>
    /// <param name="value">The header's intended value.</param>
    void SetRequestHeader(string name, string value);

    /// <summary>
    /// Retrieves the value of a response header from the latest HTTP response received.
    /// </summary>
    /// <param name="name">The name of the HTTP header to retrieve. Case-insensitive.</param>
    /// <returns>
    /// The value of the HTTP header from the latest HTTP response. If no header with a matching name has been received, or no responses have been received, returns null.
    /// </returns>
    string GetResponseHeader(string name);

    /// <summary>
    /// Retrieves a dictionary containing all the response headers received by this UnityWebRequest in the latest HTTP response.
    /// </summary>
    /// <returns>
    /// A dictionary containing all the response headers received in the latest HTTP response. If no responses have been received, returns null.
    /// </returns>
    Dictionary<string, string> GetResponseHeaders();
  }
}