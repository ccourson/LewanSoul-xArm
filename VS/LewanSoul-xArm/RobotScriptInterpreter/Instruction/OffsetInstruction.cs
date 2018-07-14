namespace Instruction
{
    internal class OffsetInstruction : IInstruction
    {
        private string[] tokens;

        public OffsetInstruction(string[] tokens)
        {
            this.tokens = tokens;
        }

        public void DoAction()
        {
            throw new System.NotImplementedException();
        }
    }
}