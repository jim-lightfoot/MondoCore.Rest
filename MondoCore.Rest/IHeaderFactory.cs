using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MondoCore.Rest
{
    /*************************************************************************/
    /*************************************************************************/
    /// <summary>
    /// Interface for add headers for a Rrest API
    /// </summary>
    public interface IHeaderFactory
    {
        Task<IDictionary<string, string>> GetHeaders(string apiName);   
    }
}
