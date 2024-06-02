using System;
using System.Linq;

namespace EsotericIDE.Ndim
{
    public readonly struct Coordinate : IEquatable<Coordinate>
    {
        private readonly int[] _vector;

        public Coordinate(int[] vector)
        {
            _vector = vector;
        }

        public int[] Vector => _vector.ToArray();

        public override bool Equals(object obj) => obj is Coordinate co && Enumerable.SequenceEqual(_vector, co._vector);
        public bool Equals(Coordinate other) => Enumerable.SequenceEqual(_vector, other._vector);
        public static bool operator ==(Coordinate one, Coordinate two) => one.Equals(two);
        public static bool operator !=(Coordinate one, Coordinate two) => !one.Equals(two);

        public Coordinate WithValue(int axis, int coordinate)
        {
            var newVector = _vector.ToArray();
            newVector[axis] = coordinate;
            return new Coordinate(newVector);
        }

        public override int GetHashCode()
        {
            var hashCode = 1718686639;
            foreach (var val in _vector)
                hashCode = hashCode * -1521134295 + val;
            hashCode = hashCode * -1521134295 + _vector.Length;
            return hashCode;
        }

        public override string ToString()
        {
            return string.Join(", ", _vector);
        }
    }
}