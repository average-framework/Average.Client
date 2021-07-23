using SDK.Shared.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Average.Threading
{
    internal class ThreadManager : IThreadManager
    {
        Main main;

        List<Thread> threads = new List<Thread>();

        public List<Thread> Threads => threads;

        public ThreadManager(Main main)
        {
            this.main = main;
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
                                    main.UnregisterTick(func);
                                }
                            }
                        }
                    };

                    thread.Func = func;
                    threads.Add(thread);

                    main.RegisterTick(func);
                }
            }
            else
            {
                Main.logger.Error($"Unable to register thread: {method.Name}, you need to delete parameters: [{string.Join(", ", method.GetParameters().Select(x => x.ParameterType.Name))}]");
            }
        }

        public void UnregisterThread(string methodName)
        {
            var thread = threads.Find(x => x.Method.Name == methodName);

            if (thread != null)
            {
                main.UnregisterTick(thread.Func);
                threads.Remove(thread);
            }
        }

        public IEnumerable<Thread> GetThreads()
        {
            return threads.AsEnumerable();
        }

        #region Internal

        void IThreadManager.UnregisterThread(string methodName)
        {
            UnregisterThread(methodName);
        }

        IEnumerable<Thread> IThreadManager.GetThreads()
        {
            return GetThreads();
        }

        #endregion
    }
}
