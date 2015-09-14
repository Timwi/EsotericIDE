using System;

namespace EsotericIDE.Hexagony
{
    abstract class Direction : IEquatable<Direction>
    {
        public static readonly Direction East = new East();
        public static readonly Direction SouthEast = new SouthEast();
        public static readonly Direction SouthWest = new SouthWest();
        public static readonly Direction West = new West();
        public static readonly Direction NorthWest = new NorthWest();
        public static readonly Direction NorthEast = new NorthEast();

        public abstract Direction TurnRight { get; }
        public abstract Direction TurnLeft { get; }

        public abstract Direction ReflectAtSlash { get; }
        public abstract Direction ReflectAtBackslash { get; }
        public abstract Direction ReflectAtUnderscore { get; }
        public abstract Direction ReflectAtPipe { get; }
        public abstract Direction ReflectAtLessThan(bool positive);
        public abstract Direction ReflectAtGreaterThan(bool positive);

        public abstract Direction Reverse { get; }
        public abstract PointAxial Vector { get; }

        public bool Equals(Direction other)
        {
            // The hash code of all the directions are unique
            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            // The hash code of all the directions are unique
            return obj is Direction && GetHashCode() == obj.GetHashCode();
        }

        public static bool operator ==(Direction a, Direction b) { return a.Equals(b); }
        public static bool operator !=(Direction a, Direction b) { return !a.Equals(b); }
        public abstract override int GetHashCode();
    }

    sealed class NorthEast : Direction
    {
        public override Direction TurnRight { get { return Direction.East; } }
        public override Direction TurnLeft { get { return Direction.NorthWest; } }

        public override Direction ReflectAtSlash { get { return Direction.NorthEast; } }
        public override Direction ReflectAtBackslash { get { return Direction.West; } }
        public override Direction ReflectAtUnderscore { get { return Direction.SouthEast; } }
        public override Direction ReflectAtPipe { get { return Direction.NorthWest; } }
        public override Direction ReflectAtLessThan(bool positive) { return Direction.SouthWest; }
        public override Direction ReflectAtGreaterThan(bool positive) { return Direction.East; }

        public override Direction Reverse { get { return Direction.SouthWest; } }
        public override PointAxial Vector { get { return new PointAxial(1, -1); } }
        public override int GetHashCode() { return 245; }
    }

    sealed class NorthWest : Direction
    {
        public override Direction TurnRight { get { return Direction.NorthEast; } }
        public override Direction TurnLeft { get { return Direction.West; } }

        public override Direction ReflectAtSlash { get { return Direction.East; } }
        public override Direction ReflectAtBackslash { get { return Direction.NorthWest; } }
        public override Direction ReflectAtUnderscore { get { return Direction.SouthWest; } }
        public override Direction ReflectAtPipe { get { return Direction.NorthEast; } }
        public override Direction ReflectAtLessThan(bool positive) { return Direction.West; }
        public override Direction ReflectAtGreaterThan(bool positive) { return Direction.SouthEast; }

        public override Direction Reverse { get { return Direction.SouthEast; } }
        public override PointAxial Vector { get { return new PointAxial(0, -1); } }
        public override int GetHashCode() { return 2456; }
    }

    sealed class West : Direction
    {
        public override Direction TurnRight { get { return Direction.NorthWest; } }
        public override Direction TurnLeft { get { return Direction.SouthWest; } }

        public override Direction ReflectAtSlash { get { return Direction.SouthEast; } }
        public override Direction ReflectAtBackslash { get { return Direction.NorthEast; } }
        public override Direction ReflectAtUnderscore { get { return Direction.West; } }
        public override Direction ReflectAtPipe { get { return Direction.East; } }
        public override Direction ReflectAtLessThan(bool positive) { return Direction.East; }
        public override Direction ReflectAtGreaterThan(bool positive) { return positive ? Direction.NorthWest : Direction.SouthWest; }

        public override Direction Reverse { get { return Direction.East; } }
        public override PointAxial Vector { get { return new PointAxial(-1, 0); } }
        public override int GetHashCode() { return 24567; }
    }

    sealed class SouthWest : Direction
    {
        public override Direction TurnRight { get { return Direction.West; } }
        public override Direction TurnLeft { get { return Direction.SouthEast; } }

        public override Direction ReflectAtSlash { get { return Direction.SouthWest; } }
        public override Direction ReflectAtBackslash { get { return Direction.East; } }
        public override Direction ReflectAtUnderscore { get { return Direction.NorthWest; } }
        public override Direction ReflectAtPipe { get { return Direction.SouthEast; } }
        public override Direction ReflectAtLessThan(bool positive) { return Direction.West; }
        public override Direction ReflectAtGreaterThan(bool positive) { return Direction.NorthEast; }

        public override Direction Reverse { get { return Direction.NorthEast; } }
        public override PointAxial Vector { get { return new PointAxial(-1, 1); } }
        public override int GetHashCode() { return 245678; }
    }

    sealed class SouthEast : Direction
    {
        public override Direction TurnRight { get { return Direction.SouthWest; } }
        public override Direction TurnLeft { get { return Direction.East; } }

        public override Direction ReflectAtSlash { get { return Direction.West; } }
        public override Direction ReflectAtBackslash { get { return Direction.SouthEast; } }
        public override Direction ReflectAtUnderscore { get { return Direction.NorthEast; } }
        public override Direction ReflectAtPipe { get { return Direction.SouthWest; } }
        public override Direction ReflectAtLessThan(bool positive) { return Direction.NorthWest; }
        public override Direction ReflectAtGreaterThan(bool positive) { return Direction.East; }

        public override Direction Reverse { get { return Direction.NorthWest; } }
        public override PointAxial Vector { get { return new PointAxial(0, 1); } }
        public override int GetHashCode() { return 2456783; }
    }

    sealed class East : Direction
    {
        public override Direction TurnRight { get { return Direction.SouthEast; } }
        public override Direction TurnLeft { get { return Direction.NorthEast; } }

        public override Direction ReflectAtSlash { get { return Direction.NorthWest; } }
        public override Direction ReflectAtBackslash { get { return Direction.SouthWest; } }
        public override Direction ReflectAtUnderscore { get { return Direction.East; } }
        public override Direction ReflectAtPipe { get { return Direction.West; } }
        public override Direction ReflectAtLessThan(bool positive) { return positive ? Direction.SouthEast : Direction.NorthEast; }
        public override Direction ReflectAtGreaterThan(bool positive) { return Direction.West; }

        public override Direction Reverse { get { return Direction.West; } }
        public override PointAxial Vector { get { return new PointAxial(1, 0); } }
        public override int GetHashCode() { return 24567837; }
    }
}