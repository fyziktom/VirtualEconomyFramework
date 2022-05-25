using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common.IoT.Dto;

namespace VEDriversLite.FluxAPI.InstanceControler.Instances.Dto
{
    /// <summary>
    /// Request Dto to run task in instances
    /// </summary>
    public class TaskToRunRequestDto
    {
        /// <summary>
        /// Unique ID of the task
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Request added At DateTime - UTC Time
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Topic on the final server app
        /// for example url to API command
        /// for example MQTT topic
        /// </summary>
        public string Topic { get; set; } = string.Empty;
        /// <summary>
        /// Parameters for request to server app on instance
        /// </summary>
        public string Parameters { get; set; } = string.Empty;
        /// <summary>
        /// Run task as parallel task
        /// </summary>
        public bool IsParallel { get; set; } = true;
        /// <summary>
        /// ID of the tasks which must be processed before this task
        /// </summary>
        public string[] Prerequisities { get; set; } = new string[0];
        /// <summary>
        /// Default priority is 0. It means standard priority.
        /// 1 - is lowes
        /// 100 - is highest
        /// Higher values are reserved
        /// </summary>
        public byte Priority { get; set; } = 0;
        /// <summary>
        /// Prefered to run on these Instances IDs
        /// </summary>
        public string[] PreferedInstances { get; set; } = new string[0];
    }
    /// <summary>
    /// Result of the task run. It contains also input parameters
    /// </summary>
    public class TaskToRunResponseDto : TaskToRunRequestDto
    {
        /// <summary>
        /// Indicates completed task
        /// </summary>
        public bool IsCompleted { get; set; } = false;        
        /// <summary>
        /// Indicates error during task
        /// </summary>
        public bool IsError { get; set; } = false;
        /// <summary>
        /// Output message from the task
        /// </summary>
        public string OutputMessage { get; set; } = string.Empty;
        /// <summary>
        /// Output data array from task
        /// </summary>
        public byte[] TaskOutputData { get; set; } = new byte[0];
        /// <summary>
        /// Time result from the time measurement
        /// </summary>
        public TimeSpan Time { get; set; } = TimeSpan.Zero;
    }    
    /// <summary>
    /// Task To Run
    /// </summary>
    public class TaskToRun : TaskToRunResponseDto
    {
        /// <summary>
        /// Requested task to process
        /// </summary>
        public Task RequestedTask { get; set; } = Task.Delay(1);
        /// <summary>
        /// Cancellation token source to cancel the running task
        /// </summary>
        public CancellationTokenSource? TaskCancellationTokenSource { get; set; }

        /// <summary>
        /// Indicates running task
        /// </summary>
        public bool IsRunning { get; set; } = false;
        
        /// <summary>
        /// Cancel task
        /// </summary>
        /// <returns></returns>
        public bool Cancel()
        {
            if (IsRunning)
            {
                if (TaskCancellationTokenSource != null)
                {
                    TaskCancellationTokenSource.Cancel();
                    return true;
                }
            }
            return false;
        }

        public void Fill(TaskToRunRequestDto request)
        {
            Topic = request.Topic;
            Parameters = request.Parameters;
            IsParallel = request.IsParallel;
            Prerequisities = request.Prerequisities;
            Priority = request.Priority;
            PreferedInstances = request.PreferedInstances;
        }

        public TaskToRunResponseDto GetResponse()
        {
            return new TaskToRunResponseDto()
            {
                Id = Id,
                CreatedAt = CreatedAt,
                Topic = Topic,
                Parameters = Parameters,
                IsParallel = IsParallel,
                Prerequisities = Prerequisities,
                Priority = Priority,
                PreferedInstances = PreferedInstances,
                IsCompleted = IsCompleted,
                IsError = IsError,
                OutputMessage = OutputMessage,
                TaskOutputData = TaskOutputData,
                Time = Time
            };
        }

        public async Task RunTask(InstanceType type, IoTDataDriverSettings idds)
        {
            if (type == InstanceType.APIService)
            {
                var httpClient = new HttpClient();

                if (idds.ConnectionParams.Secured)
                {
                    var byteArray = Encoding.ASCII.GetBytes(idds.ConnectionParams.Username + ":" + idds.ConnectionParams.Password);
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    //httpClient.DefaultRequestHeaders.Add("Origin", "https://MYORIGIN.com");
                }
                //httpClient.DefaultRequestHeaders.Add("mode", "no-cors");

                HttpResponseMessage resp = null;
                try
                {
                    var url = string.Empty;
                    if (idds.ConnectionParams.Port != 80)
                        url = $"http://{idds.ConnectionParams.IP}:{idds.ConnectionParams.Port}/{Topic}{Parameters}";
                    else
                        url = $"http://{idds.ConnectionParams.IP}/{Topic}{Parameters}";
                    
                    resp = await httpClient.GetAsync(url);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Cannot request: " + ex.Message);
                }
                if (resp != null)
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        var data = await resp.Content.ReadAsByteArrayAsync();
                        TaskOutputData = data;
                        IsCompleted = true;
                        IsError = false;
                    }
                    else
                    {
                        IsError = true;
                        OutputMessage = $"Error {resp.StatusCode}";
                    }
                }
            }
            else
            {
                await Task.CompletedTask;
            }
        }
    }
}
