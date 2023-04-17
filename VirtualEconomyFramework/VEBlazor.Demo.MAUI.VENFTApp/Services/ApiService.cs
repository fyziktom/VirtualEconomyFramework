using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEBlazor.Demo.MAUI.VENFTApp.Services
{
    public class ApiService
    {
        public ApiService(HttpClient httpClient, NavigationManager navigationManager)
        {
            HttpClient = httpClient;
            NavigationManager = navigationManager;
            HttpClient.BaseAddress = new Uri(NavigationManager.BaseUri);
        }

        public HttpClient HttpClient { get; }
        public NavigationManager NavigationManager { get; }
    }
}
