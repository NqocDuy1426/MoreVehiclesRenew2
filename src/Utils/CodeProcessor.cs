using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace MoreVehiclesRenew2.Utils
{
    internal static class CodeProcessor
    {
        public static IEnumerable<CodeInstruction> ReplaceOperands(IEnumerable<CodeInstruction> instructions, int oldValue, int newValue)
        {
            if (instructions == null)
            {
                throw new ArgumentNullException("instructions");
            }
            return ReplaceOperandsCore(instructions, oldValue, newValue);
        }

        private static IEnumerable<CodeInstruction> ReplaceOperandsCore(IEnumerable<CodeInstruction> instructions, int oldValue, int newValue)
        {
            foreach (CodeInstruction current in instructions)
            {
                if (current.opcode.OperandType == OperandType.InlineI && current.operand is int num && num == oldValue)
                {
                    current.operand = newValue;
                }
                else if (current.opcode.OperandType == OperandType.ShortInlineI && current.operand is sbyte b && b == oldValue)
                {
                    if (newValue >= -128 && newValue <= 127)
                    {
                        current.operand = (sbyte)newValue;
                    }
                    else
                    {
                        current.opcode = OpCodes.Ldc_I4;
                        current.operand = newValue;
                    }
                }
                else if (current.operand == null && GetInlineConstant(current.opcode) == oldValue)
                {
                    if (newValue >= -128 && newValue <= 127)
                    {
                        current.opcode = OpCodes.Ldc_I4_S;
                        current.operand = (sbyte)newValue;
                    }
                    else
                    {
                        current.opcode = OpCodes.Ldc_I4;
                        current.operand = newValue;
                    }
                }
                yield return current;
            }
        }

        private static int GetInlineConstant(OpCode opcode)
        {
            if (opcode == OpCodes.Ldc_I4_0) return 0;
            if (opcode == OpCodes.Ldc_I4_1) return 1;
            if (opcode == OpCodes.Ldc_I4_2) return 2;
            if (opcode == OpCodes.Ldc_I4_3) return 3;
            if (opcode == OpCodes.Ldc_I4_4) return 4;
            if (opcode == OpCodes.Ldc_I4_5) return 5;
            if (opcode == OpCodes.Ldc_I4_6) return 6;
            if (opcode == OpCodes.Ldc_I4_7) return 7;
            if (opcode == OpCodes.Ldc_I4_8) return 8;
            if (opcode == OpCodes.Ldc_I4_M1) return -1;
            return int.MinValue;
        }
    }
}
