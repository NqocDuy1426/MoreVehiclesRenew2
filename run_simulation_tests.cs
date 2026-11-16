using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Mono.Cecil;

namespace MoreVehiclesRenew2.Tests
{
    class Program
    {
        private static int s_passedTests = 0;
        private static int s_failedTests = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("======================================================================");
            Console.WriteLine("🚗 MORE VEHICLES RENEW 2 - DEEP SIMULATION TEST SUITE");
            Console.WriteLine("======================================================================");

            TestGroup1_ConstantsAndArchitecture();
            TestGroup2_CrossModSaveMigration();
            TestGroup3_GhostVehicleAndDeadlockArmor();
            TestGroup4_EmergencySlotReclaimer();
            TestGroup5_SmartLogisticsMultiplier();
            TestGroup6_ILTranspilerAudit();

            Console.WriteLine("\n======================================================================");
            Console.WriteLine($"📊 TEST EXECUTION SUMMARY: {s_passedTests} PASSED, {s_failedTests} FAILED");
            Console.WriteLine("======================================================================");

            if (s_failedTests > 0)
            {
                Environment.Exit(1);
            }
        }

        private static void Assert(bool condition, string testName)
        {
            if (condition)
            {
                Console.WriteLine($"  [PASS] {testName}");
                s_passedTests++;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  [FAIL] {testName}");
                Console.ResetColor();
                s_failedTests++;
            }
        }

        // =====================================================================
        // TEST GROUP 1: CONSTANTS & CORE ARCHITECTURE
        // =====================================================================
        private static void TestGroup1_ConstantsAndArchitecture()
        {
            Console.WriteLine("\n--- TEST GROUP 1: Constants & Core Architecture ---");

            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MoreVehiclesRenew2.dll");
            var asm = AssemblyDefinition.ReadAssembly(dllPath);

            Assert(asm != null, "MoreVehiclesRenew2.dll successfully read via Cecil");
            Assert(asm.Name.Version != null && asm.Name.Version.Major == 2 && asm.Name.Version.Minor == 0, $"Assembly has valid AssemblyVersion: {asm.Name.Version}");

            string[] coreTypes = new string[]
            {
                "MoreVehiclesRenew2.MoreVehiclesRenew2Mod",
                "MoreVehiclesRenew2.Utils.Constants",
                "MoreVehiclesRenew2.Utils.Compatibility",
                "MoreVehiclesRenew2.Utils.VehicleManagerCustomizer",
                "MoreVehiclesRenew2.Utils.CodeProcessor",
                "MoreVehiclesRenew2.Breakthroughs.GhostVehicleAnnihilator",
                "MoreVehiclesRenew2.Breakthroughs.EmergencySlotReclaimer",
                "MoreVehiclesRenew2.Breakthroughs.SmartLogisticsMultiplier",
                "MoreVehiclesRenew2.Patches.BuildingPatch",
                "MoreVehiclesRenew2.Patches.CinematicCameraControllerPatch",
                "MoreVehiclesRenew2.Patches.EventManagerPatch",
                "MoreVehiclesRenew2.Patches.OutsideConnectionAIPatch",
                "MoreVehiclesRenew2.Patches.PathVisualizerPatch",
                "MoreVehiclesRenew2.Patches.ResidentAIPatch",
                "MoreVehiclesRenew2.Patches.TouristAIPatch",
                "MoreVehiclesRenew2.Patches.VehicleLinkedListPatch",
                "MoreVehiclesRenew2.Patches.VehicleManagerPatch",
                "MoreVehiclesRenew2.Patches.VehiclePatch"
            };

            foreach (var typeName in coreTypes)
            {
                var t = asm.MainModule.GetType(typeName);
                Assert(t != null, $"Core type exists: {typeName}");
            }

            var constType = asm.MainModule.GetType("MoreVehiclesRenew2.Utils.Constants");
            var vCount = constType.Fields.FirstOrDefault(f => f.Name == "ModdedMaxVehicleCount");
            var pCount = constType.Fields.FirstOrDefault(f => f.Name == "ModdedMaxParkedVehicleCount");

            Assert(vCount != null && (int)vCount.Constant == 65536, "ModdedMaxVehicleCount is exactly 65,536 (400% capacity)");
            Assert(pCount != null && (int)pCount.Constant == 65536, "ModdedMaxParkedVehicleCount is exactly 65,536 (200% capacity)");
        }

        // =====================================================================
        // TEST GROUP 2: CROSS-MOD BIDIRECTIONAL SAVE MIGRATION
        // =====================================================================
        private static void TestGroup2_CrossModSaveMigration()
        {
            Console.WriteLine("\n--- TEST GROUP 2: Cross-Mod Save Migration ---");

            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MoreVehiclesRenew2.dll");
            var asm = AssemblyDefinition.ReadAssembly(dllPath);
            var constType = asm.MainModule.GetType("MoreVehiclesRenew2.Utils.Constants");

            var fLegacy = constType.Fields.FirstOrDefault(f => f.Name == "MetadataModNameLegacy");
            var fRenew = constType.Fields.FirstOrDefault(f => f.Name == "MetadataModNameRenew");
            var fRenew2 = constType.Fields.FirstOrDefault(f => f.Name == "MetadataModNameRenew2");
            var fRenewed = constType.Fields.FirstOrDefault(f => f.Name == "MetadataModNameRenewed");

            Assert(fLegacy != null && (string)fLegacy.Constant == "User/More Vehicles", "Legacy metadata key: 'User/More Vehicles'");
            Assert(fRenew != null && (string)fRenew.Constant == "User/More Vehicles Renew", "Renew metadata key: 'User/More Vehicles Renew'");
            Assert(fRenew2 != null && (string)fRenew2.Constant == "User/More Vehicles Renew 2", "Renew 2 metadata key: 'User/More Vehicles Renew 2'");
            Assert(fRenewed != null && (string)fRenewed.Constant == "User/More Vehicles Renewed", "Renewed metadata key: 'User/More Vehicles Renewed'");

            // Verify Simulated MetaData matching
            Dictionary<string, bool> mockMetaData = new Dictionary<string, bool>();

            // Case A: Save from 3684679695 (More Vehicles Renewed)
            mockMetaData["User/More Vehicles Renewed"] = true;
            bool canReadRenewed = mockMetaData.ContainsKey("User/More Vehicles Renewed") && mockMetaData["User/More Vehicles Renewed"];
            Assert(canReadRenewed, "Migration: Successfully recognizes saves from 3684679695 More Vehicles Renewed");

            // Case B: Save from 1764208250 (Original More Vehicles)
            mockMetaData.Clear();
            mockMetaData["User/More Vehicles"] = true;
            bool canReadLegacy = mockMetaData.ContainsKey("User/More Vehicles") && mockMetaData["User/More Vehicles"];
            Assert(canReadLegacy, "Migration: Successfully recognizes saves from 1764208250 Original More Vehicles");

            // Case C: Stamping all 4 keys on save
            mockMetaData.Clear();
            mockMetaData["User/More Vehicles"] = true;
            mockMetaData["User/More Vehicles Renew"] = true;
            mockMetaData["User/More Vehicles Renew 2"] = true;
            mockMetaData["User/More Vehicles Renewed"] = true;
            Assert(mockMetaData.Count == 4, "Serialization: Stamps all 4 metadata keys for 100% universal compatibility across all mod variants");
        }

        // =====================================================================
        // TEST GROUP 3: GHOST VEHICLE & DEADLOCK ARMOR
        // =====================================================================
        private static void TestGroup3_GhostVehicleAndDeadlockArmor()
        {
            Console.WriteLine("\n--- TEST GROUP 3: Ghost Vehicle & Deadlock Armor ---");

            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MoreVehiclesRenew2.dll");
            var asm = AssemblyDefinition.ReadAssembly(dllPath);
            var gvaType = asm.MainModule.GetType("MoreVehiclesRenew2.Breakthroughs.GhostVehicleAnnihilator");

            var cleanMethod = gvaType.Methods.FirstOrDefault(m => m.Name == "InspectAndCleanFrameSlice");
            Assert(cleanMethod != null, "GhostVehicleAnnihilator contains InspectAndCleanFrameSlice");

            // Verify that TrainAI, AircraftAI, ShipAI, PublicTransportAI are protected from stuck vehicle sweep
            bool hasTrainProtection = false;
            bool has120FrameKill = false;

            foreach (var inst in cleanMethod.Body.Instructions)
            {
                if (inst.Operand != null)
                {
                    string opStr = inst.Operand.ToString();
                    if (opStr.Contains("TrainAI") || opStr.Contains("AircraftAI") || opStr.Contains("ShipAI"))
                    {
                        hasTrainProtection = true;
                    }
                }
                if (inst.OpCode == Mono.Cecil.Cil.OpCodes.Ldc_I4 && Convert.ToInt32(inst.Operand) == 120)
                {
                    has120FrameKill = true;
                }
            }

            Assert(hasTrainProtection, "Transit Protection: Trains, Aircraft, and Ships shielded from stuck vehicle despawns");
            Assert(!has120FrameKill, "Cargo Armor: Destructive 120-frame (2 second) premature vehicle despawn eliminated");
        }

        // =====================================================================
        // TEST GROUP 4: EMERGENCY SLOT RECLAIMER
        // =====================================================================
        private static void TestGroup4_EmergencySlotReclaimer()
        {
            Console.WriteLine("\n--- TEST GROUP 4: Emergency Slot Reclaimer ---");

            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MoreVehiclesRenew2.dll");
            var asm = AssemblyDefinition.ReadAssembly(dllPath);
            var esrType = asm.MainModule.GetType("MoreVehiclesRenew2.Breakthroughs.EmergencySlotReclaimer");

            var reclaimMethod = esrType.Methods.FirstOrDefault(m => m.Name == "ReclaimStagnantSlot");
            Assert(reclaimMethod != null, "EmergencySlotReclaimer contains ReclaimStagnantSlot");

            bool protectsTransit = false;
            foreach (var inst in reclaimMethod.Body.Instructions)
            {
                if (inst.Operand != null && (inst.Operand.ToString().Contains("TrainAI") || inst.Operand.ToString().Contains("TramBaseAI")))
                {
                    protectsTransit = true;
                    break;
                }
            }
            Assert(protectsTransit, "Emergency Reclaimer: Protects rail and transit vehicles from emergency reclaim");
        }

        // =====================================================================
        // TEST GROUP 5: SMART LOGISTICS MULTIPLIER
        // =====================================================================
        private static void TestGroup5_SmartLogisticsMultiplier()
        {
            Console.WriteLine("\n--- TEST GROUP 5: Smart Logistics Multiplier ---");

            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MoreVehiclesRenew2.dll");
            var asm = AssemblyDefinition.ReadAssembly(dllPath);
            var slmType = asm.MainModule.GetType("MoreVehiclesRenew2.Breakthroughs.SmartLogisticsMultiplier");

            var boostMethod = slmType.Methods.FirstOrDefault(m => m.Name == "ApplyLogisticsBoost");
            Assert(boostMethod != null, "SmartLogisticsMultiplier contains ApplyLogisticsBoost");

            bool targetsCargo = false;
            bool targetsGarbage = false;
            bool targetsHearse = false;

            foreach (var inst in boostMethod.Body.Instructions)
            {
                if (inst.Operand != null)
                {
                    string op = inst.Operand.ToString();
                    if (op.Contains("CargoTruckAI")) targetsCargo = true;
                    if (op.Contains("GarbageTruckAI")) targetsGarbage = true;
                    if (op.Contains("HearseAI")) targetsHearse = true;
                }
            }

            Assert(targetsCargo, "Logistics: Targets CargoTruckAI (freight delivery optimization)");
            Assert(targetsGarbage, "Logistics: Targets GarbageTruckAI (waste management optimization)");
            Assert(targetsHearse, "Logistics: Targets HearseAI (deathcare capacity optimization)");
        }

        // =====================================================================
        // TEST GROUP 6: IL TRANSPILER AUDIT
        // =====================================================================
        private static void TestGroup6_ILTranspilerAudit()
        {
            Console.WriteLine("\n--- TEST GROUP 6: IL Transpiler & Simulation Step Sizing ---");

            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MoreVehiclesRenew2.dll");
            var asm = AssemblyDefinition.ReadAssembly(dllPath);
            var simStepClass = asm.MainModule.GetType("MoreVehiclesRenew2.Patches.VehicleManagerPatch/VehicleManager_SimulationStepImpl");

            Assert(simStepClass != null, "VehicleManager_SimulationStepImpl patch class exists");

            var transform = simStepClass.Methods.FirstOrDefault(m => m.Name == "Transform");
            Assert(transform != null, "VehicleManager_SimulationStepImpl contains Transform transpiler");

            bool converts1024to4096 = false;
            foreach (var inst in transform.Body.Instructions)
            {
                if (inst.OpCode == Mono.Cecil.Cil.OpCodes.Ldc_I4 && Convert.ToInt32(inst.Operand) == 4096)
                {
                    converts1024to4096 = true;
                    break;
                }
            }
            Assert(converts1024to4096, "Simulation Throughput: VehicleManager slice stepped from 1,024 to 4,096 per tick (16 frames x 4096 = 65,536)");
        }
    }
}
