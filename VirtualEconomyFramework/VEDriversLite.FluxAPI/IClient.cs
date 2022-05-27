using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.FluxAPI
{
    public partial interface IClient
    {
        /// <summary>Get command to Apps API</summary>
        /// <param name="command">for example "locations"</param>
        /// <returns>ResponseDto</returns>
        System.Threading.Tasks.Task<ResponseDto<T>> GetAppsAsync<T>(string command);
        
        /// <summary>Get command to Explorer API</summary>
        /// <param name="command">for example "utxo"</param>
        /// <returns>ResponseDto</returns>
        System.Threading.Tasks.Task<ResponseDto<T>> GetExplorerAsync<T>(string command, string[] parameters);
    }

}
