using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common;
using VEDriversLite.FluxAPI.InstanceControler.Instances;
using VEDriversLite.FluxAPI.InstanceControler.Instances.Dto;

namespace VEDriversLite.FluxAPI.InstanceControler
{
    /// <summary>
    /// Instances controler type
    /// </summary>
    public enum InstancesControlerType
    {
        /// <summary>
        /// Bacis controler for multiple instances of some service
        /// </summary>
        Basic,
        /// <summary>
        /// Fake type for tests
        /// </summary>
        Fake            
    }
    /// <summary>
    /// Interface for Services Intsances controlers
    /// </summary>
    public interface IInstancesControler
    {
        /// <summary>
        /// Instances controler type
        /// </summary>
        InstancesControlerType Type { get; set; }
        /// <summary>
        /// Instances controler name
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Name of the service in the Flux
        /// </summary>
        string ServiceName { get; set; }
        /// <summary>
        /// Service ID in the Flux
        /// </summary>
        string ServiceId { get; set; }
        /// <summary>
        /// Dictionary of the Instances
        /// </summary>
        ConcurrentDictionary<string,IInstance> Instances { get; set; }

        /// <summary>
        /// Cancellation token source to cancel the running tasks in instances
        /// </summary>
        public CancellationTokenSource? TaskCancellationTokenSource { get; set; }


        /// <summary>
        /// Find the all instances of the service and load them to list
        /// </summary>
        /// <param name="initAll">If you want to initialize the services after they are found set this true</param>
        /// <param name="name">Name of controller</param>
        /// <param name="serviceName">Name of the App in Flux</param>
        /// <param name="serviceId">ID of the App in Flux</param>
        /// <param name="port">Port</param>
        /// <param name="username">Username for end service</param>
        /// <param name="password">Password for end service</param>
        /// <returns>true if at least one service has been found</returns>
        Task<bool> Initialize(string name,
                              string serviceName,
                              string serviceId,
                              bool initAll = false,
                              int port = 0,
                              string username = "",
                              string password = "");

        /// <summary>
        /// Ask new request
        /// For example some user ask API for some info
        /// When multiple users will ask same topic and parameters it will request final instance just once and give result all
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        Task<CommonReturnTypeDto> Request(TaskToRunRequestDto task);
        
    }
}
