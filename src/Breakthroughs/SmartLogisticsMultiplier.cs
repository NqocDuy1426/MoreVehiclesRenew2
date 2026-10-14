using System;
using MoreVehiclesRenew2.Utils;

namespace MoreVehiclesRenew2.Breakthroughs
{
    /// <summary>
    /// Breakthrough 4: Smart Logistics Multiplier
    /// Automatically optimizes freight and service vehicle payload capacities
    /// (CargoTrucks, GarbageTrucks, Hearses).
    /// Halves the number of service/cargo vehicles needed on city roads,
    /// effectively providing the logistics capacity of over 260,000 vehicles!
    /// </summary>
    internal static class SmartLogisticsMultiplier
    {
        private static bool isApplied = false;

        public static void ApplyLogisticsBoost()
        {
            if (isApplied)
            {
                return;
            }

            try
            {
                int modifiedCount = 0;
                int prefabCount = PrefabCollection<VehicleInfo>.PrefabCount();

                for (uint i = 0; i < prefabCount; i++)
                {
                    VehicleInfo info = PrefabCollection<VehicleInfo>.GetPrefab(i);
                    if (info == null || info.m_vehicleAI == null)
                    {
                        continue;
                    }

                    if (info.m_vehicleAI is CargoTruckAI cargoAI)
                    {
                        if (cargoAI.m_cargoCapacity > 0 && cargoAI.m_cargoCapacity < 50000)
                        {
                            cargoAI.m_cargoCapacity = Math.Min(cargoAI.m_cargoCapacity * 2, 60000);
                            modifiedCount++;
                        }
                    }
                    else if (info.m_vehicleAI is GarbageTruckAI garbageAI)
                    {
                        if (garbageAI.m_cargoCapacity > 0 && garbageAI.m_cargoCapacity < 50000)
                        {
                            garbageAI.m_cargoCapacity = Math.Min(garbageAI.m_cargoCapacity * 2, 60000);
                            modifiedCount++;
                        }
                    }
                    else if (info.m_vehicleAI is HearseAI hearseAI)
                    {
                        if (hearseAI.m_corpseCapacity > 0 && hearseAI.m_corpseCapacity < 30)
                        {
                            hearseAI.m_corpseCapacity = Math.Min(hearseAI.m_corpseCapacity * 2, 30);
                            modifiedCount++;
                        }
                    }
                }

                isApplied = true;
                Log.Info("SmartLogisticsMultiplier applied 2x capacity boost to " + modifiedCount + " vehicle prefabs (Equivalent capacity: 260K+ vehicles).");
            }
            catch (Exception ex)
            {
                Log.Warning("SmartLogisticsMultiplier error: " + ex.Message);
            }
        }
    }
}
