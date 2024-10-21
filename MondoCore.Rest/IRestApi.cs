using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MondoCore.Rest
{
    /*************************************************************************/
    /*************************************************************************/
    /// <summary>
    /// Interface for rest APIs
    /// </summary>
    /// <typeparam name="TAPI">A generic type to differentiate apis in dependency injection. Not actually used</typeparam>
    public interface IRestApi<TAPI> : IDisposable
    {
        Task            SendRequest<TRequest>(HttpMethod method, string url, TRequest? content = default(TRequest?), object? headers = null);
        Task<TResponse> SendRequest<TRequest, TResponse>(HttpMethod method, string url, TRequest? content = default(TRequest?), object? headers = null);

        #region Default Methods

        /*************************************************************************/
        public Task<T> Get<T>(string url, object? headers = null)
        {
            return SendRequest<object, T>(HttpMethod.Get, url, headers: headers);
        }

        /*************************************************************************/
        public Task<TResponse> Post<TRequest, TResponse>(string url, TRequest content, object? headers = null)
        {
            return SendRequest<TRequest, TResponse>(HttpMethod.Post, url, content, headers);
        }

        /*************************************************************************/
        public Task Post<TRequest>(string url, TRequest content, object? headers = null)
        {
            return SendRequest<TRequest>(HttpMethod.Post, url, content, headers);
        }

        /*************************************************************************/
        public Task<TResponse> Put<TRequest, TResponse>(string url, TRequest content, object? headers = null)
        {
            return SendRequest<TRequest, TResponse>(HttpMethod.Put, url, content, headers);
        }

        /*************************************************************************/
        public Task Put<TRequest>(string url, TRequest content, object? headers = null)
        {
            return SendRequest<TRequest>(HttpMethod.Put, url, content, headers);
        }

        /*************************************************************************/
        public Task<TResponse> Patch<TRequest, TResponse>(string url, TRequest? content = default(TRequest?), object? headers = null)
        {
            return SendRequest<TRequest, TResponse>(HttpMethod.Patch, url, content, headers);
        }

        /*************************************************************************/
        public Task Patch<TRequest>(string url, TRequest? content = default(TRequest?), object? headers = null)
        {
            return SendRequest<TRequest>(HttpMethod.Patch, url, content, headers);
        }

        /*************************************************************************/
        public Task Delete(string url, object? headers = null)
        {
            return SendRequest<object>(HttpMethod.Delete, url, null, headers);
        }

        #endregion
    }
}
