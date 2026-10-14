using System;
using System.Reflection;
using ColossalFramework;
using MoreVehiclesRenew2.Patching;
using MoreVehiclesRenew2.Utils;

namespace MoreVehiclesRenew2.Patches
{
    internal static class EventManagerPatch
    {
        private sealed class EventManager_InvalidatePathsOnClosedRoads : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(EventManager).GetMethod("InvalidatePathsOnClosedRoads", BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
            }

            private static bool Prefix()
            {
                PathManager pathManager = Singleton<PathManager>.instance;
                Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                NetSegment[] segments = Singleton<NetManager>.instance.m_segments.m_buffer;

                for (int i = 1; i < vehicles.Length; i++)
                {
                    if ((vehicles[i].m_flags & Vehicle.Flags.Created) == 0)
                    {
                        continue;
                    }
                    int pathPositionIndex = vehicles[i].m_pathPositionIndex;
                    pathPositionIndex = (pathPositionIndex != 255) ? (pathPositionIndex >> 1) : 0;
                    bool invalidate = false;
                    uint currentPath = vehicles[i].m_path;
                    int iterations = 0;

                    while (currentPath != 0)
                    {
                        int positionCount = pathManager.m_pathUnits.m_buffer[currentPath].m_positionCount;
                        for (int j = pathPositionIndex; j < positionCount; j++)
                        {
                            ushort segment = pathManager.m_pathUnits.m_buffer[currentPath].GetPosition(j).m_segment;
                            if (segment > 0 && ((int)segments[segment].m_flags2 & 0x1000) != 0)
                            {
                                if (iterations == 0)
                                {
                                    vehicles[i].m_flags2 |= (Vehicle.Flags2)0x40;
                                }
                                invalidate = true;
                                break;
                            }
                        }
                        if (invalidate)
                        {
                            vehicles[i].Info.m_vehicleAI.InvalidatePath((ushort)i, ref vehicles[i], (ushort)i, ref vehicles[i]);
                            break;
                        }
                        pathPositionIndex = 0;
                        currentPath = pathManager.m_pathUnits.m_buffer[currentPath].m_nextPathUnit;
                        if (++iterations >= 262144)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid path list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
                return false;
            }
        }

        public static IPatch InvalidatePathsOnClosedRoads { get; } = new EventManager_InvalidatePathsOnClosedRoads();
    }
}
