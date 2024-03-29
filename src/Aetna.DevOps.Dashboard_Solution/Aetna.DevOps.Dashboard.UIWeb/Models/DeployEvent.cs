﻿using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class DeployEvent : OctopusModel<DeployEvent>
    {
        public string TimeAndDate;
        public string Message;
        public List<string> RelatedDocs;
        public string Category;
        public List<Environment> Environs;
        public string WebUrl;
        public string DeployId;

        public DeployEvent(string timeAndDate, string msg, List<string> related, string category, string webUrl, string deployId)
        {
            TimeAndDate = timeAndDate;
            Message = msg;
            RelatedDocs = related;
            Category = category;
            Environs = new List<Environment>();
            WebUrl = webUrl;
            DeployId = deployId;
        }
        public override string ToString()
        {
            return Message;
        }

        public bool Equals(DeployEvent other)
        {
            return (TimeAndDate == other.TimeAndDate && Message == other.Message && RelatedDocs.Equals(other.RelatedDocs) && Category == other.Category/* && Environs.DeepEquals<Environment>(other.Environs) */&& WebUrl == other.WebUrl);
        }
    }
}