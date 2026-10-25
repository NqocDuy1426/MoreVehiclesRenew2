using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MoreVehiclesRenew2.Patching;
using MoreVehiclesRenew2.Utils;

namespace MoreVehiclesRenew2.Patches
{
    /// <summary>
    /// Patches PathVisualizer.AddPathsImpl to:
    /// 1. Expand the vehicle loop limit from 16,384 to 65,536.
    /// 2. Prevent route lines from prematurely fading, blurring, or getting cut off
    ///    by neutralizing NetSegment.m_modifiedIndex >= PathUnit.m_buildIndex checks on active segments.
    /// </summary>
    internal static class PathVisualizerPatch
    {
        private sealed class PathVisualizer_AddPathsImpl : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(PathVisualizer).GetMethod(
                    "AddPathsImpl",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(int), typeof(int) },
                    null);
            }

            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
            {
                var replacedConstants = CodeProcessor.ReplaceOperands(
                    instructions,
                    Constants.VanillaMaxVehicleCount,
                    Constants.ModdedMaxVehicleCount);

                foreach (var inst in replacedConstants)
                {
                    // If instruction is reading NetSegment.m_modifiedIndex to compare against PathUnit.m_buildIndex:
                    // Pop the NetSegment reference and load 0 instead.
                    // This prevents PathVisualizer from prematurely aborting route line rendering ("đứt quãng / mờ")!
                    if (inst.opcode == OpCodes.Ldfld && inst.operand != null && inst.operand.ToString().Contains("m_modifiedIndex"))
                    {
                        yield return new CodeInstruction(OpCodes.Pop);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
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
