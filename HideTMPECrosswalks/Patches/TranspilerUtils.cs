using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Harmony;

namespace HideCrosswalks.Patches {
    using System.Reflection;
    using Utils;
    public static class TranspilerUtils {
        static void Log(object message) {
            Extensions.Log("TRANSPILER " + message);
        }

        public static List<CodeInstruction> ToCodeList(IEnumerable<CodeInstruction> instructions) {
            var originalCodes = new List<CodeInstruction>(instructions);
            var codes = new List<CodeInstruction>(originalCodes);
            return codes;
        }

        public static CodeInstruction GetLDArg(MethodInfo method, string name) {
            byte idx = (byte)(GetParameterLoc(method, name) + 1);
            return new CodeInstruction(OpCodes.Ldarg_S, idx);
        }

        /// <summary>
        /// Post condtion: add one to get argument location
        /// </summary>
        public static byte GetParameterLoc(MethodInfo method, string name) {
            var parameters = method.GetParameters();
            for (byte i = 0; i < parameters.Length; ++i) {
                if (parameters[i].Name == name) {
                    return i;
                }
            }
            throw new Exception($"did not found parameter with name:<{name}>");
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

        public static int SearchInstruction(List<CodeInstruction> codes, CodeInstruction instruction, int index, int dir = +1, int counter = 1) {
            int count = 0;
            for (; index < codes.Count; index += dir) {
                if (TranspilerUtils.IsSameInstruction(codes[index], instruction)) {
                    if (++count == counter)
                        break;
                }
            }
            if (index >= codes.Count || count != counter) {
                throw new Exception(" Did not found instruction: " + instruction);
            }
            Log("Found : \n" + new[] { codes[index], codes[index + 1] }.IL2STR());
            return index;
        }

        /// <summary>
        /// replaces one instruction at the given index with multiple instrutions
        /// </summary>
        public static void ReplaceInstructions(List<CodeInstruction> codes, CodeInstruction[] insertion, int index) {
            foreach (var code in insertion)
                if (code == null)
                    throw new Exception("Bad Instructions:\n" + insertion.IL2STR());
            Log($"replacing <{codes[index]}>\nInsert between: <{codes[index - 1]}>  and  <{codes[index + 1]}>");

            codes.RemoveAt(index);
            codes.InsertRange(index, insertion);

            Log("Replacing with\n" + insertion.IL2STR());
            Log("PEEK (RESULTING CODE):\n" + codes.GetRange(index - 4, 14).IL2STR());

        }

        public static void InsertInstructions(List<CodeInstruction> codes, CodeInstruction[] insertion, int index) {
            foreach (var code in insertion)
                if (code == null)
                    throw new Exception("Bad Instructions:\n" + insertion.IL2STR());
            Log($"Insert point:\n between: <{codes[index - 1]}>  and  <{codes[index]}>");

            codes.InsertRange(index, insertion);

            Log("\n" + insertion.IL2STR());
            Log("PEEK:\n" + codes.GetRange(index - 4, 14).IL2STR());
        }
    }
}
