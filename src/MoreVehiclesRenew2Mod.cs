using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.Plugins;
using ICities;
using CitiesHarmony.API;
using MoreVehiclesRenew2.Patches;
using MoreVehiclesRenew2.Patching;
using MoreVehiclesRenew2.Utils;

namespace MoreVehiclesRenew2
{
    public sealed class MoreVehiclesRenew2Mod : LoadingExtensionBase, IUserMod
    {
        private MethodPatcher patcher;
        private bool isPatched = false;

        public string Name => Constants.ModName;
        public string Description => "65K Active / 131K Total Vehicles + Turbo Spawn, Ghost Vehicle Eliminator & Smart Logistics (260K throughput). Renew 2 by NgocDuy.";

        public void OnEnabled()
        {
            if (Compatibility.AreAnyIncompatibleModsActive())
            {
                Log.Info("More Vehicles Renew 2 cannot be started because incompatible mods were detected.");
                return;
            }

            HarmonyHelper.DoOnHarmonyReady(() =>
            {
                EnableMod();
            });
        }

        private void EnableMod()
        {
            if (isPatched)
            {
                return;
            }

            List<string> conflicts = Compatibility.GetActiveConflictingVehicleMods(this);
            if (conflicts.Count > 0)
            {
                Log.Warning("Detected legacy vehicle mod(s) enabled: " + string.Join(", ", conflicts.ToArray()) + ". Please disable them to avoid conflict with More Vehicles Renew 2.");
            }

            Log.Info("More Vehicles Renew 2 is enabling...");

            IPatch[] array = new IPatch[]
            {
                BuildingPatch.RemoveOwnVehicle,
                BuildingPatch.IsOwnVehicle,
                BuildingPatch.RemoveGuestVehicle,
                BuildingPatch.IsGuestVehicle,
                CinematicCameraControllerPatch.GetRandomVehicle,
                CinematicCameraControllerPatch.GetVehicleWithName,
                CinematicCameraControllerPatch.GetNearestVehicle,
                EventManagerPatch.InvalidatePathsOnClosedRoads,
                OutsideConnectionAIPatch.DummyTrafficProbability,
                PathVisualizerPatch.AddPathsImpl,
                ResidentAIPatch.DoRandomMove,
                TouristAIPatch.DoRandomMove,
                VehicleManagerPatch.DataDeserialize,
                VehicleManagerPatch.UpdateData,
                VehicleManagerPatch.AirlineModified,
                VehicleManagerPatch.SimulationStepImpl,
                VehiclePatch.GetTargetFrame,
                VehicleLinkedListPatch.EndRenderingImpl,
                VehicleLinkedListPatch.PlayAudioImpl,
                VehicleLinkedListPatch.ReleaseVehicleImplementation,
                // Breakthrough 1: Ghost Vehicle Annihilator on Demolished Roads
                MoreVehiclesRenew2.Breakthroughs.GhostVehicleAnnihilator.ReleaseSegmentPatch,
                // Breakthrough 2: Turbo Spawner on Transfer Matching
                MoreVehiclesRenew2.Breakthroughs.TurboSpawnDispatcher.SimulationStepPatch,
                // Breakthrough 3: Emergency Vehicle Slot Reclaimer
                MoreVehiclesRenew2.Breakthroughs.EmergencySlotReclaimer.CreateVehiclePatch
            };

            patcher = new MethodPatcher(Constants.HarmonyId, array);
            HashSet<IPatch> applied = patcher.Apply();

            if (applied.Count == array.Length)
            {
                isPatched = true;
                Log.Info("More Vehicles Renew 2 successfully applied all " + applied.Count + "/" + array.Length + " patches.");
                Singleton<PluginManager>.instance.eventPluginsChanged += ModsChanged;
                VehicleManagerCustomizer.Customize();
            }
            else
            {
                Log.Error("More Vehicles Renew 2 failed to apply all patches: " + applied.Count + "/" + array.Length + " applied.");
                patcher.Revert();
                patcher = null;
                isPatched = false;
            }
        }

        public void OnDisabled()
        {
            if (patcher == null && !isPatched)
            {
                return;
            }

            try
            {
                Singleton<PluginManager>.instance.eventPluginsChanged -= ModsChanged;
                if (patcher != null)
                {
                    patcher.Revert();
                    Log.Info("More Vehicles Renew 2 Harmony patches have been reverted.");
                }
                VehicleManagerCustomizer.Revert();
            }
            finally
            {
                patcher = null;
                isPatched = false;
                Log.Info("More Vehicles Renew 2 has been disabled.");
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (patcher == null || (mode != LoadMode.LoadGame && mode != LoadMode.NewGame && mode != LoadMode.NewGameFromScenario))
            {
                return;
            }

            SimulationMetaData metaData = Singleton<SimulationManager>.instance.m_metaData;
            if (metaData == null)
            {
                return;
            }

            lock (metaData)
            {
                if (metaData.m_modOverride == null)
                {
                    metaData.m_modOverride = new Dictionary<string, bool>();
                }
                // Stamp all 3 keys (More Vehicles, More Vehicles Renew, More Vehicles Renew 2) for 100% cross-mod compatibility
                metaData.m_modOverride[Constants.MetadataModNameLegacy] = true;
                metaData.m_modOverride[Constants.MetadataModNameRenew] = true;
                metaData.m_modOverride[Constants.MetadataModNameRenew2] = true;
                Log.Info("More Vehicles Renew 2 recorded mod overrides for More Vehicles, Renew, and Renew 2 in SimulationMetaData.");
            }

            // Breakthrough 4: Apply Smart Logistics Multiplier (2x freight & service vehicle capacity)
            MoreVehiclesRenew2.Breakthroughs.SmartLogisticsMultiplier.ApplyLogisticsBoost();
        }

        private void ModsChanged()
        {
            if (Compatibility.AreAnyIncompatibleModsActive())
            {
                Log.Info("More Vehicles Renew 2 detected incompatible mods enabled; disabling...");
                OnDisabled();
            }
        }
    }
}
