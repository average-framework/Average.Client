using SDK.Shared.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Average.Managers
{
    internal class ThreadManager : IThreadManager
    {
        List<Thread> threads = new List<Thread>();

        Action<Func<Task>> attachCallback;
        Action<Func<Task>> detachCallback;

        public ThreadManager(Action<Func<Task>> attachCallback, Action<Func<Task>> detachCallback)
        {
            this.attachCallback = attachCallback;
            this.detachCallback = detachCallback;
        }

        internal void RegisterThread(MethodInfo method, ThreadAttribute threadAttr, object classObj)
        {
            var methodParams = method.GetParameters();

            if (methodParams.Count() == 0)
            {
                if (threadAttr != null)
                {
                    var thread = new Thread(method, threadAttr.StartDelay);
                    Func<Task> func = null;

                    func = async () =>
                    {
                        if (thread.StartDelay > -1)
                        {
                            if (!thread.isStartDelayTriggered)
                            {
                                thread.isStartDelayTriggered = true;
                            }
                        }

                        // Wait task is finished
                        await (Task)method.Invoke(classObj, new object[] { });

                        var currentThreadIndex = threads.FindIndex(x => x.Func == func);

                        if (currentThreadIndex != -1)
                        {
                            var currentThread = threads[currentThreadIndex];

                            if (threadAttr.RepeatCount > 0)
                            {
                                currentThread.RepeatedCount++;

                                if (currentThread.RepeatedCount >= threadAttr.RepeatCount)
                                {
                                    threads[threads.FindIndex(x => x.Func == func)].IsRunning = false;
                                    threads[threads.FindIndex(x => x.Func == func)].IsTerminated = true;
                                    //main.UnregisterTick(func);
                                    detachCallback(func);
                                }
                            }
                        }
                    };

                    thread.Func = func;
                    threads.Add(thread);

                    //main.RegisterTick(func);
                    attachCallback(func);

                    Main.logger.Debug($"Registering [Thread] attribute to method: {method.Name}.");
                }
            }
            else
            {
                Main.logger.Error($"Unable to register [Thread] attribute: {method.Name}, you need to delete parameters: [{string.Join(", ", methodParams.Select(x => x.ParameterType.Name))}]");
            }
        }

        public void StartThread(Func<Task> action) => attachCallback(action);

        public void StopThread(Func<Task> action) => detachCallback(action);

        public void UnregisterThread(string methodName)
        {
            var thread = threads.Find(x => x.Method.Name == methodName);

            if (thread != null)
            {
                //main.UnregisterTick(thread.Func);
                detachCallback(thread.Func);
                threads.Remove(thread);

                Main.logger.Debug($"Unregistering [Thread] attribute to method: {methodName}.");
            }
            else
            {
                Main.logger.Debug($"Unable to unregistering [Thread] attribute from method: {methodName}.");
            }
        }

        public IEnumerable<Thread> GetThreads() => threads.AsEnumerable();
    }
}
