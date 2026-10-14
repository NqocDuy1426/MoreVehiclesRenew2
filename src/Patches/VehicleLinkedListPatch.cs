using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MoreVehiclesRenew2.Patching;
using MoreVehiclesRenew2.Utils;

namespace MoreVehiclesRenew2.Patches
{
    internal static class VehicleLinkedListPatch
    {
        private sealed class VehicleManager_EndRenderingImpl : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(VehicleManager).GetMethod("EndRenderingImpl", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(RenderManager.CameraInfo) }, null);
            }

            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
            {
                return CodeProcessor.ReplaceOperands(
                    CodeProcessor.ReplaceOperands(instructions, Constants.VanillaMaxVehicleCount, Constants.ModdedMaxVehicleCount),
                    Constants.VanillaMaxParkedVehicleCount,
                    Constants.ModdedMaxParkedVehicleCount);
            }
        }

        private sealed class VehicleManager_PlayAudioImpl : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(VehicleManager).GetMethod("PlayAudioImpl", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(AudioManager.ListenerInfo) }, null);
            }

            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
            {
                return CodeProcessor.ReplaceOperands(instructions, Constants.VanillaMaxVehicleCount, Constants.ModdedMaxVehicleCount);
            }
        }

        private sealed class VehicleManager_ReleaseVehicleImplementation : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(VehicleManager).GetMethod("ReleaseVehicleImplementation", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[]
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

        public static IPatch EndRenderingImpl { get; } = new VehicleManager_EndRenderingImpl();
        public static IPatch PlayAudioImpl { get; } = new VehicleManager_PlayAudioImpl();
        public static IPatch ReleaseVehicleImplementation { get; } = new VehicleManager_ReleaseVehicleImplementation();
    }
}
