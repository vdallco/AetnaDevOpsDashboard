﻿using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class ProjectGroup : Clonable<ProjectGroup>
    {
        public string GroupName;
        public string GroupId;
        public List<Project> Projects;

        public ProjectGroup (string groupName, string groupId)
        {
            GroupName = groupName;
            GroupId = groupId;
            Projects = new List<Project>();
        }

        public void AddProject(Project project)
        {
            Projects.Add(project);
        }

        public ProjectGroup Clone()
        {
            ProjectGroup newProjectGroup = new ProjectGroup(GroupName, GroupId);
            newProjectGroup.Projects = Projects.Clone();
            return newProjectGroup;
        }

        public bool Equals(ProjectGroup other)
        {
            return (GroupName == other.GroupName && GroupId == other.GroupId && Projects.Equals(other.Projects));
        }
    }
}