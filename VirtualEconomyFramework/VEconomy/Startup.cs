using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Interfaces;
using log4net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VEDrivers.Economy.Exchanges;
using VEconomy.Common;
using MQTTnet.Server;
using MQTTnet.AspNetCore;
using MQTTnet.AspNetCore.Extensions;
using VEDrivers.Common;
using MQTTnet.Protocol;

namespace VEconomy
{
    public class Startup
    {
        private static readonly ILog log = LogManager.GetLogger("AccessLog");

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers(o => o.Filters.Add(new HttpResponseExceptionFilter()))
                .AddNewtonsoftJson(o =>
                {
                    o.UseMemberCasing();  //generates identical javascript and c# names
                    o.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;  //easier javascript work with ".Include"
                    if (Environment.IsDevelopment()) o.SerializerSettings.Formatting = Formatting.Indented;
                });


            var config = new MQTTConfig();
            Configuration.GetSection("MQTT").Bind(config);

            services
                .AddHostedMqttServer(mqttServer =>
                mqttServer
                        .WithConnectionValidator(c => { if (c.Username != config.User) return; if (c.Password != config.Pass) return; c.ReasonCode = MqttConnectReasonCode.Success; })
                        .WithoutDefaultEndpoint())
                .AddMqttConnectionHandler()
                .AddConnections();

            services.AddSwaggerGen();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                if (TimeSpan.TryParse(Configuration["LoginExpiration"], out var span))
                {
                    options.ExpireTimeSpan = span;
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Environment = env;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection(); //no ssl support planned

            app.Use(async (context, next) =>
            {
                var watch = new Stopwatch();
                watch.Start();
                await next.Invoke();
                watch.Stop();
                if (context.Response.StatusCode < 400)
                {
                    log.Info($"Reply {context.Response.StatusCode} in {watch.ElapsedMilliseconds,5} ms to [{context.Connection.RemoteIpAddress}] | {context.Request.Method} {context.Request.GetDisplayUrl()}'");
                }
                else
                {
                    log.Warn($"Reply {context.Response.StatusCode} in {watch.ElapsedMilliseconds,5} ms to [{context.Connection.RemoteIpAddress}] | {context.Request.Method} {context.Request.GetDisplayUrl()}'");
                }
            });

            app.UseFileServer(); //statické stranky, path v appconfig.json

            app.UseSwagger();
            //if (env.IsDevelopment())
            //{
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "VEconomy API V1");
            });
            //}
            app.UseRouting();

            app.UseCors(c => c.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMqtt("/mqtt");

                
            });

            EconomyMainContext.MQTTServerIsStarted = true;
            app.UseMqttServer(server =>
            {
                //nothing to do
            });
        }
    }
}
