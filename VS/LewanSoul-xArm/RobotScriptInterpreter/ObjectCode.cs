using System.Collections.Generic;

namespace RobotScriptInterpreter
{
    class ObjectCode
    {
        internal List<Instruction.IInstruction> Instructions = new List<Instruction.IInstruction>();
        internal bool HasErrors { get; set; }
        internal int ErrorLine { get; set; }
    }
}