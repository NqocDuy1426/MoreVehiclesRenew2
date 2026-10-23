using System;
using System.Reflection;
using ColossalFramework;
using MoreVehiclesRenew2.Patching;
using MoreVehiclesRenew2.Utils;

namespace MoreVehiclesRenew2.Patches
{
    /// <summary>
    /// Prevents premature path invalidation and vehicle freeze caused by NetSegment.m_modifiedIndex >= PathUnit.m_buildIndex.
    /// In Cities: Skylines, background simulation (such as road congestion flags or lane updates) updates m_modifiedIndex.
    /// Vanilla PathUnit.GetPosition(int, out Position) returns false if m_modifiedIndex >= m_buildIndex, causing
    /// CarAI to immediately slam on the brakes (targetPos3.w = 0), dump the path, and freeze in the Confused ("Hoang mang") state.
    /// This patch guarantees that as long as the physical road segment is created and alive, the path position remains valid.
    /// </summary>
    internal static class PathUnitPatch
    {
        private sealed class PathUnit_GetPosition : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(PathUnit).GetMethod(
                    "GetPosition",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new Type[] { typeof(int), typeof(PathUnit.Position).MakeByRefType() },
                    null);
            }

            private static bool Prefix(ref PathUnit __instance, int index, out PathUnit.Position position, ref bool __result)
            {
                position = __instance.GetPosition(index);
                ushort segment = position.m_segment;
                if (segment == 0)
                {
                    __result = false;
                    return false;
                }

                NetManager nm = Singleton<NetManager>.instance;
                if (nm == null || nm.m_segments == null || nm.m_segments.m_buffer == null || segment >= nm.m_segments.m_buffer.Length)
                {
                    __result = false;
                    return false;
                }

                NetSegment.Flags flags = nm.m_segments.m_buffer[segment].m_flags;
                if ((flags & NetSegment.Flags.Created) == 0 || (flags & NetSegment.Flags.Deleted) != 0)
                {
                    __result = false;
                    return false;
                }

                // If segment has suffered catastrophic failure (collapsed/flooded), path is truly broken
                if ((flags & (NetSegment.Flags.Collapsed | NetSegment.Flags.Flooded)) != 0)
                {
                    __result = false;
                    return false;
                }

                NetInfo info = nm.m_segments.m_buffer[segment].Info;
                if (info == null || info.m_lanes == null || position.m_lane >= info.m_lanes.Length)
                {
                    __result = false;
                    return false;
                }

                // Physical road is alive, intact, and lane exists: ALLOW VEHICLE TO PROCEED SMOOTHLY!
                __result = true;
                return false;
            }
        }

        public static IPatch GetPosition { get; } = new PathUnit_GetPosition();
    }
}
