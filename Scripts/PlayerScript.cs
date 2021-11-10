using Average.Client.Framework.Attributes;
using Average.Client.Framework.Enums;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Structs;
using CitizenFX.Core;
using System;
using System.Threading.Tasks;
using static Average.Client.Framework.GameAPI;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Scripts
{
    internal class PlayerScript : IScript
    {
        private bool _isFriendlyFireActive;
        private bool _isHemorrhage;
        private bool _isSprinting;
        private bool _canStaminaRegen;
        private int _staminaDecreaseValue;

        private const float walkSpeed = 0.85f;
        private const float runSpeed = 1.7f;
        private const float sprintSpeed = 2.15f;

        public int Stamina { get; set; } = 100;

        public PlayerScript()
        {
            Init();
        }

        #region Thread

        [Thread]
        private async Task KeyboardUpdate()
        {
            var ped = PlayerPedId();

            if (IsControlJustReleased(0, (uint)Keys.N1))
            {
                if (!_isHemorrhage)
                {
                    SetPedToRagdoll(ped, 5000, 5000, 0, true, true, true);

                    await BaseScript.Delay(7000);

                    ClearPedTasks(ped, 0, 0);
                    ClearPedSecondaryTask(ped);
                }
            }

            if (IsControlJustReleased(0, (uint)Keys.LALT))
            {
                _isSprinting = !_isSprinting;
            }

            if (IsControlPressed(0, (uint)Keys.SHIFT))
            {
                _canStaminaRegen = false;

                if (!IsPlayerFreeAiming(PlayerId()))
                {
                    _canStaminaRegen = false;

                    if (_isSprinting)
                    {
                        if (Stamina <= 0)
                        {
                            _staminaDecreaseValue = 2;
                            SetPedMaxMoveBlendRatio(ped, runSpeed);
                        }
                        else
                        {
                            _staminaDecreaseValue = 5;
                            SetPedMaxMoveBlendRatio(ped, sprintSpeed);
                        }
                    }
                    else
                    {
                        _staminaDecreaseValue = 2;
                        SetPedMaxMoveBlendRatio(ped, runSpeed);
                    }
                }
                else
                {
                    _canStaminaRegen = true;
                    SetPedMaxMoveBlendRatio(ped, walkSpeed);
                }
            }
            else
            {
                _canStaminaRegen = true;
                SetPedMaxMoveBlendRatio(ped, walkSpeed);
            }

            SetPedMinMoveBlendRatio(ped, 0f);
            SetPedDesiredMoveBlendRatio(ped, 3f);
            SetPedMoveAnimsBlendOut(ped);
        }

        [Thread]
        private async Task FriendlyFireUpdate()
        {
            await BaseScript.Delay(1);

            var ped = PlayerPedId();
            var playerHash = (uint)GetHashKey("PLAYER");

            if (IsControlPressed(0, 0xCEFD9220))
            {
                _isFriendlyFireActive = true;

                Call(0xBF25EB89375A37AD, 1, playerHash, playerHash);

                await BaseScript.Delay(4000);
            }

            if (!IsPedOnMount(ped) && !IsPedInAnyVehicle(ped, false) && _isFriendlyFireActive)
            {
                _isFriendlyFireActive = false;

                Call(0xBF25EB89375A37AD, 5, playerHash, playerHash);
            }
            else if (_isFriendlyFireActive && (IsPedOnMount(ped) || IsPedInAnyVehicle(ped, false)))
            {
                if (IsPedInAnyVehicle(ped, false))
                {

                }
                else if (GetPedInVehicleSeat(GetMount(ped), -1) == ped)
                {
                    _isFriendlyFireActive = false;

                    Call(0xBF25EB89375A37AD, 5, playerHash, playerHash);
                }
            }

            Call(0xF808475FA571D823, true);
        }

        #endregion

        private void Init()
        {
            var a2 = new PlayerCore(2147483648, 1901291885, 4150375216, 4264807152, 2147483648);
            var a3 = new PlayerCore(2147483648, 1901291885, 4150375216, 4264807152, 2147483648);

            unsafe
            {
                N_0xcb5d11f9508a928d(1, ((IntPtr)(&a2)).ToInt32(), ((IntPtr)(&a3)).ToInt32(), GetHashKey("UPGRADE_STAMINA_TANK_1"), 1084182731, 10, 752097756);
            }
        }

        public void SetStamina(int value)
        {
            var ped = PlayerPedId();

            if (IsPedOnFoot(ped))
            {
                Stamina = value;

                if (Stamina < 0)
                {
                    Stamina = 0;
                }
                else if (Stamina > 100)
                {
                    Stamina = 100;
                }
            }
        }

        public void AddStamina(int value)
        {
            var ped = PlayerPedId();

            if (IsPedOnFoot(ped))
            {
                Stamina += value;

                if (Stamina < 0)
                {
                    Stamina = 0;
                }
                else if (Stamina > 100)
                {
                    Stamina = 100;
                }
            }
        }

        public void RemoveStamina(int value)
        {
            var ped = PlayerPedId();

            if (IsPedOnFoot(ped))
            {
                Stamina -= value;

                if (Stamina < 0)
                {
                    Stamina = 0;
                }
                else if (Stamina > 100)
                {
                    Stamina = 100;
                }
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
