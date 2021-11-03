using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.IoC;
using Average.Server.Framework.Attributes;
using Average.Shared.Enums;
using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Average.Client.Framework.Services
{
    internal class ClientJobService : IService
    {
        private readonly Container _container;
        private readonly List<IClientJob> _jobs = new();

        private const int Delay = 1000;

        public List<IClientJob> Jobs => _jobs;

        public ClientJobService(Container container)
        {
            _container = container;

            Logger.Debug("ClientJobService Initialized successfully");
        }

        [Thread]
        private async Task Update()
        {
            for (int i = 0; i < _jobs.Count; i++)
            {
                var job = _jobs[i];

                if (job.State == JobState.Stopped)
                {
                    if (job.StartCondition.Invoke())
                    {
                        OnStartJob(job);
                        OnUpdateJob(job);
                    }
                }

                if (job.State == JobState.Started)
                {
                    if (job.StopCondition.Invoke())
                    {
                        OnStopJob(job);
                        continue;
                    }

                    if (job.LastTriggered + job.Recurring >= DateTime.Now)
                    {
                        continue;
                    }

                    OnUpdateJob(job);
                }
            }

            await BaseScript.Delay(Delay);
        }

        internal void Reflect()
        {
            var asm = Assembly.GetExecutingAssembly();
            var types = asm.GetTypes();

            foreach (var type in types)
            {
                if (_container.IsRegistered(type))
                {
                    // Continue if the service have the same type of this class
                    if (type == GetType()) continue;

                    // Get service instance
                    var service = _container.Resolve(type);
                    if (service == null) continue;

                    if (service is IClientJob @job)
                    {
                        var attr = type.GetCustomAttribute<ClientJobAttribute>();
                        if (attr == null) continue;

                        RegisterInternalJob(@job);
                    }
                }
            }
        }

        private void OnStartJob(IClientJob job)
        {
            try
            {
                job.OnStart();
                job.State = JobState.Started;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to start job {job.GetType().Name}. Error: {ex.Message}\n{ex.StackTrace}.");
            }
        }

        private void OnStopJob(IClientJob job)
        {
            try
            {
                job.OnStop();
                job.State = JobState.Stopped;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to stop job {job.GetType().Name}. Error: {ex.Message}\n{ex.StackTrace}.");
            }
        }

        private void OnUpdateJob(IClientJob job)
        {
            try
            {
                job.OnUpdate();
                job.LastTriggered = DateTime.Now;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to update job {job.GetType().Name}. Error: {ex.Message}\n{ex.StackTrace}.");
            }
        }

        private void RegisterInternalJob(IClientJob job)
        {
            _jobs.Add(job);
            Logger.Debug($"ClientJob Registering [ClientJob] of type: {job.GetType().Name} with id {job.Id}.");
        }
    }
}
