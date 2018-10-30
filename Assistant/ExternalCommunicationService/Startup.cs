//-----------------------------------------------------------------------

// <copyright file="Startup.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using AssistantInternalInterfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;

namespace ExternalCommunicationService
{
    class Startup
    {
        /// <summary>
        /// This is the current logger instance
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public const int port = 4042;

        public static IServiceProvider ServiceProvider { get; private set; }
        public static T GetService<T>() { return ServiceProvider.GetRequiredService<T>(); }

        private static bool ValidateIPAddress(string ipAddress)
        {
            // Set the default return value to false
            bool isIPAddress = false;

            // Set the regular expression to match IP addresses
            string ipPattern = @"\b(^192\.168\.)\b";

            // Create an instance of System.Text.RegularExpressions.Regex
            Regex regex = new Regex(ipPattern);

            // Validate the IP address
            isIPAddress = regex.IsMatch(ipAddress);

            return isIPAddress;
        }
        public static string GetIpAddr()
        {
            
            string ip = "";
            foreach (var interf in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (var unicastAddr in interf.GetIPProperties().UnicastAddresses)
                {
                    if (unicastAddr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ip = unicastAddr.Address.ToString();
                        if (ValidateIPAddress(ip) == false)
                        {
                            ServiceEventSource.Current.Message($"Not valid network interface with ip {ip}");
                            logger.Debug($"Not valid network interface with ip {ip}");
                            continue;
                        }
                        ServiceEventSource.Current.Message($"Found network interface with ip {ip}");
                        logger.Debug($"Found network interface with ip {ip}");
                        return ip;
                    }
                }
            }
            return null;
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            var ipAddress = $"http://{GetIpAddr() ?? ""}:{port}";
            services.AddCors(options => options.AddPolicy("CorsPolicy",
            builder =>
            {
                builder.AllowAnyMethod().AllowAnyHeader()
                       .WithOrigins(ipAddress)
                       .AllowCredentials();
            }));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSignalR().AddHubOptions<ExternalCommunicationHub>(options =>
            {
                options.EnableDetailedErrors = true;
                options.SupportedProtocols = new List<string> { "http", "json" };
                options.KeepAliveInterval = TimeSpan.MaxValue;
            });
            services.AddSingleton(typeof(IClientProxy), typeof(ClientProxy));

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseCors("CorsPolicy");

            app.UseSignalR(route =>
            {

                route.MapHub<ExternalCommunicationHub>("/ecom");

            });
            ServiceProvider = app.ApplicationServices;
            app.UseMvc();
            
        }
    }
}
