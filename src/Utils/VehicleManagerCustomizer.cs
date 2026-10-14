using System.Reflection;
using ColossalFramework;
using MoreVehiclesRenew2.Utils;

namespace MoreVehiclesRenew2.Utils
{
    internal static class VehicleManagerCustomizer
    {
        public static void Customize()
        {
            ChangeArraysSize((uint)Constants.ModdedMaxVehicleCount, (uint)Constants.ModdedMaxParkedVehicleCount);
            Log.Info("More Vehicles Renew 2 customized the vehicle manager arrays (Active: 65,536, Parked: 65,536)");
        }

        public static void Revert()
        {
            ChangeArraysSize((uint)Constants.VanillaMaxVehicleCount, (uint)Constants.VanillaMaxParkedVehicleCount);
            Log.Info("More Vehicles Renew 2 reverted the vehicle manager arrays to vanilla (Active: 16,384, Parked: 32,768)");
        }

        private static void ChangeArraysSize(uint vehicleCount, uint parkedVehicleCount)
        {
            ChangeArray16Size<Vehicle>("m_vehicles", vehicleCount);
            ChangeArray16Size<VehicleParked>("m_parkedVehicles", parkedVehicleCount);
            ChangeArraySize("m_renderBuffer", vehicleCount >> 6);
            ChangeArraySize("m_renderBuffer2", parkedVehicleCount >> 6);
            ChangeArraySize("m_updatedParked", parkedVehicleCount >> 6);
        }

        private static void ChangeArray16Size<T>(string arrayFieldName, uint size) where T : struct
        {
            FieldInfo field = GetField(arrayFieldName);
            if (field != null)
            {
                Array16<T> val = new Array16<T>(size);
                field.SetValue(Singleton<VehicleManager>.instance, val);
                ushort num = 0;
                val.CreateItem(out num);
            }
        }

        private static void ChangeArraySize(string arrayFieldName, uint size)
        {
            FieldInfo field = GetField(arrayFieldName);
            if (field != null)
            {
                ulong[] value = new ulong[size];
                field.SetValue(Singleton<VehicleManager>.instance, value);
            }
        }

        private static FieldInfo GetField(string fieldName)
        {
            FieldInfo field = typeof(VehicleManager).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                Log.Error("Failed to setup VehicleManager: no field '" + fieldName + "'");
            }
            return field;
        }
    }
}
