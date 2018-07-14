using System;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace RobotScriptInterpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            Debug.WriteLine("Begin.");

            var script = new StringBuilder();
            script.AppendLine("m s6 +100!");

            var objectCode = Compile(script.ToString());

            if (!objectCode.HasErrors)
            {
                Execute(objectCode);
            }
        }

        private static void Execute(ObjectCode objectCode)
        {
            List<Instruction.IInstruction> instructions = objectCode.Instructions;
            instructions.ForEach(e => e.DoAction());
        }

        private static ObjectCode Compile(string source)
        {
            var objectCode = new ObjectCode();
            var lines = source.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (line.Equals(string.Empty) || line.StartsWith('#')) continue;

                string[] tokens = line.Split(new[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

                switch (tokens[0])
                {
                    case "m":
                    case "move":
                        objectCode.Instructions.Add(new Instruction.MoveInstruction(tokens));
                        break;
                    case "o":
                    case "offset":
                        objectCode.Instructions.Add(new Instruction.OffsetInstruction(tokens));
                        break;
                    default:
                        throw new Exception($"Undefined command in line {i + 1}.");
                }
            }

            return objectCode;
        }       
    }
}
