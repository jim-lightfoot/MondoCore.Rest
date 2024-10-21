using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MondoCore.Rest
{
    /*************************************************************************/
    /*************************************************************************/
    /// <summary>
    /// Exception thrown when calling a Rest APi
    /// </summary>
    public class RestException : Exception
    {
        /*************************************************************************/
        public RestException(string message) : base(message)
        {
        }

        /*************************************************************************/
        public RestException(string message, Exception inner) : base(message, inner)
        {
        }

        /*************************************************************************/
        public HttpStatusCode   StatusCode { get; set; }
        public string           Url        { get; set; } = "";
        public object?          Headers    { get; set; }
        public string           ApiName    { get; set; } = "";
        public string?          Response   { get; set; }
    }   
}
