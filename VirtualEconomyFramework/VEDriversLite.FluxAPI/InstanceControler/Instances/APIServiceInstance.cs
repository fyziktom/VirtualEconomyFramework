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
    public class APIServiceInstance : CommonInstance
    {
        public override async Task<CommonReturnTypeDto> InitInstance(IoTDataDriverSettings connectionSettings)
        {
            var res = await base.InitInstance(connectionSettings);
            if (res.Success)
            {
                if (connectionSettings == null)
                    return new CommonReturnTypeDto() { Success = false, Value = "Connection settings is null" };

                IDDSettings = connectionSettings;
                var result = await TestConnection();
                if (result.Item1)
                    return new CommonReturnTypeDto() { Success = true, Value = $"{connectionSettings.ConnectionParams.IP} is Connected" };
            }    
            return new CommonReturnTypeDto() { Success = true, Value = $"{connectionSettings.ConnectionParams.IP} is Not Connected" };
        }
        public override async Task ProcessAllTasks(bool runOnBackground = false)
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

        public override Task ProcessNextTask()
        {
            return Task.CompletedTask;
        }

    }
}
