﻿namespace EsotericIDE.Ndim
{
    abstract class NdimCommand
    {
        public Position Position { get; set; }
        public NdimCommand(Position position) { Position = position; }
        public abstract void Execute(NdimEnv ndim);
    }
}
