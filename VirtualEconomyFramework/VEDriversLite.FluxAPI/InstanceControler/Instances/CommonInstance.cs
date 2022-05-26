﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common;
using VEDriversLite.Common.IoT.Dto;
using VEDriversLite.FluxAPI.InstanceControler.Instances.Dto;

namespace VEDriversLite.FluxAPI.InstanceControler.Instances
{
    public abstract class CommonInstance : IInstance
    {
        /// <summary>
        /// Instance Type
        /// </summary>
        public InstanceType Type { get; set; } = InstanceType.APIService;
        /// <summary>
        /// Instance Focus
        /// </summary>
        public InstanceFocus Focus { get; set; } = InstanceFocus.CPUCalculation;
        /// <summary>
        /// Service expire at date
        /// </summary>
        public DateTime ExpireAt { get; set; } = DateTime.MaxValue;
        /// <summary>
        /// Name of the instance
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Flag indicates Is connection live
        /// </summary>
        public bool IsConnected { get; set; } = false;
        /// <summary>
        /// Is set when Instance processing tasks
        /// </summary>
        public bool IsProcessing { get; set; } = false;
        /// <summary>
        /// Time of the last ping command response RoundtripTime
        /// </summary>
        public long LastPingRoundtripTime { get; set; }
        /// <summary>
        /// Connection settings
        /// </summary>
        public IoTDataDriverSettings IDDSettings { get; set; } = new IoTDataDriverSettings();
        /// <summary>
        /// Set if you want to measure time of the task processing
        /// </summary>
        public bool MeasureTime { get; set; } = false;
        /// <summary>
        /// Average Ping response time
        /// </summary>
        public long AveragePingTime { get; set; } = 0;
        /// <summary>
        /// AverageTask response time
        /// </summary>
        public TimeSpan AverageResponseTime { get; set; } = TimeSpan.Zero;
        /// <summary>
        /// Dictionary of the tasks to process
        /// </summary>        
        public ConcurrentDictionary<string, TaskToRun> TasksToProcess { get; set; } = new ConcurrentDictionary<string, TaskToRun>();
        /// <summary>
        /// Dictionary of the tasks in process
        /// </summary>
        public ConcurrentDictionary<string, TaskToRun> TasksInProcess { get; set; } = new ConcurrentDictionary<string, TaskToRun>();
        /// <summary>
        /// Dictionary of the tasks after process
        /// </summary>
        public ConcurrentDictionary<string, TaskToRun> ProcessedTasks { get; set; } = new ConcurrentDictionary<string, TaskToRun>();
        /// <summary>
        /// Task was finished
        /// It provides the ID of the task
        /// </summary>
        public EventHandler<string> TaskFinished { get; set; }
        /// <summary>
        /// Task was failed
        /// It provides the ID of the task
        /// </summary>        
        public EventHandler<string> TaskFailed { get; set; }

        /// <summary>
        /// Process Next task in the line
        /// </summary>
        /// <returns></returns>
        public abstract Task ProcessNextTask();


        /// <summary>
        /// History of requets
        /// </summary>
        public List<TaskToRunRequestDto> TaskRequestsHistory { get; set; } = new List<TaskToRunRequestDto>();
        /// <summary>
        /// History of responses
        /// </summary>
        public List<TaskToRunResponseDto> TaskResponseHistory { get; set; } = new List<TaskToRunResponseDto>();


        #region PrivateProperties 

        private Ping ping = new Ping();
        private List<long> pingTimeHistory = new List<long>();
        private List<TimeSpan> taskTimeHistory = new List<TimeSpan>();

        private System.Timers.Timer? connectionCheckTimer;

        private int maximumSavedInHistory = 100;
        private int checkInterval = 10000;

        private Common.Encryption.MD5 md5 = new Common.Encryption.MD5();

        private static object _lock = new object();
        
        #endregion

        #region CommonFunctions

        public void FireTaskFinished(object sender, string e)
        {
            TaskFinished?.Invoke(sender, e);
        }

        /// <summary>
        /// Init the instance
        /// </summary>
        public virtual async Task<CommonReturnTypeDto> InitInstance(IoTDataDriverSettings connectionSettings)
        {
            DisposeConnectionCheckTimer();
            connectionCheckTimer = new System.Timers.Timer(checkInterval);
            connectionCheckTimer.Elapsed += ConnectionCheckTimer_Elapsed;
            connectionCheckTimer.Enabled = true;
            connectionCheckTimer.AutoReset = true;
            connectionCheckTimer.Start();

            return new CommonReturnTypeDto() { Success = true };
        }

        private void ConnectionCheckTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            TestConnection();
        }

        private void DisposeConnectionCheckTimer()
        {
            if (connectionCheckTimer != null)
            {
                connectionCheckTimer.Stop();
                connectionCheckTimer.Dispose();
            }
        }

        /// <summary>
        /// Test connection to instance - PING
        /// </summary>
        /// <returns></returns>
        public async Task<(bool,long)> TestConnection()
        {
            if (string.IsNullOrEmpty(IDDSettings.ConnectionParams.IP)) return (false,0);
            try
            {
                var ip = IDDSettings.ConnectionParams.IP;
                if (ip.Contains(":")) ip = ip.Split(':')[0];
                var result = await ping.SendPingAsync(IDDSettings.ConnectionParams.IP, 1000);
                if (result.Status == IPStatus.Success)
                {
                    IsConnected = true;
                    LastPingRoundtripTime = result.RoundtripTime;
                    pingTimeHistory.Add(LastPingRoundtripTime);
                    if (pingTimeHistory.Count > maximumSavedInHistory)
                        pingTimeHistory.RemoveAt(0);

                    AveragePingTime = (long)pingTimeHistory.Average();
                    return (true, result.RoundtripTime);
                }
                else
                {
                    IsConnected = false;
                    LastPingRoundtripTime = 0;
                    return (false, 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot ping the instance: {Name}, error: {ex.Message}");
                IsConnected = false;
                LastPingRoundtripTime = 0;
                return (false, 0);
            }
        }

        /// <summary>
        /// Deinitialize the instance
        /// </summary>
        /// <returns></returns>
        public virtual bool DeInit()
        {
            if (connectionCheckTimer != null)
            {
                connectionCheckTimer.Stop();
                connectionCheckTimer.Dispose();
                connectionCheckTimer = null;
            }
            return true;
        }

        /// <summary>
        /// Add new task to process
        /// </summary>
        /// <param name="task"></param>
        /// <returns>returns false if already exists</returns>
        public virtual (bool,string) AddTask(TaskToRunRequestDto taskrequest)
        {
            if (taskrequest == null) 
                return (false, string.Empty);
            else
                TaskRequestsHistory.Add(taskrequest);

            var task = new TaskToRun();
            task.Fill(taskrequest);
            var taskId = string.Empty;
            lock (_lock)
            {
                md5.Value = (taskrequest.Topic + taskrequest.Parameters);
                taskId = md5.FingerPrint;
            }

            if (TasksToProcess.ContainsKey(taskId) || TasksInProcess.ContainsKey(taskId) || ProcessedTasks.ContainsKey(taskId))
            {
                return (true, taskId);
            }
            else
            {
                TasksToProcess.TryAdd(taskId, task);
                return (true, taskId);
            }
        }
        /// <summary>
        /// Cancel task if exists
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns>returns false if not exists</returns>
        public virtual bool CancelTask(string taskId)
        {
            if (TasksInProcess.TryGetValue(taskId, out var task))
            {
                task.Cancel();
                return true;
            }
            return false;
        }
        /// <summary>
        /// Remove task if exists
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns>returns false if not exists</returns>
        public virtual bool RemoveTask(string taskId)
        {
            if (TasksToProcess.TryRemove(taskId, out var task))
            {
                task.Cancel();
                return true;
            }
            else if (TasksInProcess.TryRemove(taskId, out task))
            {
                task.Cancel();
                return true;
            }
            else if (ProcessedTasks.TryRemove(taskId, out task))
            {
                task.Cancel();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if the Instance has in the list same topic+params. 
        /// It can just wait for the result then.
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="topic"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual CommonReturnTypeDto HasTask(string taskId, string topic = "", string parameters = "")
        {
            var fp = string.Empty;
            if (string.IsNullOrEmpty(taskId))
            {
                lock (_lock)
                {
                    md5.Value = (topic + parameters);
                    fp = md5.FingerPrint;
                }
            }
            else
            {
                fp = taskId;
            }

            if (TasksToProcess.ContainsKey(fp) || TasksInProcess.ContainsKey(fp) || ProcessedTasks.ContainsKey(fp))
                return CommonReturnTypeDto.GetNew(true, fp);
            else
                return CommonReturnTypeDto.GetNew<string>();
        }

        /// <summary>
        /// Get if task if exists
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="topic"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual CommonReturnTypeDto GetTask(string taskId, string topic = "", string parameters = "")
        {
            var fp = string.Empty;
            if (string.IsNullOrEmpty(taskId))
            {
                lock (_lock)
                {
                    md5.Value = (topic + parameters);
                    fp = md5.FingerPrint;
                }
            }
            else
            {
                fp = taskId;
            }

            if (TasksToProcess.TryGetValue(fp, out var taskToRun))
                return CommonReturnTypeDto.GetNew(true, taskToRun);
            else if (TasksInProcess.TryGetValue(fp, out taskToRun))
                return CommonReturnTypeDto.GetNew(true, taskToRun);
            else if (ProcessedTasks.TryGetValue(fp, out taskToRun))
                return CommonReturnTypeDto.GetNew(true, taskToRun);
            else
                return CommonReturnTypeDto.GetNew<TaskToRun>();
        }

        /// <summary>
        /// Process one task
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public virtual async Task<CommonReturnTypeDto> ProcessTask(string taskId)
        {
            try
            {
                //IsProcessing = true;
                if (TasksToProcess.TryGetValue(taskId, out var task))
                {
                    if (!task.IsRunning && !task.IsCompleted)
                    {
                        if (TasksToProcess.TryRemove(taskId, out var taskToStart))
                        {
                            taskToStart.IsRunning = true;
                            TasksInProcess.TryAdd(taskId, taskToStart);
                        }
                    }
                }

                var tasks = new Task[TasksInProcess.Count];
                if (TasksInProcess.TryGetValue(taskId, out task))
                {
                    CommonReturnTypeDto response = CommonReturnTypeDto.GetNew<TaskToRunResponseDto>();
                    try
                    {
                        response = await task.RunTask(Type, IDDSettings);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot process tasks.");
                    }
                    if (TasksInProcess.TryRemove(taskId, out var taskToFinish))
                    {
                        taskToFinish.IsRunning = false;
                        ProcessedTasks.TryAdd(taskId, taskToFinish);
                        TaskFinished?.Invoke(this, taskId);
                    }
                    return response;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot process tasks.");
            }
            finally
            {
                //IsProcessing = false;
            }
            return CommonReturnTypeDto.GetNew<TaskToRunResponseDto>();
        }

        /// <summary>
        /// Process All Tasks
        /// </summary>
        /// <returns></returns>
        public virtual async Task ProcessAllTasks(bool runOnBackground = false)
        {
            var skip = 0;
            try
            {
                IsProcessing = true;

                while (TasksToProcess.Count > (0 + skip))
                {
                    var task = TasksToProcess.FirstOrDefault();
                    if (task.Value == null) break;

                    if (!task.Value.IsRunning && !task.Value.IsCompleted)
                    {
                        if (TasksToProcess.TryRemove(task.Key, out var taskToStart))
                        {
                            taskToStart.IsRunning = true;
                            TasksInProcess.TryAdd(task.Key, taskToStart);
                        }
                    }
                    else
                    {
                        skip++;
                    }
                    if (skip >= TasksToProcess.Count)
                        break;
                }

                var tasks = new Task[TasksInProcess.Count];

                var i = 0;
                foreach (var task in TasksInProcess)
                {
                    tasks[i] = task.Value.RunTask(Type, IDDSettings);
                    i++;
                }

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot process tasks.");
                }

                skip = 0;
                while (TasksInProcess.Count > (0 + skip))
                {
                    var task = TasksInProcess.FirstOrDefault();
                    if (task.Value == null) break;

                    if (TasksInProcess.TryRemove(task.Key, out var taskToFinish))
                    {
                        taskToFinish.IsRunning = false;
                        ProcessedTasks.TryAdd(task.Key, taskToFinish);
                        TaskFinished?.Invoke(this, task.Key);
                    }
                    else
                    {
                        skip++;
                    }
                    if (skip >= TasksInProcess.Count)
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot process tasks.");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        #endregion

    }
}
