using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework
{
    public static class GameAPI
    {
        public class RaycastHit
        {
            public bool Hit { get; private set; }
            public Vector3 EndCoords { get; private set; }
            public Vector3 SurfaceNormal { get; private set; }
            public int EntityHit { get; private set; }
            public int EntityType { get => GetEntityType(EntityHit); }

            public RaycastHit(bool hit, Vector3 endCoords, Vector3 surfaceNormal, int entityHit)
            {
                Hit = hit;
                EndCoords = endCoords;
                SurfaceNormal = surfaceNormal;
                EntityHit = entityHit;
            }
        }

        public static async void SendNUI(object request)
        {
            await BaseScript.Delay(0);
            SendNuiMessage(JsonConvert.SerializeObject(request));
        }

        public static void Focus(bool cursor = true)
        {
            SetNuiFocus(true, cursor);
        }

        public static void Unfocus()
        {
            SetNuiFocus(false, false);
        }
        public static string FormatString(string str) => str.Replace("'", "").Replace("\"", "");
        public static async Task<bool> LoadModel(uint hash)
        {
            if (IsModelValid(hash))
            {
                Function.Call(Hash.REQUEST_MODEL, hash);
                while (!Function.Call<bool>(Hash.HAS_MODEL_LOADED, hash)) await BaseScript.Delay(0);
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool IsModelValid(uint hash) => Function.Call<bool>(Hash.IS_MODEL_VALID, hash);
        public static bool IsPedComponentEquipped(string category) => Function.Call<bool>((Hash)0xFB4891BD7578CDC1, PlayerPedId(), category);
        public static void ApplyWalkStyle(int ped, string walkStyle) => Function.Call((Hash)0xCB9401F918CB0F75, ped, walkStyle, true, -1);
        public static void RemoveWalkStyle(int ped, string walkStyle) => Function.Call((Hash)0xCB9401F918CB0F75, ped, walkStyle, false, -1);
        //public static void ClearHudPreset()
        //{
        //    foreach (var preset in Constant.HudPresets.Presets)
        //    {
        //        RemoveHudPreset(preset);
        //    }
        //}
        public static Vector3 GetCoordsFromCam(float distance)
        {
            var rot = GetGameplayCamRot(2);
            var coord = GetGameplayCamCoord();

            var tz = rot.Z * 0.0174532924f;
            var tx = rot.X * 0.0174532924f;
            var num = Math.Abs(Math.Cos(tx));

            var newCoordX = coord.X + -Math.Sin(tz) * (num + distance);
            var newCoordY = coord.Y + Math.Cos(tz) * (num + distance);
            var newCoordZ = coord.Z + Math.Sin(tx) * 8.0f;

            return new Vector3((float)newCoordX, (float)newCoordY, (float)newCoordZ);
        }
        public static RaycastHit GetTarget(int ped, float distance)
        {
            var camCoords = GetGameplayCamCoord();
            var farCoords = GetCoordsFromCam(distance);
            var rayHandle = StartShapeTestRay(camCoords.X, camCoords.Y, camCoords.Z, farCoords.X, farCoords.Y, farCoords.Z, -1, ped, 0);

            var hit = false;
            var endCoords = Vector3.Zero;
            var surfaceNormal = Vector3.Zero;
            var entityHit = 0;

            GetShapeTestResult(rayHandle, ref hit, ref endCoords, ref surfaceNormal, ref entityHit);

            return new RaycastHit(hit, endCoords, surfaceNormal, entityHit);
        }
        public static int GetEntityFrontOfPlayer(int ped, float distance)
        {
            var coordA = GetEntityCoords(ped, true, true);
            var coordB = GetOffsetFromEntityInWorldCoords(ped, 0.0f, distance, 0.0f);
            var rayHandle = StartShapeTestRay(coordA.X, coordA.Y, coordA.Z, coordB.X, coordB.Y, coordB.Z, 10, ped, 0);

            var hit = false;
            var endCoords = Vector3.Zero;
            var surfaceNormal = Vector3.Zero;
            var entityHit = 0;

            GetShapeTestResult(rayHandle, ref hit, ref endCoords, ref surfaceNormal, ref entityHit);

            return entityHit;
        }
        public static Vector3 GetCamDirection()
        {
            var heading = GetGameplayCamRelativeHeading() + GetEntityHeading(PlayerPedId());
            var pitch = GetGameplayCamRelativePitch();

            var x = (float)-Math.Sin(heading * Math.PI / 180.0f);
            var y = (float)Math.Cos(heading * Math.PI / 180.0f);
            var z = (float)Math.Sin(pitch * Math.PI / 180.0f);

            var len = (float)Math.Sqrt(x * x + y * y + z * z);

            if (len != 0)
            {
                x /= len;
                y /= len;
                z /= len;
            }

            return new Vector3(x, y, z);
        }
        public static Vector3 RotationToDirection(Vector3 rotation)
        {
            var adjustedRotation = new Vector3();
            adjustedRotation.X = (float)(Math.PI / 180f) * rotation.X;
            adjustedRotation.Y = (float)(Math.PI / 180f) * rotation.Y;
            adjustedRotation.Z = (float)(Math.PI / 180f) * rotation.Z;

            var direction = new Vector3();
            direction.X = (float)(-Math.Sin(adjustedRotation.Z) * Math.Abs(Math.Cos(adjustedRotation.X)));
            direction.Y = (float)(Math.Cos(adjustedRotation.Z) * Math.Abs(Math.Cos(adjustedRotation.X)));
            direction.Z = (float)Math.Sin(adjustedRotation.X);

            return direction;
        }
        public static object[] RaycastGameplayCamera(float distance, int flag)
        {
            var cameraRotation = GetGameplayCamRot(2);
            var cameraCoord = GetGameplayCamCoord();
            var direction = RotationToDirection(cameraRotation);
            var destination = new Vector3();

            destination.X = cameraCoord.X + direction.X * distance;
            destination.Y = cameraCoord.Y + direction.Y * distance;
            destination.Z = cameraCoord.Z + direction.Z * distance;

            var ray = StartShapeTestRay(cameraCoord.X, cameraCoord.Y, cameraCoord.Z, destination.X, destination.Y, destination.Z, flag, -1, 1);
            var hit = false;
            var endCoords = Vector3.Zero;
            var normalCoords = Vector3.Zero;
            var entHit = -1;

            GetShapeTestResult(ray, ref hit, ref endCoords, ref normalCoords, ref entHit);
            return new object[] { hit, endCoords, normalCoords, entHit };
        }
        public static void DrawText(float x, float y, float scale, string text, bool center = false)
        {
            var str = Function.Call<long>(Hash._CREATE_VAR_STRING, 10, "LITERAL_STRING", text);
            SetTextScale(scale, scale);
            SetTextColor(255, 255, 255, 255);
            SetTextCentre(center);
            SetTextDropshadow(15, 0, 0, 0, 255);
            DisplayText(str, x, y);
        }
        public static void DrawBoxOnEntityModel(int entity)
        {
            var model = (uint)GetEntityModel(entity);

            var minimum = Vector3.Zero;
            var maximum = Vector3.Zero;

            GetModelDimensions(model, ref minimum, ref maximum);

            var size = maximum - minimum;

            var objCoords = GetEntityCoords(entity, true, true);
            var objRot = GetEntityRotation(entity, 2);

            int alpha = 150;
            int red = 254;
            int green = 254;
            int blue = 254;

            var s = Math.Sin(objRot.Z / 180 * Math.PI);
            var c = Math.Cos(objRot.Z / 180 * Math.PI);

            // Front Bottom Left
            Function.Call((Hash)(uint)GetHashKey("DRAW_POLY"),
                objCoords.X + -(size.X / 2) * c - (-(size.Y / 2)) * s, objCoords.Y + -(size.X / 2) * s + -(size.Y / 2) * c, objCoords.Z,
                objCoords.X + -(size.X / 2) * c - (-(size.Y / 2)) * s, objCoords.Y + -(size.X / 2) * s + -(size.Y / 2) * c, objCoords.Z + size.Z,
                objCoords.X + size.X / 2 * c - (-(size.Y / 2)) * s, objCoords.Y + size.X / 2 * s + -(size.Y / 2) * c, objCoords.Z, red, green, blue, alpha);

            // Front Top Right
            Function.Call((Hash)(uint)GetHashKey("DRAW_POLY"),
                objCoords.X + -(size.X / 2) * c - (-(size.Y / 2)) * s, objCoords.Y + -(size.X / 2) * s + -(size.Y / 2) * c, objCoords.Z + size.Z,
                objCoords.X + size.X / 2 * c - (-(size.Y / 2)) * s, objCoords.Y + size.X / 2 * s + -(size.Y / 2) * c, objCoords.Z + size.Z,
                objCoords.X + size.X / 2 * c - (-(size.Y / 2)) * s, objCoords.Y + size.X / 2 * s + -(size.Y / 2) * c, objCoords.Z, red, green, blue, alpha);

            // Back Bottom Left
            Function.Call((Hash)(uint)GetHashKey("DRAW_POLY"),
                objCoords.X + -(size.X / 2) * c - size.Y / 2 * s, objCoords.Y + -(size.X / 2) * s + size.Y / 2 * c, objCoords.Z,
                objCoords.X + -(size.X / 2) * c - size.Y / 2 * s, objCoords.Y + -(size.X / 2) * s + size.Y / 2 * c, objCoords.Z + size.Z,
                objCoords.X + size.X / 2 * c - size.Y / 2 * s, objCoords.Y + size.X / 2 * s + size.Y / 2 * c, objCoords.Z, red, green, blue, alpha);

            // Back Top Right
            Function.Call((Hash)(uint)GetHashKey("DRAW_POLY"),
                objCoords.X + -(size.X / 2) * c - size.Y / 2 * s, objCoords.Y + -(size.X / 2) * s + size.Y / 2 * c, objCoords.Z + size.Z,
                objCoords.X + size.X / 2 * c - size.Y / 2 * s, objCoords.Y + size.X / 2 * s + size.Y / 2 * c, objCoords.Z + size.Z,
                objCoords.X + size.X / 2 * c - size.Y / 2 * s, objCoords.Y + size.X / 2 * s + size.Y / 2 * c, objCoords.Z, red, green, blue, alpha);

            // Top Bottom Left
            Function.Call((Hash)(uint)GetHashKey("DRAW_POLY"),
                objCoords.X + -(size.X / 2) * c - (-(size.Y / 2)) * s, objCoords.Y + -(size.X / 2) * s + -(size.Y / 2) * c, objCoords.Z + size.Z,
                objCoords.X + -(size.X / 2) * c - size.Y / 2 * s, objCoords.Y + -(size.X / 2) * s + size.Y / 2 * c, objCoords.Z + size.Z,
                objCoords.X + size.X / 2 * c - (-(size.Y / 2)) * s, objCoords.Y + size.X / 2 * s + -(size.Y / 2) * c, objCoords.Z + size.Z, red, green, blue, alpha);

            // Top Top Right
            Function.Call((Hash)(uint)GetHashKey("DRAW_POLY"),
                objCoords.X + -(size.X / 2) * c - size.Y / 2 * s, objCoords.Y + -(size.X / 2) * s + size.Y / 2 * c, objCoords.Z + size.Z,
                objCoords.X + size.X / 2 * c - size.Y / 2 * s, objCoords.Y + size.X / 2 * s + size.Y / 2 * c, objCoords.Z + size.Z,
                objCoords.X + size.X / 2 * c - (-(size.Y / 2)) * s, objCoords.Y + size.X / 2 * s + -(size.Y / 2) * c, objCoords.Z + size.Z, red, green, blue, alpha);

            // Bottom Bottom Left
            Function.Call((Hash)(uint)GetHashKey("DRAW_POLY"),
                objCoords.X + -(size.X / 2) * c - (-(size.Y / 2)) * s, objCoords.Y + -(size.X / 2) * s + -(size.Y / 2) * c, objCoords.Z,
                objCoords.X + -(size.X / 2) * c - size.Y / 2 * s, objCoords.Y + -(size.X / 2) * s + size.Y / 2 * c, objCoords.Z,
                objCoords.X + size.X / 2 * c - (-(size.Y / 2)) * s, objCoords.Y + size.X / 2 * s + -(size.Y / 2) * c, objCoords.Z, red, green, blue, alpha);

            // Bottom Top Right
            Function.Call((Hash)(uint)GetHashKey("DRAW_POLY"),
                objCoords.X + -(size.X / 2) * c - size.Y / 2 * s, objCoords.Y + -(size.X / 2) * s + size.Y / 2 * c, objCoords.Z,
                objCoords.X + size.X / 2 * c - size.Y / 2 * s, objCoords.Y + size.X / 2 * s + size.Y / 2 * c, objCoords.Z,
                objCoords.X + size.X / 2 * c - (-(size.Y / 2)) * s, objCoords.Y + size.X / 2 * s + -(size.Y / 2) * c, objCoords.Z, red, green, blue, alpha);

            // Left Bottom Left
            Function.Call((Hash)(uint)GetHashKey("DRAW_POLY"),
                objCoords.X + -(size.X / 2) * c - size.Y / 2 * s, objCoords.Y + -(size.X / 2) * s + size.Y / 2 * c, objCoords.Z,
                objCoords.X + -(size.X / 2) * c - size.Y / 2 * s, objCoords.Y + -(size.X / 2) * s + size.Y / 2 * c, objCoords.Z + size.Z,
                objCoords.X + -(size.X / 2) * c - (-(size.Y / 2)) * s, objCoords.Y + -(size.X / 2) * s + -(size.Y / 2) * c, objCoords.Z, red, green, blue, alpha);

            // Left Top Right
            Function.Call((Hash)(uint)GetHashKey("DRAW_POLY"),
                objCoords.X + -(size.X / 2) * c - size.Y / 2 * s, objCoords.Y + -(size.X / 2) * s + size.Y / 2 * c, objCoords.Z + size.Z,
                objCoords.X + -(size.X / 2) * c - (-(size.Y / 2)) * s, objCoords.Y + -(size.X / 2) * s + -(size.Y / 2) * c, objCoords.Z + size.Z,
                objCoords.X + -(size.X / 2) * c - (-(size.Y / 2)) * s, objCoords.Y + -(size.X / 2) * s + -(size.Y / 2) * c, objCoords.Z, red, green, blue, alpha);

            // Right Bottom Left
            Function.Call((Hash)(uint)GetHashKey("DRAW_POLY"),
                objCoords.X + size.X / 2 * c - size.Y / 2 * s, objCoords.Y + size.X / 2 * s + size.Y / 2 * c, objCoords.Z,
                objCoords.X + size.X / 2 * c - size.Y / 2 * s, objCoords.Y + size.X / 2 * s + size.Y / 2 * c, objCoords.Z + size.Z,
                objCoords.X + size.X / 2 * c - (-(size.Y / 2)) * s, objCoords.Y + size.X / 2 * s + -(size.Y / 2) * c, objCoords.Z, red, green, blue, alpha);

            // Right Top Right
            Function.Call((Hash)(uint)GetHashKey("DRAW_POLY"),
                objCoords.X + size.X / 2 * c - size.Y / 2 * s, objCoords.Y + size.X / 2 * s + size.Y / 2 * c, objCoords.Z + size.Z,
                objCoords.X + size.X / 2 * c - (-(size.Y / 2)) * s, objCoords.Y + size.X / 2 * s + -(size.Y / 2) * c, objCoords.Z + size.Z,
                objCoords.X + size.X / 2 * c - (-(size.Y / 2)) * s, objCoords.Y + size.X / 2 * s + -(size.Y / 2) * c, objCoords.Z, red, green, blue, alpha);
        }
        public static string RandomString()
        {
            var g = Guid.NewGuid();
            var guid = Convert.ToBase64String(g.ToByteArray());
            guid = guid.Replace("=", "");
            guid = guid.Replace("+", "");
            guid = guid.Replace("/", "");
            return guid;
        }
        public static string ConvertDecimalToString(decimal value) => value.ToString("C2").Remove(0, 1);
        public static List<int> GetAllPeds()
        {
            var results = new List<int>();
            var outEntity1 = -1;
            var firstPed = FindFirstPed(ref outEntity1);

            if (outEntity1 > 0)
            {
                results.Add(outEntity1);
            }
            for (var outEntity2 = -1; FindNextPed(firstPed, ref outEntity2); outEntity2 = -1)
            {
                if (outEntity2 > 0)
                {
                    results.Add(outEntity2);
                }
            }
            EndFindPed(firstPed);
            return results;
        }
        public static List<int> GetAllVehicles()
        {
            var results = new List<int>();
            var outEntity1 = -1;
            var firstPed = FindFirstVehicle(ref outEntity1);

            if (outEntity1 > 0)
            {
                results.Add(outEntity1);
            }
            for (var outEntity2 = -1; FindNextVehicle(firstPed, ref outEntity2); outEntity2 = -1)
            {
                if (outEntity2 > 0)
                {
                    results.Add(outEntity2);
                }
            }
            EndFindVehicle(firstPed);
            return results;
        }
        public static List<int> GetAllProps()
        {
            var results = new List<int>();
            var outEntity1 = -1;
            var firstPed = FindFirstObject(ref outEntity1);

            if (outEntity1 > 0)
            {
                results.Add(outEntity1);
            }
            for (var outEntity2 = -1; FindNextObject(firstPed, ref outEntity2); outEntity2 = -1)
            {
                if (outEntity2 > 0)
                {
                    results.Add(outEntity2);
                }
            }
            EndFindObject(firstPed);
            return results;
        }
        public static List<int> GetAllPickups()
        {
            var results = new List<int>();
            var outEntity1 = -1;
            var firstPed = FindFirstPickup(ref outEntity1);

            if (outEntity1 > 0)
            {
                results.Add(outEntity1);
            }
            for (var outEntity2 = -1; FindNextPickup(firstPed, ref outEntity2); outEntity2 = -1)
            {
                if (outEntity2 > 0)
                {
                    results.Add(outEntity2);
                }
            }
            EndFindPickup(firstPed);
            return results;
        }
        public static int CreateBlip(int sprite, string text, float scale, Vector3 position)
        {
            var blip = Function.Call<int>((Hash)0x554D9D53F696D002, 1664425300, position.X, position.Y, position.Z);
            SetBlipSprite(blip, sprite, 1);
            SetBlipScale(blip, scale);
            SetBlipNameFromPlayerString(blip, text);

            return blip;
        }
        public static void SetBlipNameFromPlayerString(int blip, string playerString) => Function.Call((Hash)0x9CB1A1623062F402, blip, playerString);
        public static void RequestModel(uint hash) => Function.Call(Hash.REQUEST_MODEL, hash);
        public static void SetRandomOutfitVariation(int handle, bool randomVariation) =>
            Function.Call((Hash)0x283978a15512b2fe, handle, randomVariation);
        public static void SetPedComponentEnabled(int ped, uint component, bool immediately, bool isMp, bool p4) => Function.Call((Hash)0xD3A7B003ED343FD9, ped, component, immediately, isMp, p4);
        public static void SetPedComponentDisabled(int ped, uint component, int p2, bool p3) => Function.Call((Hash)0xDF631E4BCE1B1FC4, ped, component, p2, p3);
        public static void DisplayHudComponent(int hash) =>
            Function.Call((Hash)0x8BC7C1F929D07BF3, hash);
        public static void HideHudComponent(int hash) =>
            Function.Call((Hash)0x4CC5F2FC1332577F, hash);
        public static void SetPedOutfitPreset(int ped, int presetId) => Function.Call((Hash)0x77FF8D35EEC6BBC4, ped, presetId, 0);
        public static bool OutfitFullyLoaded(int ped) => Function.Call<bool>((Hash)0xA0BC8FAED8CFEB3C, ped);
        public static int GetPedNumOutfitPresets(int ped) => Function.Call<int>((Hash)0x10C70A515BC03707, ped);
        public static int FromHexToHash(string hex) => Convert.ToInt32("0x" + hex, 16);
        public static long FromHexStringToUInt(string hex)
        {
            var obj = uint.Parse(hex, NumberStyles.AllowHexSpecifier);
            return Convert.ToInt64(obj);
        }
        public static bool IsMetapedUsingComponent(int ped, uint category) => Function.Call<bool>((Hash)0xFB4891BD7578CDC1, ped, category);
        public static uint GetPedComponentCategory(uint componentHash, int metapedType, bool isMp) => Function.Call<uint>((Hash)0x5FF9A878C3D115B8, componentHash, metapedType, isMp);
        public static void SetPedScale(int ped, float scale) => Function.Call((Hash)0x25ACFC650B65C538, ped, scale);
        public static void SetPedScale(float scale) => Function.Call((Hash)0x25ACFC650B65C538, scale);
        public static void UpdatePlayerPed() => Function.Call((Hash)0x704C908E9C405136, PlayerPedId());
        public static void SetPedBodyComponent(uint component)
        {
            Function.Call((Hash)0x1902C4CFCC5BE57C, PlayerPedId(), component);
            UpdatePedVariation();
        }
        public static void SetPedBodyComponent(List<int> components, int index)
        {
            Function.Call((Hash)0x1902C4CFCC5BE57C, PlayerPedId(), components[index]);
            UpdatePedVariation();
        }
        public static void UpdatePedVariation()
        {
            Function.Call((Hash)0x704C908E9C405136, PlayerPedId());
            Function.Call((Hash)0xAAB86462966168CE, PlayerPedId(), 1);
            Function.Call((Hash)0xCC8CA3E88256E58F, PlayerPedId(), 0, 1, 1, 1, 0);
        }
        public static void UpdatePedVariation(int ped)
        {
            Function.Call((Hash)0x704C908E9C405136, ped);
            Function.Call((Hash)0xAAB86462966168CE, ped, 1);
            Function.Call((Hash)0xCC8CA3E88256E58F, ped, 0, 1, 1, 1, 0);
        }
        public static void SetPedFaceFeature(int index, float value)
        {
            Function.Call((Hash)0x704C908E9C405136, PlayerPedId());
            Function.Call((Hash)0x5653AB26C82938CF, PlayerPedId(), index, value);
        }
        public static void SetPedFaceFeature(int ped, int index, float value)
        {
            Function.Call((Hash)0x704C908E9C405136, ped);
            Function.Call((Hash)0x5653AB26C82938CF, ped, index, value);
        }
        public static bool HasPedComponentLoaded() => Function.Call<bool>((Hash)0xA0BC8FAED8CFEB3C, PlayerPedId());
        public static void RemovePedComponent(uint category)
        {
            Function.Call((Hash)0xD710A5007C2AC539, PlayerPedId(), category, 0);
            UpdatePedVariation();
        }
        public static void RemovePedComponent(string category)
        {
            var obj = uint.Parse(category, NumberStyles.AllowHexSpecifier);
            var hash = Convert.ToInt64(obj);

            Function.Call((Hash)0xD710A5007C2AC539, PlayerPedId(), hash, 0);
            UpdatePedVariation();
        }
        public static void RemovePedComponent(int ped, uint category)
        {
            Function.Call((Hash)0xD710A5007C2AC539, ped, category, 0);
            UpdatePedVariation();
        }
        public static void RemovePedComponent(int ped, string category)
        {
            var obj = uint.Parse(category, NumberStyles.AllowHexSpecifier);
            var hash = Convert.ToInt64(obj);

            Function.Call((Hash)0xD710A5007C2AC539, ped, hash, 0);
            UpdatePedVariation();
        }
        public static void PlayScenarioAtPosition(int ped, string scenario, Vector3 position, float heading) =>
            Function.Call(Hash.TASK_START_SCENARIO_AT_POSITION, ped, GetHashKey(scenario), position.X, position.Y, position.Z, heading, 0, 0, 0, 0, -1082130432, 0);
        public static void PlayScenarioInPlace(int ped, string scenario, bool playEnterAnim = true, int p4 = -1082130432) =>
            Function.Call(Hash._TASK_START_SCENARIO_IN_PLACE, ped, GetHashKey(scenario), -1, playEnterAnim, false, p4, false);
        public static void SetPedRelationshipGroupDefaultHash(int ped, uint hash) => Function.Call((Hash)0xADB3F206518799E8, ped, hash);
        public static void SetAnimalMood(int ped, int mood) =>
            Function.Call((Hash)0xCC97B29285B1DC3B, ped, mood);
        public static void SetPedConfigFlag(int ped, int flagId, bool value) =>
            Function.Call((Hash)0x1913FE4CBF41C463, ped, flagId, value);
        public static void SetAnimalTuningBoolParam(int ped, int p1, bool p2) =>
            Function.Call((Hash)0x9FF1E042FA597187, ped, p1, p2);
        public static void TaskAnimalInteraction(int ped, int targetPed, int p2, int p3, int p4) =>
            Function.Call((Hash)0xCD181A959CFDD7F4, ped, targetPed, p2, p3, p4);
        public static bool GetGroundZFor3DCoord(float x, float y, float z, ref float groundZ, bool p4) =>
            Function.Call<bool>((Hash)0x24FA4267BB8D2431, x, y, z, groundZ, p4);
        public static bool TaskEmote(int ped, int category, int p2, uint emoteType, bool p4, bool p5, bool p6, bool p7, bool p8) =>
            Function.Call<bool>((Hash)0xB31A277C1AC7B7FF, ped, category, p2, emoteType, p4, p5, p6, p7, p8);
        public static bool TaskEmote(int ped, int category, int p2, string emoteType, bool p4, bool p5, bool p6, bool p7, bool p8) =>
            Function.Call<bool>((Hash)0xB31A277C1AC7B7FF, ped, category, p2, (uint)GetHashKey(emoteType), p4, p5, p6, p7, p8);
        public static bool TaskFullBodyEmote(int ped, uint emoteType) =>
            Function.Call<bool>((Hash)0xB31A277C1AC7B7FF, ped, 1, 2, emoteType, false, false, false, false, false);
        public static bool TaskUpperBodyEmote(int ped, uint emoteType) =>
            Function.Call<bool>((Hash)0xB31A277C1AC7B7FF, ped, 0, 0, emoteType, true, true, false, false, false);
        public static bool TaskEmote(int ped, uint emoteType, bool p3, bool p4, bool p5, bool p6, bool p7) =>
            Function.Call<bool>((Hash)0xB31A277C1AC7B7FF, ped, 0, 0, emoteType, p3, p4, p5, p6, p7);
        public static async void PlayClipset(string dict, string anim, int flag, int duration)
        {
            RequestAnimDict(dict);

            while (!HasAnimDictLoaded(dict))
            {
                await BaseScript.Delay(100);
            }

            if (IsEntityPlayingAnim(PlayerPedId(), dict, anim, 3))
            {
                ClearPedSecondaryTask(PlayerPedId());
            }
            else
            {
                Function.Call(Hash.TASK_PLAY_ANIM, PlayerPedId(), dict, anim, 1.0f, 8.0f, duration, flag, 0, true, 0, false, 0, false);
            }
        }
        public static async Task PlayClipset2(string dict, string anim, int flag, int duration, float blendIn, float blendOut)
        {
            RequestAnimDict(dict);

            while (!HasAnimDictLoaded(dict))
            {
                await BaseScript.Delay(100);
            }

            if (IsEntityPlayingAnim(PlayerPedId(), dict, anim, 3))
            {
                ClearPedSecondaryTask(PlayerPedId());
            }
            else
            {
                Function.Call(Hash.TASK_PLAY_ANIM, PlayerPedId(), dict, anim, blendIn, blendOut, duration, flag, 0.0f, true, 0, false, 0, false);
            }
        }
        public static void RemovePedWoundEffect(int ped, float p2) => Function.Call((Hash)0x66B1CB778D911F49, ped, p2);
        public static void SetStance(int ped, string stance) => Function.Call((Hash)0x923583741DC87BCE, ped, stance);
        public static void SetWalking(int ped, string walking) => Function.Call((Hash)0x89F5E7ADECCCB49C, ped, walking);
        public static int CreatePickup(uint model, Vector3 position) => Function.Call<int>((Hash)0xFBA08C503DD5FA58, model, position.X, position.Y, position.Z, false, 0, 0, 0, 0, 0, 0, 0);
        public static void BlockPickupPlacementLight(int pickup, int block) => Function.Call((Hash)0x0552AA3FFC5B87AA, pickup, block);
        public static void DropCurrentPedWeapon() => Function.Call((Hash)0xC6A6789BB405D11C, PlayerPedId(), 1);
        public static void DoorSystemSetDoorState(uint hash, bool isLocked) => Function.Call((Hash)0x6BAB9442830C7F53, (dynamic)hash, isLocked ? 1 : 0);
        public static void DisableFirstPersonCamThisFrame() => Function.Call((Hash)0x9C473089A934C930);
        public static void NetworkSetFriendlyFireOption(bool toggle) => Function.Call((Hash)0xF808475FA571D823, toggle);
        public static void SetRelationShipBetweenGroups(int relationship, uint group1, uint group2) => Function.Call((Hash)0xBF25EB89375A37AD, relationship, group1, group2);
        // public static void SetPedComponentEnabled(List<string> components, int index) => Function.Call((Hash)0xD3A7B003ED343FD9, PlayerPedId(), uint.Parse(components[index], System.Globalization.NumberStyles.AllowHexSpecifier), true, true, false);
        // public static void SetPedComponentEnabled(List<uint> components, int index) => Function.Call((Hash)0xD3A7B003ED343FD9, PlayerPedId(), components[index], true, true, false);
        // public static void SetPedComponentEnabled(List<int> components, int index) => Function.Call((Hash)0xD3A7B003ED343FD9, PlayerPedId(), components[index], true, true, false);
        // public static void SetPedComponentEnabled(uint components) => Function.Call((Hash)0xD3A7B003ED343FD9, PlayerPedId(), components, true, true, false);
        // public static void SetPedComponentEnabled(uint components, bool p2) => Function.Call((Hash)0xD3A7B003ED343FD9, PlayerPedId(), components, true, true, p2);
        // public static void SetPedComponentEnabled(int components) => Function.Call((Hash)0xD3A7B003ED343FD9, PlayerPedId(), components, true, true, false);
        // public static void SetPedComponentEnabled(int components, bool p2) => Function.Call((Hash)0xD3A7B003ED343FD9, PlayerPedId(), components, true, true, p2);
        // public static void SetPedComponentEnabledTest(int ped, uint components, bool p2, bool p3) => Function.Call((Hash)0xD3A7B003ED343FD9, PlayerPedId(), components, true, true, p2);
        public static void ResetPedTexture(int textureId) => Function.Call((Hash)0x8472A1789478F82F, textureId);
        public static void ResetPedTexture2(int textureId) => Function.Call((Hash)0xB63B9178D0F58D82, textureId);
        public static void DeletePedTexture(int textureId) => Function.Call((Hash)0x6BEFAA907B076859, textureId);
        public static int CreatePedTexture(uint albedoHash, uint normalHash, uint unkHash) => Function.Call<int>((Hash)0xC5E7204F322E49EB, albedoHash, normalHash, unkHash);
        public static int AddPedOverlay(int textureId, uint albedoHash, uint normalHash, uint unkHash, int colorType, float opacity, int p6) => Function.Call<int>((Hash)0x86BB5FF45F193A02, textureId, albedoHash, normalHash, unkHash, colorType, opacity, p6);
        public static void SetPedOverlayPalette(int textureId, int overlayId, uint paletteHash) => Function.Call((Hash)0x1ED8588524AC9BE1, textureId, overlayId, paletteHash);
        public static void SetPedOverlayPaletteColour(int textureId, int overlayId, int primaryColor, int secondaryColor, int tertiaryColor) => Function.Call((Hash)0x2DF59FFE6FFD6044, textureId, overlayId, primaryColor, secondaryColor, tertiaryColor);
        public static void SetPedOverlayVariation(int textureId, int overlayId, int variation) => Function.Call((Hash)0x3329AAE2882FC8E4, textureId, overlayId, variation);
        public static void SetPedOverlayOpacity(int textureId, int overlayId, float opacity) => Function.Call((Hash)0x6C76BC24F8BB709A, textureId, overlayId, opacity);
        public static bool IsPedTextureValid(int textureId) => Function.Call<bool>((Hash)0x31DC8D3F216D8509, textureId);
        public static void OverrideTextureOnPed(int ped, uint componentHash, int textureId) => Function.Call((Hash)0x0B46E25761519058, ped, componentHash, textureId);
        public static void UpdatePedTexture(int textureId) => Function.Call((Hash)0x92DAABA2C1C10B0E, textureId);
        public static async void SetPlayerModel(uint model)
        {
            Function.Call((Hash)0xED40380076A31506, PlayerId(), model, true);
        }
        public static void SetWeatherType(uint weatherType, bool p1, bool p2, bool p3, float p4, bool p5) => Function.Call((Hash)0x59174F1AFE095B5A, weatherType, p1, p2, p3, p4, p5);
        public static void SetWeatherTypeFrozen(bool toggle) => Function.Call((Hash)0xD74ACDF7DB8114AF, toggle);
        //public static int GetVariationCount(string hash) => Constant.Peds.Find(x => x.HashString == hash).Variation;
        public static void SetVisualSettings(string name, float value)
        {
            Function.Call(Hash.SET_VISUAL_SETTING_FLOAT, name, value);
        }
        public static void LoadInterior(int interior, string entitySetName)
        {
            if (!IsInteriorEntitySetActive(interior, entitySetName))
            {
                Function.Call((Hash)0x174D0AAB11CED739, interior, entitySetName);
            }
        }
        public static void UnloadInterior(int interior, string entitySetName)
        {
            if (IsInteriorEntitySetActive(interior, entitySetName))
            {
                Function.Call((Hash)0x33B81A2C07A51FFF, interior, entitySetName, true);
            }
        }
        public static void SetAllowDualWield(int ped, bool allow) => Function.Call((Hash)0x83B8D50EB9446BBA, ped, allow);
        public static uint GiveWeaponToPed(int ped, uint weaponHash, int ammoCount, bool bForceInHand, bool bForceInHolster, int attachPoint, bool bAllowMultiplieCopies, float p7, float p8, uint addReason, bool bIgnoreUnlocks, float p11, bool p12)
        {
            return Function.Call<uint>((Hash)0x5E3BDDBCB83F3D84, ped, weaponHash, ammoCount, bForceInHand, bForceInHolster, attachPoint, bAllowMultiplieCopies, p7, p8, addReason, bIgnoreUnlocks, p11, p12);
        }
        public static void SetPlayerHealthRechargeMultiplier(int player, float regenRate) => Function.Call((Hash)0x8899C244EBCF70DE, player, regenRate);
        public static float GetRandomFloatNumber(double minimum, double maximum) => (float)(new Random().NextDouble() * (maximum - minimum) + minimum);
        public static bool HasBeenDamagedByAnyPed(int entity) => Function.Call<bool>((Hash)0x9934E9C42D52D87E, entity);
        public static bool HasEntityBeenDamagedByWeapon(int entity, uint weaponHash, int weaponType) => Function.Call<bool>((Hash)0xDCF06D0CDFF68424, entity, weaponHash, weaponType);
        public static void HideAllPedWeapons(int ped, bool instantlySwitchToUnarmed) => Function.Call((Hash)0xFCCC886EDE3C63EC, ped, 2, instantlySwitchToUnarmed);
        public static void SetAttributeCoreValue(int ped, int core, int value) => Function.Call((Hash)0xC6258F41D86676E0, ped, core, value);
        public static void TaskUseNearestScenarioChainToCoord(int ped, float x, float y, float z, float distance) => Function.Call((Hash)0x9FDA1B3D7E7028B3, ped, x, y, z, distance, 1, 1, 1, 1);
        public static void SetPedWoundEffect(int ped, int p1, int boneId, float moveWoundLeftRight, float bloodFountainPressure, float yaw, float bloodFountainDirection, float bloodFountainPulse, float p8, float p9) => Function.Call((Hash)0xFFD54D9FE71B966A, ped, p1, boneId, moveWoundLeftRight, bloodFountainPressure, yaw, bloodFountainDirection, bloodFountainPulse, p8, p9);
        public static bool IsPedHogtied(int ped) => Function.Call<bool>((Hash)0x3AA24CCC0D451379, ped);
        public static string RemoveSpecialCharacters(string str)
        {
            var sb = new StringBuilder();

            foreach (char c in str)
            {
                if (c >= '0' && c <= '9' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Replace("'", "''".Replace(@"\", "\\"));
        }
        public static void UIPromptClearFavouredPedForConflictResolution() => Function.Call((Hash)0x51259AE5C72D4A1B);
        public static void UIPromptClearHorizontalOrientation(int id) => Function.Call((Hash)0x6095358C4142932A, id);
        public static void UIPromptContextSetPoint(int promptId, Vector3 position) => Function.Call((Hash)0xAE84C5EE2C384FB3, promptId, position.X, position.Y, position.Z);
        public static void UIPromptContextSetSize(int promptId, float size) => Function.Call((Hash)0x0C718001B77CA468, promptId, size);
        public static int UIPromptCreate(uint inputHash, long labelName, int p2, int p3, int p4, int p5) => Function.Call<int>((Hash)0x29FA7910726C3889, inputHash, labelName, p2, p3, p4, p5);
        public static void UIPromptDelete(int promptId) => Function.Call((Hash)0x00EDE88D4D13CF59);
        public static void UIPromptDisablePromptTypeThisFrame(int promptType) => Function.Call((Hash)0xFC094EF26DD153FA, promptType);
        public static void UIPromptDisablePromptsThisFrame() => Function.Call((Hash)0xF1622CE88A1946FB);
        public static bool UIPromptDoesAmbientGroupExist(uint hash) => Function.Call<bool>((Hash)0xEB550B927B34A1BB, hash);
        public static void UIPromptEnablePromptTypeThisFrame(int promptType) => Function.Call((Hash)0x06565032897BA861, promptType);
        public static void UIPromptFilterClear() => Function.Call((Hash)0x6A2F820452017EA2);
        public static int UIPromptGetGroupActivePage(uint hash) => Function.Call<int>((Hash)0xC1FCC36C3F7286C8, hash);
        public static int UIPromptGetGroupIdForScenarioPoint(int p0, int p1) => Function.Call<int>((Hash)0xCB73D7521E7103F0, p0, p1);
        public static int UIPromptGetGroupIdForTargetEntity(int entity) => Function.Call<int>((Hash)0xB796970BD125FCE8, entity);
        public static float UIPromptGetMashModeProgress(int promptId) => Function.Call<float>((Hash)0x8A9585293863B8A5, promptId);
        public static bool UIPromptGetUrgentPulsingEnabled(int promptId) => Function.Call<bool>((Hash)0x1FBA0DABECDDB52B, promptId);
        public static bool UIPromptHasHoldAutoFillMode(int promptId) => Function.Call<bool>((Hash)0x8010BEBD0D5ED5BC, promptId);
        public static bool UIPromptHasHoldMode(int promptId) => Function.Call<bool>((Hash)0xB60C9F9ED47ABB76, promptId);
        public static bool UIPromptHasHoldModeCompleted(int promptId) => Function.Call<bool>((Hash)0xE0F65F0640EF0617, promptId);
        public static bool UIPromptHasManualMashMode(int promptId) => Function.Call<bool>((Hash)0xA6C6A4ADB3BAC409, promptId);
        public static bool UIPromptHasMashMode(int promptId) => Function.Call<bool>((Hash)0xCD072523791DDC1B, promptId);
        public static bool UIPromptHasMashModeCompleted(int promptId) => Function.Call<bool>((Hash)0x845CE958416DC473, promptId);
        public static bool UIPromptHasMashModeFailed(int promptId) => Function.Call<bool>((Hash)0x25B18E530CF39D6F, promptId);
        public static int UIPromptHasPressedTimedModeCompleted(int promptId) => Function.Call<int>((Hash)0x3CE854D250A88DAF, promptId);
        public static int UIPromptHasPressedTimedModeFailed(int promptId) => Function.Call<int>((Hash)0x1A17B9ECFF617562, promptId);
        public static bool UIPromptHasStandardModeCompleted(int promptId) => Function.Call<bool>((Hash)0xC92AC953F0A982AE, promptId, 1);
        public static bool UIPromptIsActive(int promptId) => Function.Call<bool>((Hash)0x546E342E01DE71CF, promptId);
        public static bool UIPromptIsControlActionActive(uint control) => Function.Call<bool>((Hash)0x1BE19185B8AFE299, control);
        public static bool UIPromptIsEnabled(int promptId) => Function.Call<bool>((Hash)0x0D00EDDFB58B7F28, promptId);
        public static bool UIPromptIsHoldModeRunning(int promptId) => Function.Call<bool>((Hash)0xC7D70EAEF92EFF48, promptId);
        public static bool UIPromptIsJustPressed(int promptId) => Function.Call<bool>((Hash)0x2787CC611D3FACC5, promptId);
        public static bool UIPromptIsJustReleased(int promptId) => Function.Call<bool>((Hash)0x635CC82FA297A827, promptId);
        public static bool UIPromptIsPressed(int promptId) => Function.Call<bool>((Hash)0x21E60E230086697F, promptId);
        public static bool UIPromptIsReleased(int promptId) => Function.Call<bool>((Hash)0xAFC887BA7A7756D6, promptId);
        public static bool UIPromptIsValid(int promptId) => Function.Call<bool>((Hash)0x347469FBDD1589A9, promptId);
        public static int UIPromptRegisterBegin() => Function.Call<int>((Hash)0x04F97DE45A519419);
        public static void UIPromptRegisterEnd(int promptId) => Function.Call((Hash)0xF7AA2696A22AD8B9, promptId);
        public static void UIPromptRemoveGroup(int p0, int p1) => Function.Call((Hash)0x4E52C800A28F7BE8, p0, p1);
        public static void UIPromptRestartModes(int promptId) => Function.Call((Hash)0xDC6C55DFA2C24EE5, promptId);
        public static void UIPromptSetActiveGroupThisFrame(uint hash, string p1, int p2, int p3, int p4, int promptId) => Function.Call((Hash)0xC65A45D4453C2627, hash, p1, p2, p3, p4, promptId);
        public static void UIPromptSetAllowedAction(int promptId, int p1) => Function.Call((Hash)0x565C1CE183CB0EAF, promptId, p1);
        public static void UIPromptSetAmbientGroupThisFrame(int entity, int p1, int p2, int p3, int p4, string p5, int p6) => Function.Call((Hash)0x315C81D760609108, entity, p1, p2, p3, p4, p5, p6);
        public static void UIPromptSetAttribute(int promptId, int p1, int p2) => Function.Call((Hash)0x560E76D5E2E1803F, promptId, p1, p2);
        public static void UIPromptSetBeatMode(int promptId, bool p1) => Function.Call((Hash)0xF957A1654C6322FE, promptId, p1);
        public static void UIPromptSetBeatModeGrayedOut(int promptId, bool p1) => Function.Call((Hash)0xB487A4936FBF40AC, promptId, p1);
        public static void UIPromptSetControlAction(int promptId, uint control) => Function.Call((Hash)0xB5352B7494A08258, promptId, control);
        public static void UIPromptSetEnabled(int promptId, bool toggle) => Function.Call((Hash)0x8A0FB4D03A630D21, promptId, toggle ? 1 : 0);
        public static void UIPromptSetFavouredPedForConflictResolution(int ped) => Function.Call((Hash)0x530A428705BE5DEF, ped);
        public static void UIPromptSetGroup(int promptId, int p1, int p2) => Function.Call((Hash)0x2F11D3A254169EA4, promptId, p1, p2);
        public static void UIPromptSetHoldAutoFillMode(int promptId, bool p1, int p2) => Function.Call((Hash)0x3CE932E737C145D6, promptId, p1, p2);
        public static void UIPromptSetHoldIndefinitelyMode(int promptId) => Function.Call((Hash)0xEA5CCF4EEB2F82D1, promptId);
        public static void UIPromptSetHoldMode(int promptId, int duration) => Function.Call((Hash)0x94073D5CA3F16B7B, promptId, duration);
        public static void UIPromptSetMashAutoFillMode(int promptId, int p1, int p2) => Function.Call((Hash)0x6C39587D7CC66801, promptId, p1, p2);
        public static void UIPromptSetMashIndefinitelyMode(int promptId) => Function.Call((Hash)0x7B66E89312727274, promptId);
        public static void UIPromptSetMashManualCanFailMode(int promptId, int p1, int p2, int p3, int p4) => Function.Call((Hash)0x179DCF71F705DA20, promptId, p1, p2, p3, p4);
        public static void UIPromptSetMashManualMode(int promptId, int p1, int p2, int p3, int p4) => Function.Call((Hash)0x32DF729D8BD3C1C6, promptId, p1, p2, p3, p4);
        public static void UIPromptSetMashManualModeDecaySpeed(int promptId, float speed) => Function.Call((Hash)0x7D393C247FB9B431, promptId, speed);
        public static void UIPromptSetMashManualModeIncreasePerPress(int promptId, float increase = 1f / 128f) => Function.Call((Hash)0xA0D1D79C6036A855, promptId, increase);
        public static void UIPromptSetMashManualModePressedGrowthSpeed(int promptId, float speed) => Function.Call((Hash)0x56DBB26F98582C29, promptId, speed);
        public static void UIPromptSetMashMode(int promptId, int p1) => Function.Call((Hash)0xDF6423BF071C7F71, promptId, p1);
        public static void UIPromptSetMashWithResistanceCanFailMode(int promptId, int p1, int p2, int p3) => Function.Call((Hash)0xDC0CB602DEADBA53, promptId, p1, p2, p3);
        public static void UIPromptSetMashWithResistanceMode(int promptId, int p1, int p2, int p3) => Function.Call((Hash)0xCD1BDFF15EFA79F5, promptId, p1, p2, p3);
        public static void UIPromptSetOrderingAsInputType(int promptId, int p1) => Function.Call((Hash)0x2F385ECC5200938D, promptId, p1);
        public static void UIPromptSetPressedTimedMode(int promptId, int p1) => Function.Call((Hash)0x1473D3AF51D54276, promptId, p1);
        public static void UIPromptSetPriority(int promptId, int p1) => Function.Call((Hash)0xCA24F528D0D16289, promptId, p1);
        public static int UIPromptSetRegisterHorizontalOrientation() => Function.Call<int>((Hash)0xD9459157EB22C895);
        public static void UIPromptSetRotateMode(int promptId, int p1, int p2) => Function.Call((Hash)0x7ABE7095FB3D2581, promptId, p1, p2);
        public static void UIPromptSetSpinnerPosition(int promptId, float position) => Function.Call((Hash)0x832CB510DE546282, promptId, position);
        public static void UIPromptSetSpinnerSpeed(int promptId, float speed) => Function.Call((Hash)0xAC6586A7FDCD4B68, promptId, speed);
        public static void UIPromptSetStandardMode(int promptId, int p1) => Function.Call((Hash)0xCC6656799977741B, promptId, p1);
        public static void UIPromptSetStandardizedHoldMode(int promptId, int p1) => Function.Call((Hash)0x74C7D7B72ED0D3CF, promptId, p1);
        public static void UIPromptSetTag(int promptId, string tag) => Function.Call((Hash)0xDEC85C174751292B, promptId, tag);
        public static void UIPromptSetTargetMode(int promptId, float p1, float p2, int p3) => Function.Call((Hash)0x5F6503D9CD2754EB, promptId, p1, p2, p3);
        public static void UIPromptSetTargetModeProgress(int promptId, float p1) => Function.Call((Hash)0x00123054BEC8A30F, promptId, p1);
        public static void UIPromptSetTargetModeTarget(int promptId, float p1, float p2) => Function.Call((Hash)0x5E019C45DD3B6A14, promptId, p1, p2);
        public static void UIPromptSetText(int promptId, long text) => Function.Call((Hash)0x5DD02A8318420DD7, promptId, text);
        public static void UIPromptSetTransportMode(int promptId, int p1) => Function.Call((Hash)0x876E4A35C73A6655, promptId, p1);
        public static void UIPromptSetUrgentPulsingEnabled(int promptId, int p1) => Function.Call((Hash)0xC5F428EE08FA7F2C, promptId, p1);
        public static void UIPromptSetVisible(int promptId, bool toggle) => Function.Call((Hash)0x71215ACCFDE075EE, promptId, toggle ? 1 : 0);
        public static bool UIPromptWasBeatModePressedInTimeWindow(int promptId) => Function.Call<bool>((Hash)0x1FE4788AB1430C55, promptId);
        public static async Task Wait(int delay) => await BaseScript.Delay(delay);
        public static async Task FadeIn(int duration)
        {
            DoScreenFadeIn(duration);
            while (!IsScreenFadedIn()) await Wait(100);
        }
        public static async Task FadeOut(int duration)
        {
            DoScreenFadeOut(duration);
            while (!IsScreenFadedOut()) await Wait(100);
        }
        public static void Call(long address, params InputArgument[] args) => Function.Call((Hash)address, args);
        public static void Call(ulong address, params InputArgument[] args) => Function.Call((Hash)address, args);
        public static void Call(Hash address, params InputArgument[] args) => Function.Call(address, args);
        public static void Call(string address, params InputArgument[] args) => Function.Call((Hash)(uint)GetHashKey(address), args);
        public static T Call<T>(long address, params InputArgument[] args) => Function.Call<T>((Hash)address, args);
        public static T Call<T>(ulong address, params InputArgument[] args) => Function.Call<T>((Hash)address, args);
        public static T Call<T>(Hash address, params InputArgument[] args) => Function.Call<T>(address, args);
    }
}
