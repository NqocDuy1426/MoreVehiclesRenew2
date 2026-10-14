using System;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using MoreVehiclesRenew2.Patching;
using MoreVehiclesRenew2.Utils;
using UnityEngine;

namespace MoreVehiclesRenew2.Breakthroughs
{
    /// <summary>
    /// Breakthrough 3: Emergency Slot Reclaimer
    /// Prevents city logistics from failing when approaching the 65,535 vehicle ceiling.
    /// If all slots are full, surgically frees 1 stagnant/deadlocked vehicle so that
    /// essential service/cargo vehicles can always spawn without dropping deliveries.
    /// </summary>
    internal static class EmergencySlotReclaimer
    {
        private static int searchCursor = 1;

        private sealed class VehicleManager_CreateVehicle : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(VehicleManager).GetMethod(
                    "CreateVehicle",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new Type[]
                    {
                        typeof(ushort).MakeByRefType(),
                        typeof(Randomizer).MakeByRefType(),
                        typeof(VehicleInfo),
                        typeof(Vector3),
                        typeof(TransferManager.TransferReason),
                        typeof(bool),
                        typeof(bool)
                    },
                    null);
            }

            private static void Prefix(VehicleManager __instance)
            {
                try
                {
                    // Check if vehicle array is at or near the 65,535 capacity limit
                    if (__instance.m_vehicleCount >= Constants.ModdedMaxVehicleCount - 5)
                    {
                        ReclaimStagnantSlot(__instance);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning("EmergencySlotReclaimer Prefix warning: " + ex.Message);
                }
            }
        }

        private static void ReclaimStagnantSlot(VehicleManager vm)
        {
            Vehicle[] buffer = vm.m_vehicles.m_buffer;
            int total = buffer.Length;
            int scanned = 0;

            // Scan a rolling batch of up to 512 vehicles to find a stagnant agent
            while (scanned < 512)
            {
                if (++searchCursor >= total)
                {
                    searchCursor = 1;
                }
                scanned++;

                ushort id = (ushort)searchCursor;
                if ((buffer[id].m_flags & Vehicle.Flags.Created) == 0)
                {
                    continue;
                }

                // Never kill trailers directly (let leader manage) or emergency vehicles
                if (buffer[id].m_leadingVehicle != 0)
                {
                    continue;
                }
                if ((buffer[id].m_flags & (Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2)) != 0)
                {
                    continue;
                }

                // If vehicle has high block counter (blocked for a long time) or zero speed
                if (buffer[id].m_blockCounter >= 230 || (buffer[id].GetLastFrameVelocity().sqrMagnitude < 0.0001f && buffer[id].m_path == 0))
                {
                    vm.ReleaseVehicle(id);
                    return;
                }
            }
        }

        public static IPatch CreateVehiclePatch { get; } = new VehicleManager_CreateVehicle();
    }
}
