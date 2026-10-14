using System;
using System.Reflection;
using ColossalFramework;
using HarmonyLib;
using MoreVehiclesRenew2.Patching;
using MoreVehiclesRenew2.Utils;

namespace MoreVehiclesRenew2.Breakthroughs
{
    /// <summary>
    /// Breakthrough 2: Turbo Spawn Dispatcher
    /// Accelerates vehicle dispatch and transfer matching by 4x when traffic is cleared
    /// or when city demand is high, preventing economic collapse from slow vehicle spawning.
    /// </summary>
    internal static class TurboSpawnDispatcher
    {
        private static readonly MethodInfo MatchOffersMethod = typeof(TransferManager).GetMethod(
            "MatchOffers",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo GetFrameReasonMethod = typeof(TransferManager).GetMethod(
            "GetFrameReason",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private sealed class TransferManager_SimulationStepImpl : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(TransferManager).GetMethod(
                    "SimulationStepImpl",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(int) },
                    null);
            }

            private static void Postfix(TransferManager __instance, int subStep)
            {
                if (subStep == 0 || MatchOffersMethod == null || GetFrameReasonMethod == null)
                {
                    return;
                }

                try
                {
                    VehicleManager vm = Singleton<VehicleManager>.instance;
                    if (vm == null) return;

                    int vehicleCount = vm.m_vehicleCount;

                    // When vehicle pool has plenty of room (e.g. < 58,000 active vehicles):
                    // Quadruple the transfer matching throughput across complementary frame phases!
                    if (vehicleCount < 58000)
                    {
                        uint currentFrame = Singleton<SimulationManager>.instance.m_currentFrameIndex;
                        int frameIndex = (int)(currentFrame & 0xFF);

                        // Pass 1: phase offset +64
                        object reason1 = GetFrameReasonMethod.Invoke(__instance, new object[] { (frameIndex + 64) & 0xFF });
                        if (reason1 != null)
                        {
                            MatchOffersMethod.Invoke(__instance, new object[] { reason1 });
                        }

                        // Pass 2: phase offset +128
                        object reason2 = GetFrameReasonMethod.Invoke(__instance, new object[] { (frameIndex + 128) & 0xFF });
                        if (reason2 != null)
                        {
                            MatchOffersMethod.Invoke(__instance, new object[] { reason2 });
                        }

                        // Pass 3: phase offset +192
                        object reason3 = GetFrameReasonMethod.Invoke(__instance, new object[] { (frameIndex + 192) & 0xFF });
                        if (reason3 != null)
                        {
                            MatchOffersMethod.Invoke(__instance, new object[] { reason3 });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning("TurboSpawnDispatcher Postfix warning: " + ex.Message);
                }
            }
        }

        public static IPatch SimulationStepPatch { get; } = new TransferManager_SimulationStepImpl();
    }
}
