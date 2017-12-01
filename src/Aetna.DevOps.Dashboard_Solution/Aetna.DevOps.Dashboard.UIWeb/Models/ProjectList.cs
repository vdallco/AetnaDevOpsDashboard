﻿using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class ProjectList
    {
        public int Count;
        public List<Project> Projects;
        public ProjectList() { Projects = new List<Project>(); Count = 0; }
        public void Add(Project p) { Projects.Add(p); Count++; }

        public ProjectList Clone()
        {
            ProjectList newProjectList = new ProjectList();
            foreach (Deploy project in Projects)
            {
                newProjectList.Add(project.Clone());
            }
            return newProjectList;
        }
    }
}