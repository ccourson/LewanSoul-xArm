using RobotScriptInterpreter;

namespace Instruction
{
    internal class MoveInstruction : IInstruction
    {
        private string[] tokens;

        public MoveInstruction(string[] tokens)
        {
            this.tokens = tokens;
        }

        public void DoAction()
        {
            throw new System.NotImplementedException();
        }
    }
}