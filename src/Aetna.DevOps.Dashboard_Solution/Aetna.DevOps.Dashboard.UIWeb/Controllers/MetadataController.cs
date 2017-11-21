﻿using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Aetna.DevOps.Dashboard.UIWeb.Models;
using Swashbuckle.Swagger.Annotations;
using System.Text.RegularExpressions;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

namespace Aetna.DevOps.Dashboard.UIWeb.Controllers
{
    #region "SignalR"
    [Microsoft.AspNet.SignalR.Hubs.HubName("deployAction")]
    public class DeployAction : Hub
    {
        private static LiveDeploys currentState = new LiveDeploys();
        private static Random rnd = new Random();
        private static System.Timers.Timer timer = new System.Timers.Timer(400);
        [Microsoft.AspNet.SignalR.Hubs.HubMethodName("onAction")]
        public void onAction()
        {

        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }

    public class LiveDeploys
    {

    }
    #endregion

    #region "JSON API Classes"

    #region "Machine and list"
    public class Machine
    {
        public string id;
        public string name;
        public string url;
        public System.Collections.Generic.List<string> environs;
        public string status;
        public string statusSummary;
        public string isInProcess;
        public Machine(string Id, string Name, string Url, System.Collections.Generic.List<string> Environs, string Status, string StatusSummary, string IsInProcess)
        {
            id = Id;
            name = Name;
            url = Url;
            environs = Environs;
            status = Status;
            statusSummary = StatusSummary;
            isInProcess = IsInProcess;
        }
    }

    public class MachineList
    {
        public System.Collections.Generic.List<Machine> machines;
        public MachineList() { machines = new System.Collections.Generic.List<Machine>(); }
        
        public void add(Machine m) { machines.Add(m); }
    }
    #endregion

    #region "Environment and list"
    public class Environment
    {
        public string id;
        public string name;
        public string description;
        public MachineList machines;

        public Environment(string Id, string Name, string Description, MachineList Machines)
        {
            id = Id;
            name = Name;
            description = Description;
            machines = Machines;
        }

        public override string ToString()
        {
            return name + ":" + machines.machines.Count;
        }
    }

    public class EnvironmentList
    {
        public System.Collections.Generic.List<Environment> environments;
        public EnvironmentList() { environments = new System.Collections.Generic.List<Environment>(); }
        public void add(Environment e) { environments.Add(e); }
    }
    #endregion

    #region "Release and list"
    public class Release
    {
        public string id, version, projectid, channelid, assembled, releasenotes;
        public Release(string Id, string Version, string ProjectId, string ChannelId, string Assembled, string ReleaseNotes)
        {
            id = Id;
            version = Version;
            projectid = ProjectId;
            channelid = ChannelId;
            assembled = Assembled;
            releasenotes = ReleaseNotes;
        }
    }

    public class ReleaseList
    {
        public List<Release> releaseList;
        public ReleaseList() { releaseList = new List<Release>(); }
        public void add(Release r) { releaseList.Add(r); }
    }
    #endregion

    #region "(Active) Deploy and list"
    public class ActiveDeploy
    {
        public string Id, ProjectId, ProjectName, ReleaseId, TaskId, ChannelId, ReleaseVersion, Created, QueueTime, CompletedTime, State,
            HasWarningsOrErrors, ErrorMessage, Duration, IsCurrent, IsCompleted;

        public ActiveDeploy(string id, string projectId, string releaseId, string taskId, string channelId, string releaseVersion,
            string created, string queueTime, string completedTime, string state, string hasWarningsOrErrors, string errorMessage,
            string duration, string isCurrent, string isCompleted, string projectName)
        {
            Id = id; ProjectId = projectId; TaskId = taskId; ReleaseId = releaseId; ChannelId = channelId; ReleaseVersion = releaseVersion;
            Created = created; QueueTime = queueTime; CompletedTime = completedTime; State = state; HasWarningsOrErrors = hasWarningsOrErrors;
            ErrorMessage = errorMessage; Duration = duration; IsCurrent = isCurrent; IsCompleted = isCompleted; ProjectName = projectName;
        }
    }

    public class Deploy
    {
        public string TimeAndDate;
        public string Message;
        public System.Collections.Generic.List<string> RelatedDocs;
        public string Category;
        public System.Collections.Generic.List<Environment> Environs;

        public Deploy(string timeAndDate, string msg, System.Collections.Generic.List<string> related, string category)
        {
            TimeAndDate = timeAndDate;
            Message = msg;
            RelatedDocs = related;
            Category = category;
            Environs = new System.Collections.Generic.List<Environment>();
        }
        public override string ToString()
        {
            return Message;
        }
    }

    public class DeployList
    {
        public System.Collections.Generic.List<Deploy> deploys;
        public DeployList() { deploys = new System.Collections.Generic.List<Deploy>(); }
        public void add(Deploy d) { deploys.Add(d); }
    }
    #endregion

    #region "Project and list"
    public class Project
    {
        public string groupId;
        public string name;
        public string lifecycle;
        public string deploymentProcess;
        public List<string> relatedDocs;
        public string id;

        public Project(string id, string groupId, string name, string lifecycle, string deploymentProcess)
        {
            this.id = id;
            this.groupId = groupId;
            this.name = name;
            this.lifecycle = lifecycle;
            this.deploymentProcess = deploymentProcess;
        }
        public string getGroupId() { return groupId; }
    }

    public class ProjectList
    {
        public int count;
        public List<Project> projects;
        public ProjectList() { projects = new List<Project>(); count = 0; }
        public void Add(Project p) { projects.Add(p); count++; }
    }
    #endregion

    #region "ProjectGroup and dictionary"
    public class ProjectGroup
    {
        public string groupName;
        public string groupId;
        public ProjectList projectList;

        public ProjectGroup (string groupName, string groupId)
        {
            this.groupName = groupName;
            this.groupId = groupId;
            projectList = new ProjectList();
        }

        public void AddProject(Project project)
        {
            projectList.Add(project);
        }
    }

    public class ProjectGroupDictionary
    {
        public Dictionary<string, ProjectGroup> pGroupDictionary;
        public ProjectGroupDictionary()
        {
            pGroupDictionary = new Dictionary<string, ProjectGroup>();
        }
        public void AddProjectGroup (string groupId, ProjectGroup pGroup)
        {
            pGroupDictionary.Add(groupId, pGroup);
        }

        public void addProject (string groupId, Project project)
        {
            pGroupDictionary[groupId].AddProject(project);
        }

        public List<ProjectGroup> getProjectGroups()
        {
            return new List<ProjectGroup>(pGroupDictionary.Values);
        }
    }
    #endregion

    #endregion

    public class MetadataController : ApiController
    {
        #region "Aetna Provided"
        public MetadataController() : this(new UserDetailHelper())
        {
        }

        public MetadataController(UserDetailHelper userDetailHelper)
        {
            if (userDetailHelper == null)
                throw new ArgumentNullException(nameof(userDetailHelper));

            UserHelper = userDetailHelper;
        }
        
        public UserDetailHelper UserHelper { get; private set; }
        #endregion

        #region "API Setup"

        #region "Constants"
        private const string API_URL = "http://ec2-18-220-206-192.us-east-2.compute.amazonaws.com:81/api/";
        private const String API_KEY = "API-A5I5VUHAOV0VJJN6LQ6MXCPSMS";
        private readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        #endregion

        #region "API Datum Enum"
        private enum APIdatum
        {
            projectGroups = 0,
            projects = 1,
            lifecycles = 2,
            environments = 3,
            deploys = 4,
            machines = 5,
            projectProgression = 6,
            dashboard = 7
        }
        #endregion

        #region "HTTP Request Constructor"
        private static string GetResponse(APIdatum apid, string param="")
        {
            WebRequest request;
            string reqString = String.Empty;
            switch (apid)
            {
                case APIdatum.projectGroups:
                    reqString = "projectGroups?take=1000&";
                    break;
                case APIdatum.projects:
                    reqString = "projects?";
                    break;
                case APIdatum.lifecycles:
                    reqString = "lifecycles?";
                    break;
                case APIdatum.environments:
                    reqString = "environments/"+ (param==String.Empty?"":param) +"?";
                    break;
                case APIdatum.deploys:
                    reqString = "events?take=1000&";
                    break;
                case APIdatum.machines:
                    if (param == string.Empty) // all machines
                    {
                        reqString = "machines?take=100000&";
                    }
                    else // asking for info on machines about specific environment
                    {
                        reqString = "environments/" + param + "/machines?";
                    }
                    break;
                case APIdatum.projectProgression:
                    if (param == String.Empty) { return ""; } // can't find progression of project without it's name
                    reqString = "progression/" + param + "?";
                    break;
                case APIdatum.dashboard:
                    reqString = "dashboard?";
                    break;
                default: break;
            }
            request = WebRequest.Create(API_URL + reqString + "apikey=" + API_KEY);
            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string serverResponse = reader.ReadToEnd(); 
            reader.Close();
            response.Close();
            return serverResponse;
        }
        #endregion

        #region "Get Active Projects by Environment"
        private static List<ActiveDeploy> getEnvProjects(string envId)
        {
            List<ActiveDeploy> projList = new List<ActiveDeploy>();
            List<Project> projects = makeProjectList();
            string response = GetResponse(APIdatum.dashboard);
            dynamic jsonDeser = JsonConvert.DeserializeObject(response);
            foreach(dynamic p in jsonDeser.Items)
            {
                if(p.EnvironmentId == envId) {
                    string projName = "";
                    foreach(Project proj in projects) { if (proj.id == p.ProjectId.ToString()) { projName = proj.name; } }
                    projList.Add(new ActiveDeploy(p.Id.ToString(), p.ProjectId.ToString(),
                    p.ReleaseId.ToString(), p.TaskId.ToString(), p.ChannelId.ToString(), p.ReleaseVersion.ToString(),
                    p.Created.ToString(), p.QueueTime.ToString(), p.CompletedTime.ToString(), p.State.ToString(),
                    p.HasWarningsOrErrors.ToString(), p.ErrorMessage.ToString(), p.Duration.ToString(), p.IsCurrent.ToString(),
                    p.IsCompleted.ToString(), projName));
                }
            }
            return projList;
        }
        #endregion

        #region "Get Number of Environments"
        private Dictionary<string, string> getNumberEnviroments(string jsonTxt)
        {
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
            Dictionary<string, string> environments = new Dictionary<string, string>();

            foreach (dynamic o in jsonDeser.Items)
            {
                if (environments.ContainsKey(o.Name.ToString()))
                    environments[o.Name.ToString()]++;
                else
                    environments.Add(o.Name.ToString(), o.Id.ToString());

            }
            return environments;
        }
        #endregion

        #region "Get Number of Machines"
        private Dictionary<string, int> getNumberMachines(string jsonTxt)
        {
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
            Dictionary<string, int> machines = new Dictionary<string, int>();

            foreach (dynamic o in jsonDeser.Items)
            {
                foreach (dynamic machine in o.EnvironmentIds)
                {
                    if (machines.ContainsKey(machine.ToString()))
                        machines[machine.ToString()]++;
                    else
                        machines.Add(machine.ToString(), 1);
                }
            }

            return machines;
        }
        #endregion

        #region "Make Environment List"
        private EnvironmentList makeEnvironmentList()
        {
            EnvironmentList el = new EnvironmentList();
            Dictionary<string, int> numMachines = getNumberMachines(GetResponse(APIdatum.machines));
            Dictionary<string, string> enviromnents = getNumberEnviroments(GetResponse(APIdatum.environments));

            foreach (string key in enviromnents.Keys)
            {
                el.add(new Environment(enviromnents[key], key, numMachines[enviromnents[key]].ToString(), getMachines(enviromnents[key])));
            }

            return el;
        }
        #endregion

        #region "Make Project List"
        private static List<Project> makeProjectList()
        {
            List<Project> pl = new List<Project>();
            string jsonTxt = GetResponse(APIdatum.projects);
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
            foreach (dynamic o in jsonDeser.Items)
            {
                pl.Add(new Project(o.Id.ToString(), o.ProjectGroupId.ToString(), o.Name.ToString(), o.LifecycleId.ToString(), o.DeploymentProcessId.ToString()));
            }
            return pl;
        }
        #endregion

        #region "Sort Project List"
        private List<ProjectGroup> sortProjectGroups()
        {
            List<ProjectGroup> pg;
            ProjectGroupDictionary pgd = new ProjectGroupDictionary();

            string jsonTxt = GetResponse(APIdatum.projectGroups);

            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);

            foreach (dynamic o in jsonDeser.Items)
            {
                pgd.AddProjectGroup(o.Id.ToString(), new ProjectGroup(o.Name.ToString(), o.Id.ToString()));
            }

            List<Project> projects = makeProjectList();

            foreach(Project p in projects)
            {
                pgd.addProject(p.getGroupId(), p);
            }

            pg = pgd.getProjectGroups();
            return pg;
        }
        #endregion

        #region "Get Release List"
        private List<Release> getReleaseList(string response)
        {
            dynamic releases = JsonConvert.DeserializeObject(response);
            ReleaseList rl = new ReleaseList();
            foreach (dynamic r in releases.Releases)
            {
                Release re = new Release(r.Release.Id.ToString(), r.Release.Version.ToString(), r.Release.ProjectId.ToString(),
                    r.Release.ChannelId.ToString(), isoToDateTime(r.Release.Assembled.ToString()), r.Release.ReleaseNotes.ToString());
                rl.add(re);
            }
            return rl.releaseList;
        }
        #endregion

        #region "Get First Int"
        private string getFirstInt(string haystack) // credits to txt2re.com
        {
            string re1 = ".*?"; // Non-greedy match on filler
            string re2 = "(\\d+)";  // Integer Number 1
            Regex r = new Regex(re1 + re2, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = r.Match(haystack);
            if (m.Success)
            {
                String int1 = m.Groups[1].ToString();
                return int1;
            }
            return "API error";
        }
        #endregion

        #region "Datetime functions"

        #region "ISO to Datetime"
        private string isoToDateTime(string iso)
        {
            DateTime dateTime = DateTime.Parse(iso).ToLocalTime();
            return dateTime.ToString();
        }
        #endregion

        private DateTime dateTimeFromEpoch(long time)
        {
            return epoch.AddSeconds(time);
        }

        private long epochFromDateTime(DateTime dt)
        {
            TimeSpan epochSpan = dt.ToUniversalTime() - epoch;
            return (long)Math.Floor(epochSpan.TotalSeconds);
        }
        #endregion

        #region "Format deploys for graphing"
        private DeployList graphDeployments(string jsonTxt)
        {
            DeployList dl = new DeployList();
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
            foreach(dynamic o in jsonDeser.Items)
            {
                string occured = o.Occurred.ToString(); //isoToDateTime(o.Occurred.ToString());
                DateTime parsedDt = Convert.ToDateTime(occured);
                string occuredISO = parsedDt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fZ");
                if (DateTime.Now.AddDays(-1) > parsedDt) { continue; } // ignore events that took place more than 1 day ago
                Deploy d = new Deploy(occuredISO, o.Message.ToString(),
                    JsonConvert.DeserializeObject<System.Collections.Generic.List<string>>(o.RelatedDocumentIds.ToString()), // nested list element
                    o.Category.ToString());
                if (o.Category == "DeploymentStarted") { dl.add(d); }
                if (o.Category == "DeploymentQueued") { dl.add(d); }
                if (o.Category == "DeploymentSucceeded") { dl.add(d); }
                if (o.Category == "DeploymentFailed") { dl.add(d); }
            }
            return dl;
        }
        #endregion

        #region "Get Machines"
        private MachineList getMachines(string envId)
        {
            string machineResponse = GetResponse(APIdatum.machines, envId);
            dynamic mach = JsonConvert.DeserializeObject(machineResponse);
            MachineList m = new MachineList();
            foreach(dynamic mac in mach.Items)
            {
                System.Collections.Generic.List<string> el = new System.Collections.Generic.List<string>();
                foreach(dynamic env in mac.EnvironmentIds)
                {
                    //Environment e = getEnviron(envName);
                    el.Add(env.ToString());
                }
                //Machine m = new Machine()
                Machine machine = new Machine(mac.Id.ToString(), mac.Name.ToString(), mac.Uri.ToString(), el, mac.Status.ToString(), mac.StatusSummary.ToString(), mac.IsInProcess.ToString());
                m.add(machine);
            }
            return m;
        }
        #endregion

        #region "Get Environment"
        private Environment getEnviron(string envName)
        {
            string environData = GetResponse(APIdatum.environments, envName);
            dynamic env = JsonConvert.DeserializeObject(environData);
            Environment e = new Environment(env.Id.ToString(), env.Name.ToString(), env.Description.ToString(), getMachines(envName));
            return e;
        }
        #endregion

        #endregion

        #region "API Calls"

        #region "Project Groups"
        /// <summary>
        /// Pulls information about how many project groups there are
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/projectGroups")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetProjectGroups()
        {
            try
            {
                return Ok(getFirstInt(GetResponse(APIdatum.projectGroups)));
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region "Lifecycles"
        /// <summary>
        /// Pulls information about how many lifecycles there are
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/lifecycles")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetLifecycles()
        {
            try
            {
                return Ok(getFirstInt(GetResponse(APIdatum.lifecycles)));
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region "Projects Info"
        /// <summary>
        /// Pulls information about projects
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/projectsInfo")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetProjectsInfo()
        {
            try
            {
                return Ok<List<Project>>(makeProjectList());
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region "Projects by Environment"
        /// <summary>
        /// Pulls information about projects by environment
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/environmentProjects")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetEnvProjects(string envId)
        {
            try
            {
                return Ok<List<ActiveDeploy>>(getEnvProjects(envId));
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region "Project Count"
        /// <summary>
        /// Pulls information about how many projects there are
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/projects")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetProjects()
        {
            try
            {
                return Ok(getFirstInt(GetResponse(APIdatum.projects)));
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region "Project Releases"
        /// <summary>
        /// Pulls information about how a project has progressed in respect to releases
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/projectProgression")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetProjectProgression(string project)
        {
            try
            {
                return Ok<List<Release>>(getReleaseList(GetResponse(APIdatum.projectProgression, project)));
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region "Machines by Environment"
        /// <summary>
        /// Pulls information about how each machine for a specified environment
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/environmentMachines")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetEnvironmentMachines(string envId)
        {
            try
            {
                return Ok<List<Machine>>(getMachines(envId).machines);
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region "Environments"
        /// <summary>
        /// Pulls information about how many enviornments there are
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/environments")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetEnvironments()
        {
            try
            {
                return Ok(getFirstInt(GetResponse(APIdatum.environments)));
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region "Environment List"
        /// <summary>
        /// Pulls information about how many enviornments there are
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/environmentList")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetEnvironmentList()
        {
            try
            {
                EnvironmentList el = makeEnvironmentList();
                return Ok<List<Environment>>(el.environments);
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region "Project List"
        /// <summary>
        /// Pulls information about how many enviornments there are
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/ProjectList")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetProjectList()
        {
            try
            {
                List<ProjectGroup> pg = sortProjectGroups();
                return Ok<List<ProjectGroup>>(pg);
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region "Deploys"
        /// <summary>
        /// Pulls information about how many deploys there are over the past 24 hours and information about each one
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/deploys")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetDeploys()
        {
            try
            {
                DeployList dl = graphDeployments(GetResponse(APIdatum.deploys));
                for(int x = 0; x < dl.deploys.Count; x++)
                {
                    if (dl.deploys[x].RelatedDocs.Count > 0)
                    {
                        foreach(string docID in dl.deploys[x].RelatedDocs){
                            if (docID.Contains("Environments"))
                            {
                                dl.deploys[x].Environs.Add(getEnviron(docID));
                            }
                        }
                    }
                }
                return Ok<System.Collections.Generic.List<Deploy>>(dl.deploys);
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #endregion

        #region "Aetna Provided"
        /// <summary>
        /// Gets information about the current user.
        /// </summary>
        /// <returns></returns>
        [Route("api/Metadata/UserDetail")]
        [ResponseType(typeof(UserDetail))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetUserDetail()
        {
            try
            {
                UserDetail model = UserHelper.GetAuthHeaderDetails(Request);

                HttpContext.Current.Session["currentUser"] = model;

                return Ok(model);
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }

        /// <summary>
        /// Gets information about the current operating environment.
        /// </summary>
        /// <returns></returns>
        [Route("api/Metadata/Environment")]
        [ResponseType(typeof(OperatingEnvironment))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(OperatingEnvironment))]
        public IHttpActionResult GetEnvironment()
        {
            try
            {
                String showEnvironment = System.Configuration.ConfigurationManager.AppSettings["showEnvironment"];
                String environmentName = System.Configuration.ConfigurationManager.AppSettings["environmentName"];
                String cssClass = System.Configuration.ConfigurationManager.AppSettings["cssClass"];

                DateTime buildDate = File.GetCreationTime(Assembly.GetExecutingAssembly().Location);
                Version version = Assembly.GetExecutingAssembly().GetName().Version;

                OperatingEnvironment model = new OperatingEnvironment()
                {
                    ShowEnvironment = showEnvironment,
                    EnvironmentName = environmentName,
                    CssClass = cssClass,
                    BuildDate = buildDate,
                    Version = "v" + version.ToString(4)
                };

                return Ok(model);
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion
    }
}