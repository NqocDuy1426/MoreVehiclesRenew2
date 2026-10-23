using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ColossalFramework;
using ColossalFramework.IO;
using HarmonyLib;
using MoreVehiclesRenew2.Patching;
using MoreVehiclesRenew2.Utils;

namespace MoreVehiclesRenew2.Patches
{
    internal static class VehicleManagerPatch
    {
        private sealed class VehicleManager_Data_Deserialize : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(VehicleManager.Data).GetMethod("Deserialize", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(DataSerializer) }, null);
            }

            private static int FindLoadLocalInstructionForStoreLocal(List<CodeInstruction> instructions, int startIndex, CodeInstruction storeLocalInstruction, bool searchBackwards)
            {
                OpCode opcode = storeLocalInstruction.opcode;
                int loadLocalOperand = 0;
                OpCode loadLocalOp;
                if (opcode == OpCodes.Stloc_0)
                {
                    loadLocalOp = OpCodes.Ldloc_0;
                }
                else if (opcode == OpCodes.Stloc_1)
                {
                    loadLocalOp = OpCodes.Ldloc_1;
                }
                else if (opcode == OpCodes.Stloc_2)
                {
                    loadLocalOp = OpCodes.Ldloc_2;
                }
                else if (opcode == OpCodes.Stloc_3)
                {
                    loadLocalOp = OpCodes.Ldloc_3;
                }
                else if (opcode == OpCodes.Stloc_S)
                {
                    loadLocalOp = OpCodes.Ldloc_S;
                    loadLocalOperand = ((LocalBuilder)storeLocalInstruction.operand).LocalIndex;
                }
                else if (opcode == OpCodes.Stloc)
                {
                    loadLocalOp = OpCodes.Ldloc;
                    loadLocalOperand = ((LocalBuilder)storeLocalInstruction.operand).LocalIndex;
                }
                else
                {
                    Log.Error("Failed to customize VehicleManager: unexpected store local IL '" + opcode + "'");
                    return -1;
                }

                if (!searchBackwards)
                {
                    return instructions.FindIndex(startIndex, i => IsMatchingLoad(i, loadLocalOp, loadLocalOperand));
                }
                return instructions.FindLastIndex(startIndex, i => IsMatchingLoad(i, loadLocalOp, loadLocalOperand));
            }

            private static bool IsMatchingLoad(CodeInstruction i, OpCode loadLocalOp, int loadLocalOperand)
            {
                if (i.opcode == loadLocalOp)
                {
                    if (i.opcode == OpCodes.Ldloc_S || i.opcode == OpCodes.Ldloc)
                    {
                        return ((LocalBuilder)i.operand).LocalIndex == loadLocalOperand;
                    }
                    return true;
                }
                return false;
            }

            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> list = instructions.ToList();
                MethodInfo arraySizeGetter = AccessTools.Method(typeof(VehicleManager_Data_Deserialize), "GetMaxVehicleCount", null, null);
                MethodInfo arraySizeGetter2 = AccessTools.Method(typeof(VehicleManager_Data_Deserialize), "GetMaxParkedVehicleCount", null, null);
                if (!PatchCode(list, "m_vehicles", "m_buffer", arraySizeGetter, 1) || !PatchCode(list, "m_parkedVehicles", "m_buffer", arraySizeGetter2, 2))
                {
                    return instructions;
                }
                return list;
            }

            private static bool Prefix()
            {
                Vehicle[] buffer = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                Array.Clear(buffer, 0, buffer.Length);
                VehicleParked[] buffer2 = Singleton<VehicleManager>.instance.m_parkedVehicles.m_buffer;
                Array.Clear(buffer2, 0, buffer2.Length);
                return true;
            }

            private static bool PatchCode(List<CodeInstruction> instructionList, string targetArrayName, string bufferArrayName, MethodInfo arraySizeGetter, int fullScanCount)
            {
                int num = instructionList.FindIndex(i => i.opcode == OpCodes.Ldfld && i.operand != null && i.operand.ToString().Contains(targetArrayName));
                if (num < 0 || ++num >= instructionList.Count)
                {
                    Log.Error("Failed to customize VehicleManager: no IL instruction for '" + targetArrayName + "'");
                    return false;
                }
                CodeInstruction val = instructionList[num];
                if (val.opcode != OpCodes.Ldfld || val.operand == null || !val.operand.ToString().Contains(bufferArrayName) || ++num >= instructionList.Count)
                {
                    Log.Error("Failed to customize VehicleManager: no IL instruction for '" + bufferArrayName + "'");
                    return false;
                }
                val = instructionList[num];
                num = FindLoadLocalInstructionForStoreLocal(instructionList, num, val, false);
                if (num < 0 || ++num >= instructionList.Count || instructionList[num].opcode != OpCodes.Ldlen)
                {
                    Log.Error("Failed to customize VehicleManager: ldloc and ldlen IL instructions not found");
                    return false;
                }
                if (num + 2 >= instructionList.Count)
                {
                    Log.Error("Failed to customize VehicleManager: stloc IL instructions not found, method end reached");
                    return false;
                }
                val = instructionList[num + 1];
                if (val.opcode == OpCodes.Conv_I4)
                {
                    val = instructionList[num + 2];
                }
                OpCode opcode = val.opcode;
                if (opcode != OpCodes.Stloc && opcode != OpCodes.Stloc_0 && opcode != OpCodes.Stloc_1 && opcode != OpCodes.Stloc_2 && opcode != OpCodes.Stloc_3 && opcode != OpCodes.Stloc_S)
                {
                    Log.Error("Failed to customize VehicleManager: stloc IL instructions not found");
                    return false;
                }
                instructionList.RemoveAt(--num);
                instructionList[num] = new CodeInstruction(OpCodes.Call, arraySizeGetter);
                num = instructionList.Count - 1;
                while (fullScanCount-- > 0)
                {
                    num = FindLoadLocalInstructionForStoreLocal(instructionList, num, val, true);
                    if (num < 0)
                    {
                        Log.Error("Failed to customize VehicleManager: last ldloc IL instructions not found");
                        return false;
                    }
                    instructionList[num] = new CodeInstruction(OpCodes.Ldc_I4, Constants.ModdedMaxVehicleCount);
                }
                return true;
            }

            private static bool IsModEnabled()
            {
                var modOverride = Singleton<SimulationManager>.instance.m_metaData?.m_modOverride;
                if (modOverride == null) return false;

                foreach (var kvp in modOverride)
                {
                    if (kvp.Value && !string.IsNullOrEmpty(kvp.Key) && kvp.Key.IndexOf("More Vehicles", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
                return false;
            }

            private static int GetMaxVehicleCount()
            {
                if (!IsModEnabled())
                {
                    return Constants.VanillaMaxVehicleCount;
                }
                return Constants.ModdedMaxVehicleCount;
            }

            private static int GetMaxParkedVehicleCount()
            {
                if (!IsModEnabled())
                {
                    return Constants.VanillaMaxParkedVehicleCount;
                }
                return Constants.ModdedMaxParkedVehicleCount;
            }
        }

        private sealed class VehicleManager_UpdateData : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(VehicleManager).GetMethod("UpdateData", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(SimulationManager.UpdateMode) }, null);
            }

            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
            {
                return CodeProcessor.ReplaceOperands(
                    CodeProcessor.ReplaceOperands(instructions, Constants.VanillaMaxVehicleCount, Constants.ModdedMaxVehicleCount),
                    Constants.VanillaMaxParkedVehicleCount,
                    Constants.ModdedMaxParkedVehicleCount);
            }
        }

        private sealed class VehicleManager_AirlineModified : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(VehicleManager).GetMethod("AirlineModified", BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
            }

            private static bool Prefix(VehicleManager __instance)
            {
                for (int i = 0; i < __instance.m_vehicles.m_buffer.Length; i++)
                {
                    VehicleInfo info = __instance.m_vehicles.m_buffer[i].Info;
                    if (info != null)
                    {
                        PassengerPlaneAI planeAI = info.m_vehicleAI as PassengerPlaneAI;
                        if (planeAI != null)
                        {
                            planeAI.SetAirline((ushort)i, ref __instance.m_vehicles.m_buffer[i]);
                        }
                    }
                }
                return false;
            }
        }

        private sealed class VehicleManager_SimulationStepImpl : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(VehicleManager).GetMethod("SimulationStepImpl", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(int) }, null);
            }

            private static IEnumerable<CodeInstruction> Transform(IEnumerable<CodeInstruction> instructions)
            {
                // Vanilla 1024 vehicles per frame (16384 / 16) -> Modded 4096 vehicles per frame (65536 / 16)
                return CodeProcessor.ReplaceOperands(
                    CodeProcessor.ReplaceOperands(instructions, 1024, 4096),
                    Constants.VanillaMaxVehicleCount,
                    Constants.ModdedMaxVehicleCount);
            }

            private static void Postfix(int subStep)
            {
                if (subStep != 0)
                {
                    int frame = (int)(Singleton<SimulationManager>.instance.m_currentFrameIndex & 0xF);
                    MoreVehiclesRenew2.Breakthroughs.GhostVehicleAnnihilator.InspectAndCleanFrameSlice(frame * 4096, (frame + 1) * 4096);
                }
            }
        }

        public static IPatch DataDeserialize { get; } = new VehicleManager_Data_Deserialize();
        public static IPatch UpdateData { get; } = new VehicleManager_UpdateData();
        public static IPatch AirlineModified { get; } = new VehicleManager_AirlineModified();
        public static IPatch SimulationStepImpl { get; } = new VehicleManager_SimulationStepImpl();
    }
}
