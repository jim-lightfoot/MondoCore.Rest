using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MondoCore.Common;
using Newtonsoft.Json;

namespace MondoCore.Rest
{
    /*************************************************************************/
    /*************************************************************************/
   /// <summary>
    /// Class to call a REST Api
    /// </summary>
    /// <typeparam name="TAPI">A generic type to differentiate apis in dependency injection. Not actually used</typeparam>
    public class RestApi<TAPI> :  IRestApi<TAPI>
    {
        private readonly IClientFactory   _clientFactory;
        private readonly IHeaderFactory?  _headerFactory;
        private readonly string           _name;
        private readonly int              _timeout;

        /*************************************************************************/
        /// <summary>
        /// Create a RestApi from an IHttpClientFactory
        /// </summary>
        /// <param name="httpClientFactory">Client factory</param>
        /// <param name="name">Name of api</param>
        /// <param name="headerFactory">A header factory to create headers at request time</param>
        public RestApi(IHttpClientFactory httpClientFactory, string name, IHeaderFactory? headerFactory = null, int timeout = 0)
        {
            _clientFactory = new FactoryClientFactory(httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory)), name);
            _headerFactory = headerFactory;

            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            _name = name;
            _timeout = timeout;
        }
        
        /*************************************************************************/
        /// <summary>
        /// Create a RestApi from an HttpClient
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="name">Name of api</param>
        /// <param name="dispose">If the HttpClient is a typed client then this should be true, otherwide it should be false</param>
        /// <param name="headerFactory">A header factory to create headers at request time</param>
        public RestApi(HttpClient httpClient, string name, bool dispose = true, IHeaderFactory? headerFactory = null, int timeout = 0)
        {
            _clientFactory = new TypedClientFactory(httpClient ?? throw new ArgumentNullException(nameof(httpClient)), dispose);
            _headerFactory = headerFactory;

            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            _name = name;
            _timeout = timeout;
        }
        
        #region IRestApi

        /*************************************************************************/
        public async Task SendRequest<TRequest>(HttpMethod method, string url, TRequest? content = default(TRequest?), object? headers = null)
        {
            await InternalSendRequest(method, url, content, headers);
        }

        /*************************************************************************/
        public async Task<TResponse> SendRequest<TRequest, TResponse>(HttpMethod method, string url, TRequest? content = default(TRequest?), object? headers = null)
        {
            var response = await InternalSendRequest(method, url, content, headers);
            var result   = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if(typeof(TResponse).Name == "String")
                return (TResponse)((object)result);

            return JsonConvert.DeserializeObject<TResponse>(result)!;
        }

        #endregion

        #region Private

        /*************************************************************************/
        private static void AddHeaders(HttpRequestMessage request, IDictionary<string, string> headers)
        {
            if(headers != null)
            {
                foreach (var kv in headers)
                    request.Headers.Add(kv.Key, kv.Value);
            }

            return;
        }

        /*************************************************************************/
        protected virtual async Task<HttpResponseMessage> InternalSendRequest<TRequest>(HttpMethod method, string url, TRequest content, object? headers)
        {
            using var clientManager = _clientFactory.CreateClient();
            var       client        = clientManager.Client;
            var       request       = new HttpRequestMessage(method, url);
            using var tokenSource   = new CancellationTokenSource();

            if(_timeout != 0)
                tokenSource.CancelAfter(TimeSpan.FromMilliseconds(_timeout));

            if(headers != null)
                AddHeaders(request, headers.ToStringDictionary());

            if(_headerFactory != null)
            { 
                var dHeaders = await _headerFactory.GetHeaders(_name).ConfigureAwait(false);

                AddHeaders(request, dHeaders);
            }

            // Set up the content
            if (content != null)
            {
                if (content is HttpContent httpContent)
                    request.Content = httpContent;
                else if (content is IDictionary<string, string> dict)
                    request.Content = new FormUrlEncodedContent(dict);
                else if (content is String contentStr)
                    request.Content = new StringContent(contentStr, System.Text.Encoding.UTF8, "application/json");
                else
                    request.Content = JsonContent.Create<TRequest>(content);
            }

            // Send the request
            var response = await client.SendAsync(request, tokenSource.Token).ConfigureAwait(false);

            // Ensure the request was successful (or throw exception)
            await CheckStatusCode(response, url, headers);

            return response;
        }

        /*************************************************************************/
        private async Task CheckStatusCode(HttpResponseMessage message, string url, object? headers = null)
        {
            if (!message.IsSuccessStatusCode)
            {
                var response = await GetExceptionResponse(message);

                throw new RestException(response.Message)
                { 
                    StatusCode = message.StatusCode,
                    Url        = url,
                    Headers    = headers,
                    ApiName    = _name,
                    Response   = response.Response
                };
            }
        }
        
        /*************************************************************************/
        private static async Task<(string Message, string Response)> GetExceptionResponse(HttpResponseMessage responseMsg)
        {
            var responseStr = "";

            try
            {
                responseStr = await responseMsg.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (responseMsg?.Content?.Headers?.ContentType?.MediaType == "application/problem+json")
                {
                    var innerError = JsonConvert.DeserializeObject<JsonProblem>(responseStr);

                    if(!string.IsNullOrWhiteSpace(innerError?.title))
                    {
                        return (innerError.title, responseStr);
                    }
                }
            }
            catch
            {
                // Just return the default response below
            }

            var statusCode = responseMsg?.StatusCode.ToString() ?? "";

            return ($"Rest Api Exception, Status Code = {statusCode}", responseStr);
        }

        public void Dispose() => _clientFactory?.Dispose();

        /*************************************************************************/
        /*************************************************************************/
        // https://datatracker.ietf.org/doc/html/rfc7807
        private sealed class JsonProblem
        {
            public string? type        { get; set; } 
            public string? title       { get; set; } 
            public string? detail      { get; set; } 
            public string? instance    { get; set; } 
        }

        #region ClientManager

        /*************************************************************************/
        private interface IClientFactory : IDisposable
        {
          IClientManager CreateClient();
        }

        /*************************************************************************/
        private class TypedClientFactory(HttpClient client, bool dispose) : IClientFactory
        {
            public IClientManager CreateClient() => new TypedClient(client);

            /*************************************************************************/
            public void Dispose() 
            { 
                if(dispose)
                    client?.Dispose();
            }
        }

        /*************************************************************************/
        private class FactoryClientFactory(IHttpClientFactory factory, string name) : IClientFactory
        {
            public IClientManager CreateClient() => new FactoryClient(factory, name);

            /*************************************************************************/
            public void Dispose() 
            { 
                // Do nothing
            }
        }

        /*************************************************************************/
        private interface IClientManager : IDisposable
        {
          HttpClient Client { get; }
        }

        /*************************************************************************/
        private class TypedClient(HttpClient client) : IClientManager
        {
            public HttpClient Client => client;

            /*************************************************************************/
            public void Dispose() 
            { 
                // Do nothing
            }
        }

        /*************************************************************************/
        private class FactoryClient(IHttpClientFactory factory, string name) : IClientManager
        {
            public HttpClient Client => factory.CreateClient(name);

            public void Dispose() 
            { 
                this.Client?.Dispose();
            }
        }

        #endregion

        #endregion
    }
}
