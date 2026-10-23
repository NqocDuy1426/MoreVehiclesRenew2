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
                var replacedConstants = CodeProcessor.ReplaceOperands(instructions, Constants.VanillaMaxVehicleCount, Constants.ModdedMaxVehicleCount);
                foreach (var inst in replacedConstants)
                {
                    // If instruction is reading NetSegment.m_modifiedIndex to compare against PathUnit.m_buildIndex:
                    // Pop the NetSegment reference and load 0 instead.
                    // This prevents PathVisualizer from prematurely aborting route line rendering ("đứt quãng")!
                    if (inst.opcode == System.Reflection.Emit.OpCodes.Ldfld && inst.operand != null && inst.operand.ToString().Contains("m_modifiedIndex"))
                    {
                        yield return new CodeInstruction(System.Reflection.Emit.OpCodes.Pop);
                        yield return new CodeInstruction(System.Reflection.Emit.OpCodes.Ldc_I4_0);
                    }
                    else
                    {
                        yield return inst;
                    }
                }
            }
        }

        public static IPatch AddPathsImpl { get; } = new PathVisualizer_AddPathsImpl();
    }
}
