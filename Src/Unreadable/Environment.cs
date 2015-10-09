using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Unreadable
{
    sealed class UnreadableEnv : ExecutionEnvironment
    {
        private Program _program;
        private string _input;
        private Stack<BigInteger?> _stack;
        private Dictionary<BigInteger, BigInteger> _variables;

        public UnreadableEnv(string source, string input)
        {
            _program = Program.Parse(source);
            _input = input;
            _stack = new Stack<BigInteger?>();
            _variables = new Dictionary<BigInteger, BigInteger>();
        }

        public override void UpdateWatch()
        {
            _txtWatch.Text = "Stack:\r\n{0}\r\n\r\nVariables:\r\n{1}".Fmt(
                _stack.Select(val => val == null ? "<void>" : val.Value.ToString()).DefaultIfEmpty("<empty>").JoinString("\r\n"),
                _variables.Select(kvp => "#{0} = {1}".Fmt(kvp.Key, kvp.Value)).DefaultIfEmpty("<none>").JoinString("\r\n")
            );
        }

        protected override IEnumerable<Position> GetProgram()
        {
            return _program.Execute(this);
        }

        public BigInteger? Pop() { return _stack.Pop(); }
        public void Push(BigInteger? value) { _stack.Push(value); }

        private BigInteger popNotNull(string errorMessage)
        {
            var value = _stack.Pop();
            if (value == null)
                throw new Exception(errorMessage);
            return value.Value;
        }

        public void Print()
        {
            var value = popNotNull("Cannot print the result of a void operation.");
            _output.Append(char.ConvertFromUtf32((int) value));
            _stack.Push(value);
        }

        public void One() { _stack.Push(1); }
        public void Inc() { _stack.Push(popNotNull("Cannot increment a void value.") + 1); }
        public void Dec() { _stack.Push(popNotNull("Cannot decrement a void value.") - 1); }

        public void Sequence()
        {
            var value = _stack.Pop();
            _stack.Pop();
            _stack.Push(value);
        }

        public void Set()
        {
            var value = popNotNull("Cannot set a variable to a void value.");
            var variable = popNotNull("Void is not a valid variable identifier.");
            _variables[variable] = value;
            _stack.Push(value);
        }

        public void Get()
        {
            _stack.Push(_variables.Get(popNotNull("Void is not a valid variable identifier."), 0));
        }

        public void Read()
        {
            if (_input.Length == 0)
                _stack.Push(-1);
            else
            {
                _stack.Push(char.ConvertToUtf32(_input, 0));
                _input = _input.Substring(char.IsSurrogate(_input[0]) ? 2 : 1);
            }
        }
    }
}
