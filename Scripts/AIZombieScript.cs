using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Enums;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Average.Shared.Enums;
using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Average.Client.Framework.GameAPI;
using static Average.Client.Framework.Services.GameEventService;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Scripts
{
    internal class AIZombieScript : IScript
    {
        internal enum ZombieType : int
        {
            Rusher, Attacker
        }

        private readonly EventService _eventService;
        private readonly GameEventService _gameEventService;
        private readonly AudioService _audioService;
        private readonly ThreadService _threadService;

        private const float StopRange = 1f;
        private const float Speed = 5f;
        private const int FollowDuration = -1;
        private const float AggroDistance = 30f;
        private const int UpdateEntitiesInterval = 1000;

        private const bool EnableZombies = true;

        private int _burnCount;
        private float _range = 150f;
        private float _intensity = 15f;
        private float _dist = 1f;
        private bool _hasShoot;
        private bool _isAnyZombieIsAlert;
        //private Vector3 _shootCoords;

        public class Zone
        {
            public Vector3 Position { get; set; }
            public float Radius { get; set; }
            public int MaxEntityCount { get; set; }

            public Zone(Vector3 position, float radius, int maxEntityCount)
            {
                Position = position;
                Radius = radius;
                MaxEntityCount = maxEntityCount;
            }
        }

        private readonly List<Zone> _zones = new();
        private readonly List<Tuple<int, bool>> _entities = new();
        private readonly List<Tuple<int, ZombieType>> _zombiesTypes = new();

        private readonly Dictionary<uint, List<int>> _modelsNeedToBeReplaced = new()
        {

        };

        private readonly Dictionary<uint, Tuple<Vector3, float, int, float, int>> _weaponsModifiers = new()
        {
            { (uint)GetHashKey(Weapon.WEAPON_REPEATER_WINCHESTER), new Tuple<Vector3, float, int, float, int>(new Vector3(0f, -10f, 0f), 0.5f, 300, 1f, 450) },
            { (uint)GetHashKey(Weapon.WEAPON_SHOTGUN_DOUBLEBARREL), new Tuple<Vector3, float, int, float, int>(new Vector3(0f, -15f, 0f), 1f, 450, 2f, 650) },
        };

        private readonly List<int> _headBones = new()
        {
            52596,
            57278
        };

        private readonly List<int> _legsBones = new()
        {
            33646,
            45454,
            6884,
            65478,
            56200,
            55120,
            43312
        };

        public AIZombieScript(AudioService audioService, GameEventService gameEventService, EventService eventService, ThreadService threadService)
        {
            _audioService = audioService;
            _gameEventService = gameEventService;
            _eventService = eventService;
            _threadService = threadService;

            if (EnableZombies)
            {
                _gameEventService.PedCreated += OnPedCreated;
                _gameEventService.PedDestroyed += OnPedDestroyed;
                _gameEventService.EntityDamaged += EntityDamaged;

                _threadService.StartThread(Update);
                _threadService.StartThread(UpdatePlayer);
                _threadService.StartThread(UpdateEntities);
            }

            _zones.Add(new Zone(new Vector3(-866.5847f, -1354.093f, 43.41318f), 100f, 34));
            _zones.Add(new Zone(new Vector3(-273.7368f, 719.1126f, 114.1314f), 100f, 24));
        }

        [ClientCommand("getpos")]
        private void GetPos()
        {
            var ped = PlayerPedId();
            var coords = GetEntityCoords(ped, true, true);

            Logger.Debug("coords: " + coords);
        }

        [ClientCommand("regionblip")]
        private void RegionBlip(string regionName, string blipStyle)
        {
            // MapEnableRegionBlip
            Call(0x563FCB6620523917, (uint)GetHashKey(regionName), (uint)GetHashKey(blipStyle));
        }

        [ClientCommand("clearregionblip")]
        private void ClearRegionBlip(string regionName)
        {
            // MapEnableRegionBlip
            Call(0x6786D7AFAC3162B3, (uint)GetHashKey(regionName));
        }

        [ClientCommand("sound")]
        private void PlaySound(string soundRef, string soundName, int speechLine)
        {
            _audioService.PlayAmbientSpeechFromEntity(PlayerPedId(), soundRef, soundName, "speech_params_force", speechLine);
        }

        [ClientCommand("giveall")]
        private void GiveAll()
        {
            RemoveAllPedWeapons(PlayerPedId(), true, true);
            GiveDelayedWeaponToPed(PlayerPedId(), (uint)GetHashKey(Weapon.WEAPON_SHOTGUN_DOUBLEBARREL), 999, true, 0);
        }

        [ClientCommand("torch")]
        private void Torch()
        {
            RemoveAllPedWeapons(PlayerPedId(), true, true);
            GiveDelayedWeaponToPed(PlayerPedId(), (uint)GetHashKey(Weapon.WEAPON_MELEE_LANTERN), 999, true, 0);
        }

        [ClientCommand("light")]
        private void Light(int entity, float intensity)
        {
            Call(0x6EC2A67962296F49, PlayerPedId(), 255, 0, 0);
            Call(0xBDBACB52A03CC760, PlayerPedId());
        }

        [ClientCommand("burn")]
        private void Burn(float unk1, int unk2, int type)
        {
            Call(0xC4DC7418A44D6822, PlayerPedId(), unk1, unk2, type);
        }

        [ClientCommand("shake")]
        private async void Shake(string shakeType, float amplitude, int duration)
        {
            // ShakeGameplayCam
            Call(0xD9B31B4650520529, shakeType, amplitude);
            // SetGameplayCamShakeAmplitude
            Call(0x570E35F5C4A44838, amplitude);

            await BaseScript.Delay(duration);

            // StopGameplayCamShaking
            Call(0xE0DE43D290FB65F9, false);
        }

        [ClientCommand("test")]
        private void Test(float range, float intensity, float dist)
        {
            _range = range;
            _intensity = intensity;
            _dist = dist;

            // Eteindre les lumières des villes etc
            // 0xB2797619A7C7747B
        }

        [ClientCommand("blackout")]
        private void Blackout(bool state)
        {
            Call(0xB2797619A7C7747B, state);
        }

        private async Task Update()
        {
            uint currentWeapon = 0;
            var retrieveWeapon = GetCurrentPedWeapon(PlayerPedId(), ref currentWeapon, true, 0, true);

            if (retrieveWeapon)
            {
                if (currentWeapon == (uint)GetHashKey(Weapon.WEAPON_MELEE_LANTERN))
                {
                    var pedCoords = GetEntityCoords(PlayerPedId(), true, true);
                    var pedForward = GetEntityForwardVector((uint)PlayerPedId());
                    var lightPos = pedCoords += (pedForward * _dist);

                    //Call(0xD2D9E04C0DF927F4, pedCoords.X, pedCoords.Y, pedCoords.Z + 1f, 222, 89, 55, range, intensity);
                    Call(0xD2D9E04C0DF927F4, pedCoords.X, pedCoords.Y, pedCoords.Z + 0.5f, 222, 89, 55, 50f, 0.5f);
                }
            }

            Call(0x7AEFB85C1D49DEB6, PlayerPedId(), 1);

            // Supprime l'apparition des charretes générer par le jeu
            Call(0xFEDFA97638D61D4A, 0f);
            Call(0x1F91D44490E1EA0C, 0f);
            Call(0x606374EBFC27B133, 0f);

            Call(0x2F9AC754FE179D58, 0.05f);
            Call(0xBA0980B5C0A11924, 0.05f);
            Call(0xAB0D553FE20A6E25, 0.05f);
            Call(0x28CB6391ACEDD9DB, 0.05f);
            Call(0x7A556143A1C03898, 0.05f);

            //// Multiplie les dégats dans la tête
            //Call(0x2BA918C823B8BA56, PlayerPedId(), 100f);

            SetEntityInvincible(PlayerPedId(), true);
        }

        private async Task UpdatePlayer()
        {
            var ped = PlayerPedId();

            if (IsPedShooting(ped))
            {
                _hasShoot = true;

                uint currentWeapon = 0;
                var retrieveWeapon = GetCurrentPedWeapon(ped, ref currentWeapon, true, 0, true);

                var pedCoords = GetEntityCoords(ped, true, true);

                var ents = _entities.Where(x => Vector3.Distance(pedCoords, GetEntityCoords(x.Item1, true, true)) <= 300f).ToList();

                for (int i = 0; i < ents.Count; i++)
                {
                    var entity = ents[i].Item1;
                    var isEntityDead = IsEntityDead(entity);

                    if (!isEntityDead)
                    {
                        var distance = Vector3.Distance(pedCoords, GetEntityCoords(entity, true, true));

                        if (distance >= 0f && distance <= 4f)
                        {
                            if (!_isAnyZombieIsAlert)
                            {
                                // TaskLookAtEntity
                                Call(0x69F4BE8C8CC4796C, entity, ped, -1, 2048, 3, 0);

                                await BaseScript.Delay(1500);

                                _audioService.PlayAmbientSpeechFromEntity(entity, "VOCALIZATIONS_WAVELOADED_MALE", "SCREAM_TERROR", "speech_params_force", 0);

                                // ClearPedTasks
                                Call(0xE1EF3C1216AFF2CD, entity, true, true);

                                await BaseScript.Delay(2000);

                                // IsPedInCombat
                                if (!Call<bool>(0x4859F1FC66A6278E, entity, ped))
                                {
                                    // TaskCombatPed
                                    Call(0xF166E48407BAC484, entity, ped, 16384, 0);
                                }

                                _isAnyZombieIsAlert = true;
                            }
                        }
                        else
                        {
                            if (!_isAnyZombieIsAlert)
                            {
                                //_shootCoords = pedCoords;

                                Call(0x5BC448CB78FA3E88, entity, pedCoords.X, pedCoords.Y, pedCoords.Z, 2.5f, 0, false, (uint)GetHashKey("injured_left_leg"), -1f);
                            }
                        }
                    }
                }

                if (!retrieveWeapon) return;
                if (!_weaponsModifiers.ContainsKey(currentWeapon)) return;

                if (Call<bool>(0x937C71165CF334B3, currentWeapon))
                {
                    var isCrouched = Call<bool>(0xD5FE956C70FF370B, ped);

                    if (isCrouched)
                    {
                        var modifiers = _weaponsModifiers[currentWeapon];
                        var shakeAmplitude = modifiers.Item2;
                        var shakeDuration = modifiers.Item3;

                        Call(0xD9B31B4650520529, "JOLT_SHAKE", shakeAmplitude);
                        Call(0x570E35F5C4A44838, shakeAmplitude);

                        await BaseScript.Delay(shakeDuration);
                        Call(0xE0DE43D290FB65F9, false);
                    }
                    else
                    {
                        var modifiers = _weaponsModifiers[currentWeapon];
                        var shakeAmplitude = modifiers.Item4;
                        var shakeDuration = modifiers.Item5;

                        Call(0xD9B31B4650520529, "JOLT_SHAKE", shakeAmplitude);
                        Call(0x570E35F5C4A44838, shakeAmplitude);

                        await BaseScript.Delay(shakeDuration);
                        Call(0xE0DE43D290FB65F9, false);
                    }
                }
            }
        }

        private async Task UpdateEntities()
        {
            var player = PlayerId();
            var ped = PlayerPedId();
            var pedCoords = GetEntityCoords(ped, true, true);
            var weaponHash = (uint)GetHashKey(Weapon.WEAPON_MELEE_KNIFE);
            var isFarOfEnnemies = _entities.Count <= 0 || _entities.TrueForAll(x => Vector3.Distance(pedCoords, GetEntityCoords(x.Item1, true, true)) >= 100f);

            var zone = _zones.Find(x => Vector3.Distance(pedCoords, x.Position) <= x.Radius);

            //Logger.Debug($"Vous êtes dans la zone: {(zone != null ? "Oui" : "Non")}");

            for (int i = 0; i < _entities.Count; i++)
            {
                var entity = _entities[i].Item1;

                if (IsEntityDead(entity) || !DoesEntityExist(entity))
                {
                    _entities.RemoveAll(x => x.Item1 == entity);
                    _zombiesTypes.RemoveAll(x => x.Item1 == entity);

                    continue;
                }

                var entityCoords = GetEntityCoords(entity, true, true);
                var distance = GetDistanceBetweenCoords(pedCoords.X, pedCoords.Y, pedCoords.Z, entityCoords.X, entityCoords.Y, entityCoords.Z, true);
                var stealthNoise = GetPlayerCurrentStealthNoise(player);
                var isCrouched = Call<bool>(0xD5FE956C70FF370B, ped);
                var isEntityDead = IsEntityDead(entity);
                var currentPlayerInterior = GetInteriorFromEntity(ped);
                var currentEntityInterior = GetInteriorFromEntity(entity);

                if (!isEntityDead)
                {
                    // HasPedGotWeapon
                    if (!Call<bool>(0x8DECB02F88F428BC, entity, weaponHash, 1, true))
                    {
                        GiveWeaponToPed_2(entity, weaponHash, 30, true, true, 0, false, 0.5f, 1f, 752097756, true, 0, false);
                        SetAmmoInClip(entity, weaponHash, 7);
                    }

                    // SetPedCurrentWeaponVisible
                    Call(0x0725A4CCFDED9A70, entity, false, false, false, false);

                    if (IsPedCanSeeThisPed(ped, entity))
                    {
                        if (distance <= AggroDistance)
                        {
                            // Le pnj peu voir le joueur

                            if (currentEntityInterior == 0 && currentPlayerInterior != 0)
                            {
                                // Si le pnj est à l'extérieur et que je suis dans un batiment
                            }
                            else if (currentEntityInterior != 0 && currentPlayerInterior == 0)
                            {
                                // Si le pnj est dans un batiment et que je suis à l'extérieur
                            }
                            else if (currentEntityInterior == 0 && currentPlayerInterior == 0)
                            {
                                // Le joueur est le pnj ce trouve tout les deux à l'extérieur

                                if (!_isAnyZombieIsAlert)
                                {
                                    // TaskLookAtEntity
                                    Call(0x69F4BE8C8CC4796C, entity, ped, -1, 2048, 3, 0);

                                    await BaseScript.Delay(1500);

                                    _audioService.PlayAmbientSpeechFromEntity(entity, "VOCALIZATIONS_WAVELOADED_MALE", "SCREAM_TERROR", "speech_params_force", 0);

                                    // ClearPedTasks
                                    Call(0xE1EF3C1216AFF2CD, entity, true, true);

                                    await BaseScript.Delay(2000);

                                    _isAnyZombieIsAlert = true;
                                }

                                _isAnyZombieIsAlert = true;
                            }
                            else if (currentEntityInterior == currentPlayerInterior)
                            {
                                // Le joueur est le pnj ce trouve tout les deux dans le même batiment

                                if (!_isAnyZombieIsAlert)
                                {
                                    // TaskLookAtEntity
                                    Call(0x69F4BE8C8CC4796C, entity, ped, -1, 2048, 3, 0);

                                    await BaseScript.Delay(1500);

                                    _audioService.PlayAmbientSpeechFromEntity(entity, "VOCALIZATIONS_WAVELOADED_MALE", "SCREAM_TERROR", "speech_params_force", 0);

                                    // ClearPedTasks
                                    Call(0xE1EF3C1216AFF2CD, entity, true, true);

                                    await BaseScript.Delay(2000);

                                    _isAnyZombieIsAlert = true;
                                }

                                _isAnyZombieIsAlert = true;
                            }
                            else
                            {
                                // Si le pnj est dans un batiment et que je suis dans un autre batiment
                            }
                        }
                    }
                    else
                    {
                        // Le pnj ne peu pas voir le joueur

                        if (isCrouched)
                        {
                            // Le joueur est accroupi

                            if (!_isAnyZombieIsAlert)
                            {
                                if (stealthNoise == 0f)
                                {
                                    // Ne fait pas de bruit
                                }
                                else if (stealthNoise >= 2f && stealthNoise <= 4f)
                                {
                                    // Fait un peu de bruit

                                    if (distance <= 10f)
                                    {
                                        // Le zombie ce retourne si il entend trop de bruit en direction du joueur
                                        // Il doit crier pour prévenir les autres zombie

                                        // 2096 / 43

                                        if (!_isAnyZombieIsAlert)
                                        {
                                            // TaskLookAtEntity
                                            Call(0x69F4BE8C8CC4796C, entity, ped, -1, 2048, 3, 0);

                                            await BaseScript.Delay(1500);

                                            _audioService.PlayAmbientSpeechFromEntity(entity, "VOCALIZATIONS_WAVELOADED_MALE", "SCREAM_TERROR", "speech_params_force", 0);

                                            // ClearPedTasks
                                            Call(0xE1EF3C1216AFF2CD, entity, true, true);

                                            await BaseScript.Delay(2000);

                                            _isAnyZombieIsAlert = true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Le joueur n'est pas accroupi

                            if (distance <= AggroDistance)
                            {
                                if (stealthNoise >= 0f && stealthNoise <= 10f)
                                {
                                    if (!_isAnyZombieIsAlert)
                                    {
                                        // TaskLookAtEntity
                                        Call(0x69F4BE8C8CC4796C, entity, ped, -1, 2048, 3, 0);

                                        await BaseScript.Delay(1500);

                                        _audioService.PlayAmbientSpeechFromEntity(entity, "VOCALIZATIONS_WAVELOADED_MALE", "SCREAM_TERROR", "speech_params_force", 0);

                                        // ClearPedTasks
                                        Call(0xE1EF3C1216AFF2CD, entity, true, true);

                                        await BaseScript.Delay(2000);

                                        _isAnyZombieIsAlert = true;
                                    }
                                }
                            }
                        }
                    }

                    if (_isAnyZombieIsAlert)
                    {
                        if (distance >= 0f && distance <= 6f)
                        {
                            var zombieType = _zombiesTypes.Find(x => x.Item1 == entity);

                            if (zombieType != null)
                            {
                                if (zombieType.Item2 == ZombieType.Rusher)
                                {
                                    uint grappleStyle = (uint)GetHashKey("AR_GRAPPLE_MOUNT_STANDING_FROM_BACK");

                                    Call(0x779A2FFACEFAEA7B, entity, ped, grappleStyle, 1, 2f, 1, 0);
                                }
                                else
                                {
                                    // IsPedInCombat
                                    if (!Call<bool>(0x4859F1FC66A6278E, entity, ped))
                                    {
                                        // SetCurrentPedWeapon
                                        Call(0xADF692B254977C0C, entity, weaponHash, true, 0, false, false);

                                        //// SetPedCombatRange
                                        //Call(0x3C606747B23E497B, entity, 0);

                                        // TaskCombatPed
                                        //Call(0xF166E48407BAC484, entity, ped, 16384, 0);

                                        Call(0x482C99D0B38D1B0A, entity, ped, 0, 0, 1, 2f, 1, (uint)GetHashKey(Weapon.WEAPON_MELEE_KNIFE) /* Float: -1f */);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var zombieType = _zombiesTypes.Find(x => x.Item1 == entity);
                            if (zombieType == null) continue;

                            var speed = zombieType.Item2 == ZombieType.Attacker ? 0.1f : 2f;

                            // TaskFollowToOffsetOfEntity
                            Call(0x304AE42E357B8C7E, entity, ped, 0f, 0f, 0f, speed, FollowDuration, StopRange, true, true, false, false, false, false);
                        }

                        if (i % 30 == 0)
                        {
                            _audioService.PlayAmbientSpeechFromEntity(entity, "VOCALIZATIONS_WAVELOADED_MALE", "SCREAM_TERROR", "speech_params_force", 0);

                            //var soundRef = "MOOSE";
                            //var soundName = "injured_cry";

                            //_audioService.PlayAmbientSpeechFromEntity(entity, soundRef, soundName, "speech_params_force", 0);
                        }
                    }
                }

                if (!_hasShoot)
                {
                    if (!_isAnyZombieIsAlert)
                    {
                        // Doit retrouver sont comportement normal quand il n'y a plus d'aggro

                        // TaskWanderStandard
                        Call(0xBB9CE077274F6A1B, entity, 100f, -1);
                    }
                }
                else
                {
                    if (!_isAnyZombieIsAlert)
                    {
                        //Logger.Debug("Z Distance: " + distance);

                        if (distance <= 2f)
                        {
                            // Doit retrouver sont comportement normal quand il n'y a plus d'aggro

                            // TaskWanderStandard
                            Call(0xBB9CE077274F6A1B, entity, 10f, 10);
                        }
                    }
                }

                if (isFarOfEnnemies)
                {
                    _isAnyZombieIsAlert = false;
                    _hasShoot = false;

                    // Doit retrouver sont comportement normal quand il n'y a plus d'aggro

                    // ClearPedTasks
                    Call(0xE1EF3C1216AFF2CD, entity, true, true);
                }
            }

            //Logger.Debug("Register Entity: " + _entities.Count);

            await BaseScript.Delay(UpdateEntitiesInterval);
        }

        private bool IsPedCanSeeThisPed(int playerPed, int entityPed)
        {
            var playerCoords = GetEntityCoords(playerPed, true, true);
            var targetPedCoords = GetEntityCoords(entityPed, true, true);
            var heading = playerCoords - targetPedCoords;
            var targetForwardVector = GetEntityForwardVector((uint)entityPed);

            return Vector3.Dot(heading, targetForwardVector) > 0;
        }

        [ClientCommand("apply")]
        private void Apply(int entityId)
        {
            var netId = NetworkGetNetworkIdFromEntity(entityId);

            Logger.Debug("Apply info: " + entityId + ", " + netId);

            _eventService.EmitServer("apply", netId);
        }

        [ClientEvent("apply")]
        private async void ApplyEvent(int netId, bool isRusher)
        {
            if (NetworkDoesNetworkIdExist(netId))
            {
                var entity = NetworkGetEntityFromNetworkId(netId);

                if (!NetworkHasControlOfEntity(entity))
                {
                    while (!NetworkHasControlOfEntity(entity))
                    {
                        NetworkRequestControlOfEntity(entity);
                        await BaseScript.Delay(250);
                    }
                }

                Call(0x46DF918788CB093F, entity, "PD_Face_Splatter", 1f, 1f);
                Call(0x46DF918788CB093F, entity, "PD_Human_carcass_Hvy", 1f, 1f);
                Call(0x46DF918788CB093F, entity, "PD_Mud_Blood_Splatter_Body", 1f, 1f);
                Call(0x46DF918788CB093F, entity, "PD_Blood_face_left", 1f, 1f);
                Call(0x46DF918788CB093F, entity, "PD_Blood_face_right", 1f, 1f);
                Call(0x46DF918788CB093F, entity, "PD_Vulture_bloody_head", 1f, 1f);
                Call(0x46DF918788CB093F, entity, "PD_Fall_death", 1f, 1f);
                Call(0x46DF918788CB093F, entity, "PD_Animal_attack_blood_body_upper_left", 1f, 1f);
                Call(0x46DF918788CB093F, entity, "PD_Animal_attack_blood_body_upper_right", 1f, 1f);
                Call(0x46DF918788CB093F, entity, "PD_AnimalBlood_Lrg_Bloody", 1f, 1f);
                Call(0x46DF918788CB093F, entity, "PD_AnimalBlood_Lrg_Bloody", 1f, 1f);
                Call(0x46DF918788CB093F, entity, "PD_Mud_Blood_Splatter_Body", 1f, 1f);
                Call(0x46DF918788CB093F, entity, "PD_Pissing_Pants", 1f, 1f);
                Call(0x46DF918788CB093F, entity, "PD_ANM_piss_pot", 1f, 1f);
                Call(0x46DF918788CB093F, entity, "PD_headshot", 1f, 1f);
                Call(0x46DF918788CB093F, entity, "PD_Savage_Fight_Arrow_Left_Shoulder", 1f, 1f);
                Call(0x46DF918788CB093F, entity, "PD_Savage_Fight_Knife_Chest", 1f, 1f);
                Call(0x46DF918788CB093F, entity, "PD_Savage_Fight_Axe_Back", 1f, 1f);

                //Logger.Debug("Apply IsRusher: " + isRusher);

                var zombieTypeIndex = _zombiesTypes.FindIndex(x => x.Item1 == entity);
                var isZombieRusher = isRusher ? ZombieType.Rusher : ZombieType.Attacker;

                //Logger.Debug("ZombieType IsRusher: " + isZombieRusher);

                if (isRusher)
                {
                    // StartEntityFire
                    Call(0xC4DC7418A44D6822, entity, 0f, -1, 8);

                    if (zombieTypeIndex > -1)
                    {
                        _zombiesTypes[zombieTypeIndex] = new Tuple<int, ZombieType>(entity, isZombieRusher);
                        _zombiesTypes.Add(new Tuple<int, ZombieType>(entity, isZombieRusher));
                    }
                }

                //if (zombieTypeIndex > -1)
                //{
                //    _zombiesTypes[zombieTypeIndex] = new Tuple<int, ZombieType>(entity, isZombieRusher);
                //    _zombiesTypes.Add(new Tuple<int, ZombieType>(entity, isZombieRusher));
                //}
                //else
                //{
                //    _zombiesTypes.Add(new Tuple<int, ZombieType>(entity, isZombieRusher));
                //}
            }
        }

        private async void OnPedCreated(object sender, EventPedCreatedEventArgs e)
        {
            var entity = e.Handle;
            var entityCoords = GetEntityCoords(entity, true, true);

            var ownerId = NetworkGetEntityOwner(entity);
            var myNetId = NetworkPlayerIdToInt();
            //var serverId = GetPlayerServerId(NetworkGetEntityOwner(entity));

            var myServerId = GetPlayerServerId(PlayerId());
            var targetServerId = GetPlayerServerId(NetworkGetEntityOwner(entity));

            var isRusher = false;

            //Logger.Debug("OwnerId: " + ownerId + ", MyNetId: " + myNetId + ", ServerId: " + myServerId + ", TargetServerId: " + targetServerId);

            if (!_entities.Exists(x => x.Item1 == entity))
            {
                var zone = _zones.Find(x => Vector3.Distance(entityCoords, x.Position) <= x.Radius);

                if (zone == null)
                {
                    DeleteEntity(ref entity);
                    return;
                }

                //if (_entities.Count >= zone.MaxEntityCount)
                //{
                //    DeleteEntity(ref entity);
                //    return;
                //}

                var entityType = GetEntityType(entity);

                if (entityType == (int)EntityType.Ped)
                {
                    // IsPedHuman
                    if (Call<bool>(0xB980061DA992779D, entity))
                    {
                        var entityModel = (uint)GetEntityModel(entity);

                        if (_modelsNeedToBeReplaced.ContainsKey(entityModel))
                        {
                            var rndVariation = new Random().Next(0, _modelsNeedToBeReplaced[entityModel].Count - 1);
                            SetPedOutfitPreset(entity, rndVariation);
                        }

                        if (IsPedOnMount(entity))
                        {
                            var mount = GetMount(entity);
                            DeleteEntity(ref mount);
                        }

                        RemoveAllPedWeapons(entity, true, true);

                        // SetBlockingOfNonTemporaryEvents
                        Call(0x9F8AA94D6D97DBF4, entity, true);

                        // SetPedCombatMovement
                        Call(0x4D9CA1009AFBD057, entity, 3);

                        // SetPedSeeingRange
                        Call(0xF29CF591C4BF6CEE, entity, 30f);

                        // Damage packs
                        Call(0x46DF918788CB093F, entity, "PD_Face_Splatter", 1f, 1f);
                        Call(0x46DF918788CB093F, entity, "PD_Human_carcass_Hvy", 1f, 1f);
                        Call(0x46DF918788CB093F, entity, "PD_Mud_Blood_Splatter_Body", 1f, 1f);
                        Call(0x46DF918788CB093F, entity, "PD_Blood_face_left", 1f, 1f);
                        Call(0x46DF918788CB093F, entity, "PD_Blood_face_right", 1f, 1f);
                        Call(0x46DF918788CB093F, entity, "PD_Vulture_bloody_head", 1f, 1f);
                        Call(0x46DF918788CB093F, entity, "PD_Fall_death", 1f, 1f);
                        Call(0x46DF918788CB093F, entity, "PD_Animal_attack_blood_body_upper_left", 1f, 1f);
                        Call(0x46DF918788CB093F, entity, "PD_Animal_attack_blood_body_upper_right", 1f, 1f);
                        Call(0x46DF918788CB093F, entity, "PD_AnimalBlood_Lrg_Bloody", 1f, 1f);
                        Call(0x46DF918788CB093F, entity, "PD_AnimalBlood_Lrg_Bloody", 1f, 1f);
                        Call(0x46DF918788CB093F, entity, "PD_Mud_Blood_Splatter_Body", 1f, 1f);
                        Call(0x46DF918788CB093F, entity, "PD_Pissing_Pants", 1f, 1f);
                        Call(0x46DF918788CB093F, entity, "PD_ANM_piss_pot", 1f, 1f);
                        Call(0x46DF918788CB093F, entity, "PD_headshot", 1f, 1f);
                        Call(0x46DF918788CB093F, entity, "PD_Savage_Fight_Arrow_Left_Shoulder", 1f, 1f);
                        Call(0x46DF918788CB093F, entity, "PD_Savage_Fight_Knife_Chest", 1f, 1f);
                        Call(0x46DF918788CB093F, entity, "PD_Savage_Fight_Axe_Back", 1f, 1f);
                        //Call(0x46DF918788CB093F, entity, "PD_Face_Dirt", 1f, 1f);
                        //Call(0x46DF918788CB093F, entity, "PD_Vomit", 1f, 1f);

                        // ClearPedTasks
                        Call(0xE1EF3C1216AFF2CD, entity, true, true);

                        // Test ???
                        NetworkRegisterEntityAsNetworked(entity);

                        //if (_burnCount % 24 == 0)
                        if (_burnCount % 4 == 0)
                        {
                            isRusher = true;

                            // StartEntityFire
                            Call(0xC4DC7418A44D6822, entity, 0f, -1, 8);

                            _zombiesTypes.Add(new Tuple<int, ZombieType>(entity, ZombieType.Rusher));
                        }
                        else
                        {
                            var rndWalkStyle = new Random(Environment.TickCount * 1).Next(0, 2);

                            // IsPedMale
                            //if (Call<bool>(0x6D9F5FAA7488BA46, entity))
                            //{
                            //    //Call(0x923583741DC87BCE, entity, "war_veteran");
                            //    Call(0x923583741DC87BCE, entity, "lost_man");
                            //}
                            //else
                            //{
                            //    //Call(0x923583741DC87BCE, entity, "default");
                            //    Call(0x923583741DC87BCE, entity, "lost_man");
                            //}

                            Call(0x923583741DC87BCE, entity, "war_veteran");

                            switch (rndWalkStyle)
                            {
                                case 0:
                                    Call(0x89F5E7ADECCCB49C, entity, "injured_left_leg");
                                    break;
                                case 1:
                                    Call(0x89F5E7ADECCCB49C, entity, "injured_right_leg");
                                    break;
                                case 2:
                                    Call(0x89F5E7ADECCCB49C, entity, "injured_torso");
                                    break;
                            }

                            _zombiesTypes.Add(new Tuple<int, ZombieType>(entity, ZombieType.Attacker));
                        }

                        _burnCount++;

                        if (_burnCount > 100)
                        {
                            _burnCount = 0;
                        }

                        _entities.Add(new Tuple<int, bool>(entity, false));
                    }
                }
            }

            if (myServerId == targetServerId)
            {
                Logger.Debug("Send dp for entity: " + entity);

                var netId = NetworkGetNetworkIdFromEntity(entity);
                _eventService.EmitServer("apply", netId, isRusher);
            }

            isRusher = false;
        }

        private void OnPedDestroyed(object sender, EventPedDestroyedEventArgs e)
        {

        }

        private async void EntityDamaged(object sender, EventEntityDamagedEventArgs e)
        {
            if (e.EntityIdOwnerOfDamage != e.DamagedEntityId)
            {
                if (!_weaponsModifiers.ContainsKey(e.WeaponHash)) return;

                if (Call<bool>(0x937C71165CF334B3, e.WeaponHash))
                {
                    var maxHealth = GetEntityMaxHealth(e.DamagedEntityId, 1);
                    var health = GetEntityHealth(e.DamagedEntityId);

                    var weaponModifier = _weaponsModifiers[e.WeaponHash];

                    // ApplyForceToEntity
                    Call(0xF15E8F5D333F09C4, e.DamagedEntityId, 3, weaponModifier.Item1.X, weaponModifier.Item1.Y, weaponModifier.Item1.Z, 0f, 0f, 0f, -1, true, false, true, false, true);

                    var boneId = -1;
                    var haveDamagedBone = GetPedLastDamageBone(e.DamagedEntityId, ref boneId);

                    if (haveDamagedBone)
                    {
                        // Blood
                        Call(0xFFD54D9FE71B966A, e.DamagedEntityId, 2, boneId, 0.0f, 0.1f, 0.0f, 0f, 100.0f, -1.0f, 1.0f);
                    }

                    if (!_headBones.Contains(boneId))
                    {
                        // Annule les dégats subit du ped si le tir n'est pas dans la tête
                        SetEntityHealth(e.DamagedEntityId, maxHealth, 1);
                    }

                    if (_legsBones.Contains(boneId))
                    {
                        await Task.Factory.StartNew(async () =>
                        {
                            var rnd = new Random(Environment.TickCount).Next(2500, 5000);
                            SetPedToRagdoll(e.DamagedEntityId, rnd, rnd, 0, true, true, false);
                        });
                    }
                }
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
