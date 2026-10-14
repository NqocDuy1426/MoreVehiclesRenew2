using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MoreVehiclesRenew2.Patching;
using MoreVehiclesRenew2.Utils;

namespace MoreVehiclesRenew2.Patches
{
    internal static class PathVisualizerPatch
    {
        private sealed class PathVisualizer_AddPathsImpl : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(PathVisualizer).GetMethod("AddPathsImpl", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[]
                {
                    typeof(int),
                    typeof(int)
                }, null);
            }

            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
            {
                return CodeProcessor.ReplaceOperands(instructions, Constants.VanillaMaxVehicleCount, Constants.ModdedMaxVehicleCount);
            }
        }

        public static IPatch AddPathsImpl { get; } = new PathVisualizer_AddPathsImpl();
    }
}
