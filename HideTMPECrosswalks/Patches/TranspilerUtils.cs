using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Harmony;

namespace HideTMPECrosswalks.Patches {
    public static class TranspilerUtils {
        public static void LogDebug(object message) {
#if DEBUG
            UnityEngine.Debug.Log(message);
#endif
        }
        public static bool IsSameInstruction(CodeInstruction a, CodeInstruction b, bool debug = false) {
            if (a.opcode == b.opcode) {
                if (a.operand == b.operand) {
                    return true;
                }

                // This special code is needed for some reason because the == operator doesn't work on System.Byte
                return (a.operand is byte aByte && b.operand is byte bByte && aByte == bByte);
            } else {
                return false;
            }
        }

        public static bool IsLdLoc(CodeInstruction instruction) {
            return (instruction.opcode == OpCodes.Ldloc_0 || instruction.opcode == OpCodes.Ldloc_1 ||
                    instruction.opcode == OpCodes.Ldloc_2 || instruction.opcode == OpCodes.Ldloc_3
                    || instruction.opcode == OpCodes.Ldloc_S || instruction.opcode == OpCodes.Ldloc
                );
        }

        public static bool IsStLoc(CodeInstruction instruction) {
            return (instruction.opcode == OpCodes.Stloc_0 || instruction.opcode == OpCodes.Stloc_1 ||
                    instruction.opcode == OpCodes.Stloc_2 || instruction.opcode == OpCodes.Stloc_3
                    || instruction.opcode == OpCodes.Stloc_S || instruction.opcode == OpCodes.Stloc
                );
        }

        /// <summary>
        /// Get the instruction to load the variable which is stored here.
        /// </summary>
        public static CodeInstruction BuildLdLocFromStLoc(CodeInstruction instruction) {
            if (instruction.opcode == OpCodes.Stloc_0) {
                return new CodeInstruction(OpCodes.Ldloc_0);
            } else if (instruction.opcode == OpCodes.Stloc_1) {
                return new CodeInstruction(OpCodes.Ldloc_1);
            } else if (instruction.opcode == OpCodes.Stloc_2) {
                return new CodeInstruction(OpCodes.Ldloc_2);
            } else if (instruction.opcode == OpCodes.Stloc_3) {
                return new CodeInstruction(OpCodes.Ldloc_3);
            } else if (instruction.opcode == OpCodes.Stloc_S) {
                return new CodeInstruction(OpCodes.Ldloc_S, instruction.operand);
            } else if (instruction.opcode == OpCodes.Stloc) {
                return new CodeInstruction(OpCodes.Ldloc, instruction.operand);
            } else {
                throw new Exception("Statement is not stloc!");
            }
        }

        public static CodeInstruction BuildLdLocFromLdLoc(CodeInstruction instruction) {
            if (instruction.opcode == OpCodes.Ldloc_0) {
                return new CodeInstruction(OpCodes.Ldloc_0);
            } else if (instruction.opcode == OpCodes.Ldloc_1) {
                return new CodeInstruction(OpCodes.Ldloc_1);
            } else if (instruction.opcode == OpCodes.Ldloc_2) {
                return new CodeInstruction(OpCodes.Ldloc_2);
            } else if (instruction.opcode == OpCodes.Ldloc_3) {
                return new CodeInstruction(OpCodes.Ldloc_3);
            } else if (instruction.opcode == OpCodes.Ldloc_S) {
                return new CodeInstruction(OpCodes.Ldloc_S, instruction.operand);
            } else if (instruction.opcode == OpCodes.Ldloc) {
                return new CodeInstruction(OpCodes.Ldloc, instruction.operand);
            } else {
                throw new Exception("Statement is not ldloc!");
            }
        }

        public static CodeInstruction BuildStLocFromLdLoc(CodeInstruction instruction) {
            if (instruction.opcode == OpCodes.Ldloc_0) {
                return new CodeInstruction(OpCodes.Stloc_0);
            } else if (instruction.opcode == OpCodes.Ldloc_1) {
                return new CodeInstruction(OpCodes.Stloc_1);
            } else if (instruction.opcode == OpCodes.Ldloc_2) {
                return new CodeInstruction(OpCodes.Stloc_2);
            } else if (instruction.opcode == OpCodes.Ldloc_3) {
                return new CodeInstruction(OpCodes.Stloc_3);
            } else if (instruction.opcode == OpCodes.Ldloc_S) {
                return new CodeInstruction(OpCodes.Stloc_S, instruction.operand);
            } else if (instruction.opcode == OpCodes.Ldloc) {
                return new CodeInstruction(OpCodes.Stloc, instruction.operand);
            } else {
                throw new Exception("Statement is not ldloc!");
            }
        }

        internal static string IL2STR(this IEnumerable<CodeInstruction> instructions) {
            string ret = "";
            foreach (var code in instructions) {
                ret += code + "\n";
            }
            return ret;
        }
    }
}
