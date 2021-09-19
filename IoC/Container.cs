using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Average.Client.IoC
{
    internal class Container
    {
        private const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        private readonly List<ContainerItem> _registeredTypes = new List<ContainerItem>();

        internal void RegisterInstance(object instance, Reuse reuse = Reuse.Singleton, string serviceKey = "")
        {
            if (string.IsNullOrEmpty(serviceKey))
            {
                if (!_registeredTypes.Exists(x => x.Type == instance.GetType()))
                {
                    _registeredTypes.Add(new ContainerItem(instance.GetType(), reuse, serviceKey, instance));
                }
                else
                {
                    Debug.WriteLine($"Unable to register service: [{instance.GetType()}]. If you have multiple registered types with this type, you need to set an unique serviceKey.");
                }
            }
            else
            {
                if (!_registeredTypes.Exists(x => x.ServiceKey == serviceKey))
                {
                    _registeredTypes.Add(new ContainerItem(instance.GetType(), reuse, serviceKey, instance));
                }
                else
                {
                    Debug.WriteLine($"This service key: [{serviceKey}] is already registered.");
                }
            }
        }

        internal void Register<T>(string serviceKey = "", Reuse reuse = Reuse.Singleton)
        {
            var asm = Assembly.GetExecutingAssembly();

            if (string.IsNullOrEmpty(serviceKey))
            {
                // Can only have one of this type in _registeredTypes

                if (!_registeredTypes.Exists(x => x.Type == typeof(T)))
                {
                    object instance = null;

                    var ctors = typeof(T).GetConstructors(flags);

                    // Get the first service constructor, need to cancel registeration if the type have more than one constructor
                    var ctorParams = ctors[0].GetParameters().ToList();
                    var newArgs = new List<object>();

                    foreach (var param in ctorParams)
                    {
                        var requiredItem = _registeredTypes.Find(x => x.Type == param.ParameterType);

                        if (requiredItem == null)
                        {
                            Debug.WriteLine($"Unable to register service: [{typeof(T)}] with key [{serviceKey}] because the parameter of type {param.ParameterType} is not registered or instancied.");
                            return;
                        }

                        newArgs.Add(requiredItem.Instance);
                    }

                    switch (reuse)
                    {
                        case Reuse.Singleton:
                            if (instance == null)
                            {
                                if (ctorParams.Count() > 0)
                                {
                                    instance = (T)Activator.CreateInstance(typeof(T), newArgs.ToArray());
                                }
                                else
                                {
                                    instance = (T)Activator.CreateInstance(typeof(T));
                                }
                            }
                            break;
                        case Reuse.Transient:
                            if (ctorParams.Count() > 0)
                            {
                                instance = (T)Activator.CreateInstance(typeof(T), newArgs.ToArray());
                            }
                            else
                            {
                                instance = (T)Activator.CreateInstance(typeof(T));
                            }
                            break;
                    }

                    _registeredTypes.Add(new ContainerItem(typeof(T), reuse, serviceKey, instance));
                }
                else
                {
                    Debug.WriteLine($"Unable to register service: [{typeof(T)}]. If you have multiple registered types with this type, you need to set an unique serviceKey.");
                }
            }
            else
            {
                // Can have multiple of this type in _registeredTypes with a unique serviceKey

                if (!_registeredTypes.Exists(x => x.ServiceKey == serviceKey))
                {
                    object instance = null;

                    var ctors = typeof(T).GetConstructors(flags);

                    // Get the first service constructor, need to cancel registeration if the type have more than one constructor
                    var ctorParams = ctors[0].GetParameters().ToList();
                    var newArgs = new List<object>();

                    foreach (var param in ctorParams)
                    {
                        var requiredItem = _registeredTypes.Find(x => x.Type == param.ParameterType);

                        if (requiredItem == null)
                        {
                            Debug.WriteLine($"Unable to register service: [{typeof(T)}] with key [{serviceKey}] because the parameter of type {param.ParameterType} is not registered or instancied.");
                            return;
                        }

                        newArgs.Add(requiredItem.Instance);
                    }

                    switch (reuse)
                    {
                        case Reuse.Singleton:
                            if (instance == null)
                            {
                                if (ctorParams.Count() > 0)
                                {
                                    instance = (T)Activator.CreateInstance(typeof(T), newArgs.ToArray());
                                }
                                else
                                {
                                    instance = (T)Activator.CreateInstance(typeof(T));
                                }
                            }
                            break;
                        case Reuse.Transient:
                            if (ctorParams.Count() > 0)
                            {
                                instance = (T)Activator.CreateInstance(typeof(T), newArgs.ToArray());
                            }
                            else
                            {
                                instance = (T)Activator.CreateInstance(typeof(T));
                            }
                            break;
                    }

                    _registeredTypes.Add(new ContainerItem(typeof(T), reuse, serviceKey, instance));
                }
                else
                {
                    Debug.WriteLine($"This service key: [{serviceKey}] is already registered.");
                }
            }

            Resolve<T>(serviceKey);
        }

        internal T Resolve<T>(string serviceKey = "")
        {
            if (!string.IsNullOrEmpty(serviceKey))
            {
                var item = _registeredTypes.Find(x => x.ServiceKey == serviceKey);

                if (item != null)
                {
                    return (T)item.Instance;
                }
                else
                {
                    Debug.WriteLine($"Unable to resolve service: [{serviceKey}].");
                }

                return default(T);
            }
            else
            {
                var items = _registeredTypes.Where(x => x.Type == typeof(T));

                if (items.Count() > 0)
                {
                    var item = items.First();
                    return (T)item.Instance;
                }
                else
                {
                    Debug.WriteLine($"Unable to resolve service: [{typeof(T)}]");
                }

                return default(T);
            }
        }
    }
}
