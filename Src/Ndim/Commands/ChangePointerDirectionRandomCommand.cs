using RT.Util;

namespace EsotericIDE.Ndim
{
    class ChangePointerDirectionRandomCommand : NdimCommand
    {
        public ChangePointerDirectionRandomCommand(Position position) : base(position) { }

        public override void Execute(NdimEnv ndim)
        {
            var newDirection = Rnd.Next(0, ndim.Dimensions);
            if (Rnd.Next(0, 2) != 0)
                newDirection *= -1;
            ndim.Pointer.SetDirection(newDirection);
        }
        public override string ToString() => "?";
    }
}
