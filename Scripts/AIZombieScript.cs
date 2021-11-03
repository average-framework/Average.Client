using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Average.Shared.Enums;
using System;
using static Average.Client.Framework.GameAPI;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Scripts
{
    internal class AIZombieScript : AIBase, IScript
    {
        private readonly AIRoutineService _aiRoutineService;
        private readonly AIComportementService _aiComportementService;
        private readonly EventService _eventService;

        //private readonly List<>

        public AIZombieScript(AIRoutineService aiRoutineService, AIComportementService aiComportementService, EventService eventService)
        {
            _aiRoutineService = aiRoutineService;
            _aiComportementService = aiComportementService;
            _eventService = eventService;

            _eventService.PopulationPedCreating += OnPopulationPedCreating;

            Init();
        }

        private void Init()
        {
            _aiComportementService.CreateComportement(new AIComportement("zombie_combat", EntityType.Ped, (comportement) => 
            {
                
            },
            () => 
            { 
                return true;
            }));
        }

        private void OnPopulationPedCreating(object sender, Framework.Events.PopulationPedCreatingEventArgs e)
        {
            var ped = PlayerPedId();
            var coords = GetEntityCoords(ped, true, true);

            // Doit récupèrer l'id de l'entité via ça position

            e.OverrideCalls.setPosition(coords.X, coords.Y, coords.Z);
            //e.OverrideCalls.setModel()
            //SetPedOutfitPreset()
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
