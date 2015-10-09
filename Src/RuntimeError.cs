using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EsotericIDE
{
    sealed class RuntimeError
    {
        public Position Position { get; private set; }
        public string Message { get; private set; }
        public RuntimeError(Position position, string message = null)
        {
            Position = position;
            Message = message;
        }
    }
}
