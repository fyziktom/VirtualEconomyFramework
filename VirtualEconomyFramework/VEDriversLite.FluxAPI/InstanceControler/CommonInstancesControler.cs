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
    public abstract class CommonInstancesControler : IInstancesControler
    {
        /// <summary>
        /// Instances controler type
        /// </summary>
        public InstancesControlerType Type { get; set; } = InstancesControlerType.Basic;
        /// <summary>
        /// Instances controler name
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Name of the service in the Flux
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;
        /// <summary>
        /// Service ID in the Flux
        /// </summary>
        public string ServiceId { get; set; } = string.Empty;
        /// <summary>
        /// Dictionary of the Instances
        /// </summary>
        public ConcurrentDictionary<string, IInstance> Instances { get; set; } = new ConcurrentDictionary<string, IInstance>();

        /// <summary>
        /// Cancellation token source to cancel the running tasks in instances
        /// </summary>
        public CancellationTokenSource? TaskCancellationTokenSource { get; set; }

        #region PrivateMembers

        private volatile int bussy = 0;
        private AsyncManualResetEvent wait = new AsyncManualResetEvent();
        private Common.Encryption.MD5 md5 = new Common.Encryption.MD5();
        private static object _lock = new object();
        
        #endregion

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
        public virtual async Task<bool> Initialize(string name, 
                                                   string serviceName, 
                                                   string serviceId, 
                                                   bool initAll = false, 
                                                   int port = 0, 
                                                   string username = "", 
                                                   string password = "")
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name), "Name cannot be null or empty");
            if (string.IsNullOrEmpty(serviceName))
                throw new ArgumentNullException(nameof(serviceName), "ServiceName cannot be null or empty");
            if (string.IsNullOrEmpty(serviceId))
                throw new ArgumentNullException(nameof(serviceId), "ServiceId cannot be null or empty");
            Name = name;
            ServiceName = serviceName;
            ServiceId = serviceId;
            
            try
            {
                Console.WriteLine($" Loading the {serviceName} instances list...");
                var list = await FluxAPIHelpers.GetListOfAppsLocations(serviceName);
                if (list == null) return false;
                if (list.Count == 0) return false;

                Console.WriteLine($" Loaded info about {list.Count} of {serviceName} instances.");
                Console.WriteLine($" Initializing {list.Count} of {serviceName} instances...");
                foreach (var item in list)
                {
                    IInstance instance = null;
                    instance = InstanceFactory.GetInstance(InstanceType.APIService);
                    if (instance == null) continue;
                    var settings = new Common.IoT.Dto.IoTDataDriverSettings()
                    {
                        IoTComType = Common.IoT.Dto.IoTCommunicationType.API,
                        ComSchemeType = Common.IoT.Dto.CommunicationSchemeType.Requests,
                        ConnectionParams = new Common.IoT.Dto.CommonConnectionParams()
                        {
                            Encrypted = false,
                            Secured = false,
                            SType = Common.IoT.Dto.CommunitacionSecurityType.SSL,
                            IP = item.IP,
                            DeviceId = item.Hash,
                            Username = username,
                            Password = password,
                        }
                    };
                    
                    instance.Name = item.Hash;
                    instance.ExpireAt = item.ExpireAt;
                    instance.MeasureTime = true;
                    
                    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                        settings.ConnectionParams.Secured = true;
                    
                    if (port > 0)
                        settings.ConnectionParams.Port = port;
                    else
                    {
                        if (item.IP.Contains(":"))
                        {
                            Console.WriteLine($"double info about port...skiping this instance {item.IP}.");
                            continue;
                            /*
                            var ip = item.IP.Split(':')[0];
                            var prt = item.IP.Split(':')[1];
                            var portStr = prt?.Split('/')[0];
                            if (int.TryParse(portStr ?? "0", out int portNum))
                                if (portNum > 0)
                                    settings.ConnectionParams.Port = portNum;
                            settings.ConnectionParams.IP = ip ?? item.IP; */
                        }
                        else
                        {
                            settings.ConnectionParams.Port = 34909;
                        }
                    }

                    if (initAll)
                    {
                        var res = await instance.InitInstance(settings);
                        if (res.Success)
                        {
                            Instances.TryAdd($"{item.Hash}-{item.IP}", instance);
                        }
                        Console.WriteLine($"Instance - {instance.Name}, {item.Hash}-{item.IP} - initialized connection with result: {res.Success}, {res.Value}");
                    }
                }

                Console.WriteLine($" Initialized.");
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Ask new request
        /// For example some user ask API for some info
        /// When multiple users will ask same topic and parameters it will request final instance just once and give result all
        /// </summary>
        /// <param name="taskrequest"></param>
        /// <returns></returns>
        public virtual async Task<CommonReturnTypeDto> Request(TaskToRunRequestDto taskrequest)
        {
            bool master = false;
            string taskId = string.Empty;

            lock (_lock)
            {
                md5.Value = (taskrequest.Topic + taskrequest.Parameters);
                taskId = md5.FingerPrint;
            }

            var instanceWithSameTask = CheckIfRequestAlreadyExists(taskId).Where(i => i.IsConnected).MinBy(i => i.AveragePingTime);
            if (instanceWithSameTask != null)
            {
                Console.WriteLine($"Request for {taskrequest.Topic} with parameters {taskrequest.Parameters} already exists in {instanceWithSameTask.Name}. Client {taskrequest.ClientId} will get result from the existing instance.");
                var tasktorun = instanceWithSameTask.GetTask(taskId);
                if (tasktorun.Success)
                    return await (tasktorun.Value as TaskToRun).RunTask(instanceWithSameTask.Type, instanceWithSameTask.IDDSettings);
            }

            IInstance instance = InstanceFactory.GetInstance(InstanceType.None);            
            try
            {
                if (Instances.Count > 0)
                {
                    var inst = Instances.Values.Where(i => i.IsConnected).MinBy(i => i.AveragePingTime);
                    if (inst != null)
                        instance = inst;

                    if (instance == null) return CommonReturnTypeDto.GetNew<TaskToRunResponseDto>();

                    var ts = instance.AddTask(taskrequest);
                    if (ts.Item1)
                    {
                        taskId = ts.Item2;
                        instance.TaskFinished -= TaskFinishedHandler;
                        instance.TaskFinished += TaskFinishedHandler;
                        await instance.ProcessAllTasks();
                    }


                    if (instance == null) return CommonReturnTypeDto.GetNew<TaskToRunResponseDto>();

                    if (instance.ProcessedTasks.TryGetValue(taskId, out var finishedTask))
                    {
                        return CommonReturnTypeDto.GetNew<TaskToRunResponseDto>(true, finishedTask.GetResponse());
                    }
                    else
                    {
                        return CommonReturnTypeDto.GetNew<TaskToRunResponseDto>();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
            return CommonReturnTypeDto.GetNew<TaskToRunResponseDto>();
        }
        
        private void TaskFinishedHandler(object sender, string e)
        {
            wait.Set();
        }

        private IEnumerable<IInstance> CheckIfRequestAlreadyExists(string taskId)
        {
            foreach (var instance in Instances)
            {
                var res = instance.Value.HasTask(taskId);
                if (res.Success)
                    yield return instance.Value;
            }
        }
    }
}
