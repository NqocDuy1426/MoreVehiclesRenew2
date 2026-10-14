using System;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Math;
using MoreVehiclesRenew2.Patching;
using MoreVehiclesRenew2.Utils;
using UnityEngine;

namespace MoreVehiclesRenew2.Patches
{
    internal static class CinematicCameraControllerPatch
    {
        private sealed class CinematicCameraController_GetRandomVehicle : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(CinematicCameraController).GetMethod("GetRandomVehicle", BindingFlags.Instance | BindingFlags.Public, null, new Type[]
                {
                    typeof(ItemClass.Service),
                    typeof(ItemClass.SubService),
                    typeof(ItemClass.Level)
                }, null);
            }

            private static bool Prefix(ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, ref ushort __result)
            {
                int num = Singleton<SimulationManager>.instance.m_randomizer.Int32(1, Constants.ModdedMaxVehicleCount - 1);
                Vehicle[] buffer = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                int current = num;
                Vector3 position = default(Vector3);
                Quaternion rotation = default(Quaternion);
                Vector3 velocity = default(Vector3);

                for (int i = 0; i < Constants.ModdedMaxVehicleCount - 1; i++)
                {
                    if (++current >= Constants.ModdedMaxVehicleCount)
                    {
                        current = 1;
                    }
                    if (buffer[current].m_flags == 0)
                    {
                        continue;
                    }
                    VehicleInfo info = buffer[current].Info;
                    if ((service == ItemClass.Service.None || info.GetService() == service) &&
                        (subService == ItemClass.SubService.None || info.GetSubService() == subService) &&
                        (level == ItemClass.Level.None || info.GetClassLevel() == level))
                    {
                        InstanceID instanceId = default(InstanceID);
                        instanceId.Vehicle = (ushort)current;
                        if (InstanceManager.GetPosition(instanceId, out position, out rotation, out velocity) && IsInsideCityLimits(position))
                        {
                            __result = (ushort)current;
                            return false;
                        }
                    }
                }
                __result = 0;
                return false;
            }
        }

        private sealed class CinematicCameraController_GetVehicleWithName : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(CinematicCameraController).GetMethod("GetVehicleWithName", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(string) }, null);
            }

            private static bool Prefix(string name, ref ushort __result)
            {
                Vehicle[] buffer = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                for (int i = 1; i < Constants.ModdedMaxVehicleCount; i++)
                {
                    if ((buffer[i].m_flags & Vehicle.Flags.CustomName) != 0)
                    {
                        InstanceID instanceId = default(InstanceID);
                        instanceId.Vehicle = (ushort)i;
                        if (Singleton<InstanceManager>.instance.GetName(instanceId).Equals(name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            __result = (ushort)i;
                            return false;
                        }
                    }
                }
                __result = 0;
                return false;
            }
        }

        private sealed class CinematicCameraController_GetNearestVehicle : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(CinematicCameraController).GetMethod("GetNearestVehicle", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(Vector3) }, null);
            }

            private static bool Prefix(Vector3 position, ref ushort __result)
            {
                float minDistanceSqr = float.MaxValue;
                ushort nearestVehicle = 0;
                Vehicle[] buffer = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                Vector3 vehPos = default(Vector3);
                Quaternion vehRot = default(Quaternion);
                Vector3 vehVel = default(Vector3);

                for (int i = 1; i < Constants.ModdedMaxVehicleCount; i++)
                {
                    if ((buffer[i].m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Deleted)) != Vehicle.Flags.Created)
                    {
                        continue;
                    }
                    InstanceID instanceId = default(InstanceID);
                    instanceId.Vehicle = (ushort)i;
                    if (InstanceManager.GetPosition(instanceId, out vehPos, out vehRot, out vehVel))
                    {
                        Vector3 diff = position - vehPos;
                        float sqrMagnitude = diff.sqrMagnitude;
                        if (sqrMagnitude < minDistanceSqr)
                        {
                            minDistanceSqr = sqrMagnitude;
                            nearestVehicle = (ushort)i;
                        }
                    }
                }
                __result = nearestVehicle;
                return false;
            }
        }

        private static readonly Func<Vector3, bool> IsInsideCityLimits = (Func<Vector3, bool>)Delegate.CreateDelegate(
            typeof(Func<Vector3, bool>),
            typeof(CinematicCameraController).GetMethod("IsInsideCityLimits", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(Vector3) }, null));

        public static IPatch GetRandomVehicle { get; } = new CinematicCameraController_GetRandomVehicle();
        public static IPatch GetVehicleWithName { get; } = new CinematicCameraController_GetVehicleWithName();
        public static IPatch GetNearestVehicle { get; } = new CinematicCameraController_GetNearestVehicle();
    }
}
