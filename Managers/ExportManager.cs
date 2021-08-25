using SDK.Client.Diagnostics;
using SDK.Shared;
using SDK.Shared.Exports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Average.Client.Managers
{
    public class ExportManager : IExportManager
    {
        private Dictionary<string, Delegate> _exports = new Dictionary<string, Delegate>();

        public void CallMethod(string exportName, params object[] args)
        {
            if (_exports.ContainsKey(exportName))
                _exports[exportName].DynamicInvoke(args);
            else
                Log.Debug($"Unable to call export: {exportName}, this export does not exists.");
        }

        public T CallMethod<T>(string exportName, params object[] args)
        {
            if (_exports.ContainsKey(exportName))
                return (T)_exports[exportName].DynamicInvoke(args);
            else
                Log.Debug($"Unable to call export: {exportName}, this export does not exists.");

            return (T)Activator.CreateInstance(typeof(T));
        }

        public void RegisterExport(MethodInfo method, ExportAttribute exportAttr, object classObj)
        {
            var methodParams = method.GetParameters();

            if (!_exports.ContainsKey(exportAttr.Name))
            {
                var action = Action.CreateDelegate(Expression.GetDelegateType((from parameter in method.GetParameters() select parameter.ParameterType).Concat(new[] { method.ReturnType }).ToArray()), classObj, method);
                _exports.Add(exportAttr.Name, action);

                Log.Debug($"Registering [Export] attribute: {exportAttr.Name} on method: {method.Name}, args count: {methodParams.Count()}.");
            }
            else
            {
                Log.Error($"Unable to register [Export] attribute: {exportAttr.Name} on method: {method.Name}, an export have already been registered with this name.");
            }
        }

        #region Internal

        internal void InternalCallMethod(string methodName, List<object> args) => CallMethod(methodName, args.ToArray());

        #endregion
    }
}
