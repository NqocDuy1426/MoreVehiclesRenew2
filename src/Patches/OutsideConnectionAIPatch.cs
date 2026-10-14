using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MoreVehiclesRenew2.Patching;
using MoreVehiclesRenew2.Utils;

namespace MoreVehiclesRenew2.Patches
{
    internal static class OutsideConnectionAIPatch
    {
        private sealed class OutsideConnectionAI_DummyTrafficProbability : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(OutsideConnectionAI).GetMethod("DummyTrafficProbability", BindingFlags.Static | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            }

            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
            {
                return CodeProcessor.ReplaceOperands(instructions, Constants.VanillaMaxVehicleCount, Constants.ModdedMaxVehicleCount);
            }
        }

        public static IPatch DummyTrafficProbability { get; } = new OutsideConnectionAI_DummyTrafficProbability();
    }
}
