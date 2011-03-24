using System.Collections.Generic;
using System.Text;

namespace EsotericIDE
{
    abstract class ExecutionEnvironment
    {
        public IEnumerator<Position> InstructionPointer;
        public StringBuilder Output = new StringBuilder();

        public abstract string DescribeExecutionState();
    }
}
