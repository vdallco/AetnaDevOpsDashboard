﻿using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Aetna.DevOps.Dashboard.UIWeb.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Aetna.DevOps.Dashboard.UIWeb.Controllers
{
    [HubName("deployHub")]
    public class DeployHub : Hub
    {
        private static JsonSerializerSettings jsonCamelCaseSettings;
        public static DataState currentState = new DataState();
        private static System.Timers.Timer timer = new System.Timers.Timer(2000); // Set Timer to run every 2 seconds
        public DeployHub() : base()
        {
            jsonCamelCaseSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            timer.Elapsed += (sender, e) =>
            {
                if (OctopusController.UpdateDataState(currentState))
                {
                    Clients.All.onChange(JsonConvert.SerializeObject(currentState, jsonCamelCaseSettings));
                }
            };
            timer.Enabled = true;
            timer.Start();
        }
    }
}