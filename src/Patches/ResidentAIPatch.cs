using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MoreVehiclesRenew2.Patching;
using MoreVehiclesRenew2.Utils;

namespace MoreVehiclesRenew2.Patches
{
    internal static class ResidentAIPatch
    {
        private sealed class ResidentAI_DoRandomMove : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(ResidentAI).GetMethod("DoRandomMove", BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            }

            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
            {
                return CodeProcessor.ReplaceOperands(instructions, Constants.VanillaMaxVehicleCount, Constants.ModdedMaxVehicleCount);
            }
        }

        public static IPatch DoRandomMove { get; } = new ResidentAI_DoRandomMove();
    }
}
