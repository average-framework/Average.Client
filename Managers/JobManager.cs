using System.Collections.Generic;
using System.Threading.Tasks;
using SDK.Client;
using SDK.Client.Interfaces;
using SDK.Client.Models;

namespace Average.Client.Managers
{
    public class JobManager : InternalPlugin, IJobManager
    {
        private readonly Dictionary<string, JobRole[]> _registeredJobs = new();

        public delegate void OnRecruitPlayer(string rockstarId, string lastJobName, JobRole lastRole, string newJobName, JobRole newRole);
        public delegate void OnFiredPlayer(string rockstarId, string lastJobName, JobRole lastRole, string newJobName, JobRole newRole);
        public delegate void OnPromotePlayer(string rockstarId, string lastJobName, JobRole lastRole, string newJobName, JobRole newRole);

        public event OnRecruitPlayer OnRecruitPlayerHandler;
        public event OnFiredPlayer OnFiredPlayerHandler;
        public event OnPromotePlayer OnPromotePlayerHandler;

        public virtual void OnRecruitPlayerToJobReached(string rockstarId, string lastJobName, JobRole lastRole, string newJobName, JobRole newRole) => OnRecruitPlayerHandler?.Invoke(rockstarId, lastJobName, lastRole, newJobName, newRole);
        public virtual void OnFiredPlayerToJobReached(string rockstarId, string lastJobName, JobRole lastRole, string newJobName, JobRole newRole) => OnFiredPlayerHandler?.Invoke(rockstarId, lastJobName, lastRole, newJobName, newRole);
        public virtual void OnPromotePlayerToJobReached(string rockstarId, string lastJobName, JobRole lastRole, string newJobName, JobRole newRole) => OnPromotePlayerHandler?.Invoke(rockstarId, lastJobName, lastRole, newJobName, newRole);

        public override void OnInitialized()
        {
            Task.Factory.StartNew(async () =>
            {
                await Character.IsReady();
            });
        }

        public void RegisterJob(string jobName, params JobRole[] roles)
        {
            if (_registeredJobs.ContainsKey(jobName))
            {
                _registeredJobs[jobName] = roles;
            }
            else
            {
                _registeredJobs.Add(jobName, roles);
            }
        }

        public void UnregisterJob(string jobName) => _registeredJobs.Remove(jobName);

        public JobRole[] GetRolesByJob(string jobName)
        {
            if (_registeredJobs.ContainsKey(jobName))
            {
                return _registeredJobs[jobName];
            }
            else
            {
                return null;
            }
        }

        public bool IsJobRegistered(string jobName) => _registeredJobs.ContainsKey(jobName);

        public void RecruitPlayerByRockstarId(string targetRockstarId, string jobName, string roleName, int roleLevel)
        {
            Event.EmitServer("Job.RecruitPlayerByRockstarId", targetRockstarId, jobName, roleName, roleLevel);
        }

        public void FiredPlayerByRockstarId(string targetRockstarId, string jobName, string roleName, int roleLevel)
        {
            Event.EmitServer("Job.FiredPlayerByRockstarId", targetRockstarId, jobName, roleName, roleLevel);
        }

        public void PromotePlayerByRockstarId(string targetRockstarId, string jobName, string roleName, int roleLevel)
        {
            Event.EmitServer("Job.PromotePlayerByRockstarId", targetRockstarId, jobName, roleName, roleLevel);
        }

        public void RecruitPlayerByServerId(int targetServerId, string jobName, string roleName, int roleLevel)
        {
            Event.EmitServer("Job.RecruitPlayerByServerId", targetServerId, jobName, roleName, roleLevel);
        }

        public void FiredPlayerByServerId(int targetServerId, string jobName, string roleName, int roleLevel)
        {
            Event.EmitServer("Job.FiredPlayerByServerId", targetServerId, jobName, roleName, roleLevel);
        }

        public void PromotePlayerByServerId(int targetServerId, string jobName, string roleName, int roleLevel)
        {
            Event.EmitServer("Job.PromotePlayerByServerId", targetServerId, jobName, roleName, roleLevel);
        }

        #region Event

        [ClientEvent("Job.RecruitPlayerToJobByRockstarId")]
        private void RecruitPlayerByRockstarIdEvent(string rockstarId, string jobName, string roleName, int roleLevel)
        {
            var lastJobName = Character.Current.Job.Name;
            var lastJobRole = new JobRole(Character.Current.Job.Role.Name, Character.Current.Job.Role.Level);
            var newJobRole = new JobRole(roleName, roleLevel);

            Character.Current.Job.Name = jobName;
            Character.Current.Job.Role.Name = roleName;
            Character.Current.Job.Role.Level = roleLevel;

            OnRecruitPlayerToJobReached(rockstarId, lastJobName, lastJobRole, jobName, newJobRole);

            var jobLabel = "";
            var roleLabel = "";

            if (Enterprise.EnterpriseExist(jobName))
            {
                var ent = Enterprise.GetEnterpriseModel(jobName);

                jobLabel = ent.JobLabel;

                var role = ent.Roles.Find(x => x.Name == roleName);

                if (role != null)
                {
                    roleLabel = role.Label;
                }
                else
                {
                    roleLabel = roleName;
                }
            }
            else
            {
                jobLabel = jobName;
                roleLabel = roleName;
            }

            Notification.Schedule("TRAVAIL", $"Vous êtes désormais {jobLabel} avec le grade {roleLabel}.", 5000);
        }

        [ClientEvent("Job.FiredPlayerToJobByRockstarId")]
        private void FiredPlayerByRockstarIdEvent(string rockstarId, string jobName, string roleName, int roleLevel)
        {
            var lastJobName = Character.Current.Job.Name;
            var lastJobRole = new JobRole(Character.Current.Job.Role.Name, Character.Current.Job.Role.Level);
            var newJobRole = new JobRole(roleName, roleLevel);

            Character.Current.Job.Name = jobName;
            Character.Current.Job.Role.Name = roleName;
            Character.Current.Job.Role.Level = roleLevel;

            OnFiredPlayerToJobReached(rockstarId, lastJobName, lastJobRole, jobName, newJobRole);

            Notification.Schedule("TRAVAIL", $"Vous avez été licencié(e).", 5000);
        }

        [ClientEvent("Job.PromotePlayerToJobByRockstarId")]
        private void PromotePlayerByRockstarIdEvent(string rockstarId, string jobName, string roleName, int roleLevel)
        {
            var lastJobName = Character.Current.Job.Name;
            var lastJobRole = new JobRole(Character.Current.Job.Role.Name, Character.Current.Job.Role.Level);
            var newJobRole = new JobRole(roleName, roleLevel);

            Character.Current.Job.Name = jobName;
            Character.Current.Job.Role.Name = roleName;
            Character.Current.Job.Role.Level = roleLevel;

            OnPromotePlayerToJobReached(rockstarId, lastJobName, lastJobRole, jobName, newJobRole);

            var roleLabel = "";

            if (Enterprise.EnterpriseExist(jobName))
            {
                var ent = Enterprise.GetEnterpriseModel(jobName);

                var role = ent.Roles.Find(x => x.Name == roleName);

                if (role != null)
                {
                    roleLabel = ent.Roles.Find(x => x.Name == roleName).Label;
                }
                else
                {
                    roleLabel = roleName;
                }
            }
            else
            {
                roleLabel = roleName;
            }
            
            Notification.Schedule("TRAVAIL", $"Vous avez été promu au grade {roleLabel}.", 5000);
        }

        #endregion
    }
}