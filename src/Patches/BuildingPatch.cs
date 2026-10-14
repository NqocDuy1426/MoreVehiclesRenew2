using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MoreVehiclesRenew2.Patching;
using MoreVehiclesRenew2.Utils;

namespace MoreVehiclesRenew2.Patches
{
    internal static class BuildingPatch
    {
        private sealed class Building_RemoveOwnVehicle : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(Building).GetMethod("RemoveOwnVehicle", BindingFlags.Instance | BindingFlags.Public, null, new Type[]
                {
                    typeof(ushort),
                    typeof(Vehicle).MakeByRefType()
                }, null);
            }

            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
            {
                return CodeProcessor.ReplaceOperands(instructions, Constants.VanillaMaxVehicleCount, Constants.ModdedMaxVehicleCount);
            }
        }

        private sealed class Building_IsOwnVehicle : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(Building).GetMethod("IsOwnVehicle", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(ushort) }, null);
            }

            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
            {
                return CodeProcessor.ReplaceOperands(instructions, Constants.VanillaMaxVehicleCount, Constants.ModdedMaxVehicleCount);
            }
        }

        private sealed class Building_RemoveGuestVehicle : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(Building).GetMethod("RemoveGuestVehicle", BindingFlags.Instance | BindingFlags.Public, null, new Type[]
                {
                    typeof(ushort),
                    typeof(Vehicle).MakeByRefType()
                }, null);
            }

            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
            {
                return CodeProcessor.ReplaceOperands(instructions, Constants.VanillaMaxVehicleCount, Constants.ModdedMaxVehicleCount);
            }
        }

        private sealed class Building_IsGuestVehicle : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(Building).GetMethod("IsGuestVehicle", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(ushort) }, null);
            }

            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
            {
                return CodeProcessor.ReplaceOperands(instructions, Constants.VanillaMaxVehicleCount, Constants.ModdedMaxVehicleCount);
            }
        }

        public static IPatch RemoveOwnVehicle { get; } = new Building_RemoveOwnVehicle();
        public static IPatch IsOwnVehicle { get; } = new Building_IsOwnVehicle();
        public static IPatch RemoveGuestVehicle { get; } = new Building_RemoveGuestVehicle();
        public static IPatch IsGuestVehicle { get; } = new Building_IsGuestVehicle();
    }
}
