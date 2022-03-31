using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.UnstoppableDomains
{
    public partial interface IClient
    {
        /// <summary>Returns the Details of Unstoppable Domain address</summary>
        /// <param name="address">Crypto address</param>
        /// <returns>Object containing address details, empty object is returned.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetAddressDetailsResponse> GetAddressDetailsAsync(string address);
        /// <summary>
        /// Returns the address details from the Unstoppable Domain API
        /// </summary>
        /// <param name="address"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        System.Threading.Tasks.Task<GetAddressDetailsResponse> GetAddressDetailsAsync(string address, System.Threading.CancellationToken cancellationToken);

    }

}
