using System;
using System.Reflection;
using ColossalFramework;
using HarmonyLib;
using MoreVehiclesRenew2.Patching;
using MoreVehiclesRenew2.Utils;
using UnityEngine;

namespace MoreVehiclesRenew2.Breakthroughs
{
    /// <summary>
    /// Breakthrough 1: Ghost & Invisible Vehicle Annihilator
    /// Instantly dissolves invisible vehicles (missing or corrupted prefabs/AI),
    /// orphaned trailers, vehicles stranded on bulldozed roads, and permanent deadlocks
    /// to completely eliminate phantom roadblocks and restore crystal-clear traffic routes.
    /// </summary>
    internal static class GhostVehicleAnnihilator
    {
        private sealed class NetManager_ReleaseSegmentImplementation : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(NetManager).GetMethod(
                    "ReleaseSegmentImplementation",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(ushort), typeof(NetSegment).MakeByRefType(), typeof(bool) },
                    null);
            }

            private static void Prefix(ushort segment, ref NetSegment data, bool keepNodes)
            {
                try
                {
                    if (data.m_flags == NetSegment.Flags.None)
                    {
                        return;
                    }

                    VehicleManager vm = Singleton<VehicleManager>.instance;
                    NetManager nm = Singleton<NetManager>.instance;
                    if (vm == null || nm == null)
                    {
                        return;
                    }

                    ScanAndReleaseVehiclesOnSegment(segment, vm, nm);
                }
                catch (Exception ex)
                {
                    Log.Warning("GhostVehicleAnnihilator NetManager Prefix warning: " + ex.Message);
                }
            }
        }

        public static void ScanAndReleaseVehiclesOnSegment(ushort segmentID, VehicleManager vm, NetManager nm)
        {
            PathManager pm = Singleton<PathManager>.instance;
            if (pm == null) return;

            Vehicle[] buffer = vm.m_vehicles.m_buffer;
            for (int i = 1; i < buffer.Length; i++)
            {
                if ((buffer[i].m_flags & Vehicle.Flags.Created) == 0)
                {
                    continue;
                }

                uint path = buffer[i].m_path;
                if (path != 0)
                {
                    int pathIndex = buffer[i].m_pathPositionIndex >> 1;
                    int count = pm.m_pathUnits.m_buffer[path].m_positionCount;
                    if (pathIndex >= 0 && pathIndex < count)
                    {
                        ushort seg = pm.m_pathUnits.m_buffer[path].GetPosition(pathIndex).m_segment;
                        if (seg == segmentID)
                        {
                            // Vehicle was traveling on the destroyed road segment: release it cleanly!
                            vm.ReleaseVehicle((ushort)i);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called from VehicleManager.SimulationStepImpl on the slice of 4,096 vehicles being simulated.
        /// Cycles through all 65,536 vehicle slots every 16 frames.
        /// </summary>
        public static void InspectAndCleanFrameSlice(int startVehicle, int endVehicle)
        {
            try
            {
                VehicleManager vm = Singleton<VehicleManager>.instance;
                NetManager nm = Singleton<NetManager>.instance;
                PathManager pm = Singleton<PathManager>.instance;
                BuildingManager bm = Singleton<BuildingManager>.instance;
                if (vm == null || nm == null || pm == null || bm == null) return;

                Vehicle[] vehicles = vm.m_vehicles.m_buffer;
                int max = Math.Min(endVehicle, vehicles.Length);

                for (int i = startVehicle; i < max; i++)
                {
                    if (i == 0) continue;
                    if ((vehicles[i].m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Deleted)) != Vehicle.Flags.Created)
                    {
                        continue;
                    }

                    // 1. INVISIBLE VEHICLE ANNIHILATION: Missing or corrupted prefab / AI
                    // When Info is null, the vehicle cannot render a 3D mesh (100% invisible)
                    // and its AI never runs to move it, permanently blocking all traffic behind it!
                    VehicleInfo info = vehicles[i].Info;
                    if (info == null || info.m_prefabDataIndex < 0 || info.m_vehicleAI == null)
                    {
                        vm.ReleaseVehicle((ushort)i);
                        continue;
                    }

                    // 2. ORPHANED TRAILER ANNIHILATION
                    // If a trailer's leader is destroyed or nonexistent, it sits paralyzed forever.
                    if (vehicles[i].m_leadingVehicle != 0)
                    {
                        ushort leader = vehicles[i].m_leadingVehicle;
                        if (leader >= vehicles.Length || (vehicles[leader].m_flags & Vehicle.Flags.Created) == 0)
                        {
                            vm.ReleaseVehicle((ushort)i);
                            continue;
                        }
                        // Valid trailers are governed by their leader
                        continue;
                    }

                    // 3. DESTROYED ROAD SEGMENT DETECTION
                    uint path = vehicles[i].m_path;
                    if (path != 0)
                    {
                        int posIdx = vehicles[i].m_pathPositionIndex >> 1;
                        if (posIdx >= 0 && posIdx < pm.m_pathUnits.m_buffer[path].m_positionCount)
                        {
                            ushort segment = pm.m_pathUnits.m_buffer[path].GetPosition(posIdx).m_segment;
                            if (segment > 0 && (nm.m_segments.m_buffer[segment].m_flags & NetSegment.Flags.Created) == 0)
                            {
                                vm.ReleaseVehicle((ushort)i);
                                continue;
                            }
                        }
                    }

                    // 4. PERMANENT DEADLOCK / STALLED ROADBLOCK CLEARING
                    // Detect vehicles stuck for >= 80 frames with zero velocity
                    if (vehicles[i].m_blockCounter >= 80 && vehicles[i].GetLastFrameVelocity().sqrMagnitude < 0.0001f)
                    {
                        // Protect active emergency vehicles with sirens blaring
                        if ((vehicles[i].m_flags & (Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2)) != 0)
                        {
                            continue;
                        }

                        // Check if destination building is destroyed or non-existent
                        ushort target = vehicles[i].m_targetBuilding;
                        if (target != 0 && (bm.m_buildings.m_buffer[target].m_flags & Building.Flags.Created) == 0)
                        {
                            vm.ReleaseVehicle((ushort)i);
                            continue;
                        }

                        // Check if vehicle has no path and cannot spawn
                        if (path == 0 && (vehicles[i].m_flags & Vehicle.Flags.WaitingPath) == 0)
                        {
                            vm.ReleaseVehicle((ushort)i);
                            continue;
                        }

                        // Vehicle is deadlocked in traffic for >= 120 frames (2 full seconds of gridlock):
                        // Safely release it to break the deadlock and allow the entire traffic line to flow!
                        if (vehicles[i].m_blockCounter >= 120)
                        {
                            vm.ReleaseVehicle((ushort)i);
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning("InspectAndCleanFrameSlice error: " + ex.Message);
            }
        }

        public static IPatch ReleaseSegmentPatch { get; } = new NetManager_ReleaseSegmentImplementation();
    }
}
