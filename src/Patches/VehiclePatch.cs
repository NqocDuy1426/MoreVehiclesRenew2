using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MoreVehiclesRenew2.Patching;
using MoreVehiclesRenew2.Utils;

namespace MoreVehiclesRenew2.Patches
{
    internal static class VehiclePatch
    {
        private sealed class Vehicle_GetTargetFrame : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(Vehicle).GetMethod("GetTargetFrame", BindingFlags.Instance | BindingFlags.Public, null, new Type[]
                {
                    typeof(VehicleInfo),
                    typeof(ushort)
                }, null);
            }

            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
            {
                return CodeProcessor.ReplaceOperands(instructions, Constants.VanillaMaxVehicleCount, Constants.ModdedMaxVehicleCount);
            }
        }

        public static IPatch GetTargetFrame { get; } = new Vehicle_GetTargetFrame();
    }
}
