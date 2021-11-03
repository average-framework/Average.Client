using Average.Client.Framework.Attributes;
using Average.Client.Framework.Interfaces;
using Average.Shared.Enums;
using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Average.Client.Framework.Services
{
    internal abstract class AIBase
    {
        public AIBase()
        {

        }
    }

    internal class AIComportement
    {
        public string Name { get; set; }
        public EntityType EntityType { get; set; }
        public Action<AIComportement> Action { get; set; }
        public Func<bool> Condition { get; set; }

        public AIComportement(string name, EntityType entityType, Action<AIComportement> action, Func<bool> condition)
        {
            Name = name;
            EntityType = entityType;
            Action = action;
            Condition = condition;
        }
    }

    internal class AIRoutine
    {
        public string Name { get; set; }
        public int Interval { get; set; }
        public Func<bool> Condition { get; set; }
        public List<AIComportement> Comportements { get; set; }

        public AIRoutine(string name, int interval, List<AIComportement> comportements, Func<bool> condition)
        {
            Name = name;
            Interval = interval;
            Comportements = comportements;
            Condition = condition;
        }
    }

    internal class AIRoutineService : IService
    {
        private readonly Dictionary<string, AIRoutine> _routines = new();

        public AIRoutineService()
        {

        }

        #region Thread

        [Thread]
        private async Task Update()
        {
            for (int i = 0; i < _routines.Count; i++)
            {
                var routine = _routines.ElementAt(i);

                if (routine.Value.Condition.Invoke())
                {
                    for (int c = 0; c < routine.Value.Comportements.Count; c++)
                    {
                        var comportement = routine.Value.Comportements[c];

                        if (comportement.Condition.Invoke())
                        {
                            comportement.Action.Invoke(comportement);
                        }
                    }
                }

                await BaseScript.Delay(routine.Value.Interval);
            }
        }

        #endregion

        #region Logic

        internal bool RoutineExists(string name)
        {
            return _routines.ContainsKey(name);
        }

        internal void CreateRoutine(AIRoutine routine)
        {
            if (!RoutineExists(routine.Name))
            {
                _routines.Add(routine.Name, routine);
            }
        }

        internal void RemoveRoutine(string name)
        {
            if (RoutineExists(name))
            {
                _routines.Remove(name);
            }
        }

        internal AIRoutine GetRoutineByName(string name)
        {
            if (RoutineExists(name))
            {
                return _routines[name];
            }

            return null;
        }

        internal void AddComportementInRoutine(string routineName, AIComportement comportement)
        {
            if (RoutineExists(routineName))
            {
                _routines[routineName].Comportements.Add(comportement);
            }
        }

        #endregion
    }

    internal class AIComportementService : IService
    {
        private readonly Dictionary<string, AIComportement> _comportements = new();

        public AIComportementService()
        {

        }

        internal bool ComportementExists(string name)
        {
            return _comportements.ContainsKey(name);
        }

        internal void CreateComportement(AIComportement comportement)
        {
            if (!ComportementExists(comportement.Name))
            {
                _comportements.Add(comportement.Name, comportement);
            }
        }

        internal void RemoveComportement(string name)
        {
            if (ComportementExists(name))
            {
                _comportements.Remove(name);
            }
        }

        internal AIComportement GetComportementByName(string name)
        {
            if (ComportementExists(name))
            {
                return _comportements[name];
            }

            return null;
        }

        internal IEnumerable<AIComportement> GetComportementsByEntityType(EntityType entityType)
        {
            return _comportements.Values.ToList().Where(x => x.EntityType == entityType);
        }
    }
}
