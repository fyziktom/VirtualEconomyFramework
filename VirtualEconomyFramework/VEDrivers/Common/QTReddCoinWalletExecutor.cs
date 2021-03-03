using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace VEDrivers.Common
{
    public class QTReddCoinWalletExcecutorConfig
    {
        public string Exe { get; set; }
        public string Args { get; set; }
        public bool Mandatory { get; set; }
        public int StartupTime { get; set; }
    }
    public class QTReddCoinWalletExcecutor : IHostedService
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IHostApplicationLifetime lifetime;
        private readonly string fullPath;
        private readonly string arguments;
        private readonly bool mandatory;
        private readonly int startupTime;

        private Process process;

        public QTReddCoinWalletExcecutor(IConfiguration settings, IHostApplicationLifetime lifetime)
        {
            var config = settings.GetSection("QTReddCoinWalletExecutor").Get<QTReddCoinWalletExcecutorConfig>();
            if (config == null)
            {
                log.Fatal("QTWalletExecutor section not found in config");
                throw new Exception();
            }
            fullPath = config.Exe;
            arguments = config.Args;
            startupTime = config.StartupTime;
            mandatory = config.Mandatory;
            this.lifetime = lifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            var info = new ProcessStartInfo()
            {
                FileName = fullPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = false,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            bool exited = false;
            bool monitoring = false;
            process = new Process();
            process.StartInfo = info;
            process.EnableRaisingEvents = true;

            var source = new CancellationTokenSource();
            process.Exited += (sender, e) =>
            {
                lock (this)
                {
                    if (source != null) source.Cancel();
                }
                Volatile.Write(ref exited, true);
                var code = process.ExitCode;
                if (!Volatile.Read(ref monitoring))
                {
                    var errors = process.StandardError.ReadToEnd();
                    if (string.IsNullOrEmpty(errors)) errors = process.StandardOutput.ReadToEnd();
                    if (code != 0) log.Error($"Process '{fullPath}' exited with exit code {code}{(string.IsNullOrEmpty(errors) ? "" : Environment.NewLine)}{errors}");
                }
                if (mandatory)
                {
                    log.Fatal("Required process not running");
                    lifetime.StopApplication();
                }
            };
            try
            {
                process.Start();
                log.Info("QTWallet started");
            }
            catch (Exception ex)
            {

                if (mandatory)
                {
                    log.Fatal($"Cannot start mandatory proces '{fullPath}'", ex);
                    lifetime.StopApplication();
                }
                else log.Error($"Cannot start proces '{fullPath}'", ex);
                return;
            }

            try
            {
                await Task.Delay(startupTime, source.Token); //wait for node to bind socket
            }
            catch { return; }
            lock (this)
            {
                source.Dispose();
                source = null;
            }
            if (!Volatile.Read(ref exited))
            {
                process.OutputDataReceived += (s, e) => { if (log.IsDebugEnabled) log.Debug($"process output: {e.Data}"); };
                process.ErrorDataReceived += (s, e) => { if (log.IsDebugEnabled) log.Debug($"process  error: {e.Data}"); };
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                Volatile.Write(ref monitoring, true);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (process != null)
            {
                try { process.Kill(); } catch { }; //not interrested in failures
                process.Dispose();
                process = null;
            }
            return Task.CompletedTask;
        }
    }
}
