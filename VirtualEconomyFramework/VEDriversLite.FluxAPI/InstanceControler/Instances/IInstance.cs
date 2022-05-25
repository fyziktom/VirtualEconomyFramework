using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common;
using VEDriversLite.Common.IoT.Dto;
using VEDriversLite.FluxAPI.InstanceControler.Instances.Dto;

namespace VEDriversLite.FluxAPI.InstanceControler.Instances
{
    /// <summary>
    /// Instance types
    /// </summary>
    public enum InstanceType
    {
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// Common application with API
        /// </summary>
        APIService,
        /// <summary>
        /// IPFS node service
        /// </summary>
        IPFSNode,
        /// <summary>
        /// Neblio node service
        /// </summary>
        NeblioNode,
        /// <summary>
        /// Fake instance for tests
        /// </summary>
        Fake
    }
    /// <summary>
    /// Main focus of the HW which runs the instance
    /// It can be dedicated more to CPU or GPU calculations, etc.
    /// </summary>
    public enum InstanceFocus
    {
        /// <summary>
        /// Classic CPU based calculations
        /// </summary>
        CPUCalculation,
        /// <summary>
        /// Low computing power, mainly storage services
        /// </summary>
        Storage,
        /// <summary>
        /// Parallel calculations in GPU
        /// </summary>
        GPUCalculation,
        /// <summary>
        /// Specific calculations in ASICs or GPU optimized for Artificial Neural Networks
        /// </summary>
        NeuralNetworkCalculations
    }
    public interface IInstance
    {
        /// <summary>
        /// Instance Type
        /// </summary>
        InstanceType Type { get; set; }
        /// <summary>
        /// Instance Focus
        /// </summary>
        InstanceFocus Focus { get; set; }
        /// <summary>
        /// Service expire at date
        /// </summary>
        DateTime ExpireAt { get; set; }
        /// <summary>
        /// Name of the instance
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Flag indicates Is connection live
        /// </summary>
        bool IsConnected { get; set; }
        /// <summary>
        /// Time of the last ping command response RoundtripTime
        /// </summary>
        long LastPingRoundtripTime { get; set; }
        /// <summary>
        /// Connection settings
        /// </summary>
        IoTDataDriverSettings IDDSettings { get; set; }
        /// <summary>
        /// Set if you want to measure time of the task processing
        /// </summary>
        bool MeasureTime { get; set; }

        /// <summary>
        /// Average Ping response time
        /// </summary>
        long AveragePingTime { get; set; }
        /// <summary>
        /// AverageTask response time
        /// </summary>
        TimeSpan AverageResponseTime { get; set; }
        /// <summary>
        /// Dictionary of the tasks to process
        /// </summary>
        ConcurrentDictionary<string, TaskToRun> TasksToProcess { get; set; }
        /// <summary>
        /// Dictionary of the tasks in process
        /// </summary>
        ConcurrentDictionary<string, TaskToRun> TasksInProcess { get; set; }
        /// <summary>
        /// Dictionary of the tasks after process
        /// </summary>
        ConcurrentDictionary<string, TaskToRun> ProcessedTasks { get; set; }
        /// <summary>
        /// History of requets
        /// </summary>
        List<TaskToRunRequestDto> TaskRequestsHistory { get; set; }
        /// <summary>
        /// History of responses
        /// </summary>
        List<TaskToRunResponseDto> TaskResponseHistory { get; set; }
        /// <summary>
        /// Task was finished
        /// It provides the ID of the task
        /// </summary>
        EventHandler<string> TaskFinished { get; set; }
        /// <summary>
        /// Task was failed
        /// It provides the ID of the task
        /// </summary>
        EventHandler<string> TaskFailed { get; set; }
        /// <summary>
        /// Init the instance
        /// </summary>
        Task<CommonReturnTypeDto> InitInstance(IoTDataDriverSettings connectionSettings);
        /// <summary>
        /// Deinitialize the instance
        /// </summary>
        /// <returns></returns>
        bool DeInit();
        /// <summary>
        /// Test connection to instance - PING
        /// </summary>
        /// <returns></returns>
        Task<(bool, long)> TestConnection();
        /// <summary>
        /// Add new task to process
        /// </summary>
        /// <param name="task"></param>
        /// <returns>returns false if already exists</returns>
        (bool,string) AddTask(TaskToRunRequestDto taskrequest);
        /// <summary>
        /// Cancel task if exists
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns>returns false if not exists</returns>
        bool CancelTask(string taskId);
        /// <summary>
        /// Remove task if exists
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns>returns false if not exists</returns>
        bool RemoveTask(string taskId);
        /// <summary>
        /// Check if the Instance has in the list same topic+params. 
        /// It can just wait for the result then.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        CommonReturnTypeDto HasTask(string topic, string parameters);
        /// <summary>
        /// Process Next task in the line
        /// </summary>
        /// <returns></returns>
        Task ProcessNextTask();
        /// <summary>
        /// Process All Tasks
        /// </summary>
        /// <returns></returns>
        Task ProcessAllTasks(bool runOnBackground = false);
    }
}
