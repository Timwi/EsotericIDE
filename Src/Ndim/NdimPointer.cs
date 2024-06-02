using System;

namespace EsotericIDE.Ndim
{
    public class NdimPointer
    {
        public int Direction { get; private set; }
        public Coordinate Position { get; private set; }
        private readonly int _dimensions;

        public NdimPointer(int dimensions)
        {
            _dimensions = dimensions;
            Direction = 1;
            Position = new Coordinate(new int[dimensions]);
        }

        public void SetDirection(int dir)
        {
            Direction = dir;
        }

        public void MoveNext()
        {
            int step = Direction < 1 ? -1 : 1;
            var vec = Position.Vector;
            vec[Math.Abs(Direction) - 1] += step;
            Position = new Coordinate(vec);
        }

        public Coordinate GetLeft()
        {
            var vec = Position.Vector;
            vec[Math.Abs(Direction) % _dimensions] -= 1;
            return new Coordinate(vec);
        }

        public Coordinate GetRight()
        {
            var vec = Position.Vector;
            vec[Math.Abs(Direction) % _dimensions] += 1;
            return new Coordinate(vec);
        }

        public void TurnLeft()
        {
            Direction = -((Math.Abs(Direction) % _dimensions) + 1);
        }

        public void TurnRight()
        {
            Direction = (Math.Abs(Direction) % _dimensions) + 1;
        }

        public void SetPosition(Coordinate position)
        {
            Position = position;
        }
    }
}
