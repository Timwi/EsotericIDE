using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Hexagony
{
    abstract class Direction : IEquatable<Direction>
    {
        public abstract Direction right { get; }
        public abstract Direction left { get; }

        public abstract Direction reflect_diag_up { get; }
        public abstract Direction reflect_diag_down { get; }
        public abstract Direction reflect_hori { get; }
        public abstract Direction reflect_vert { get; }
        public abstract Direction reflect_branch_left(bool right);
        public abstract Direction reflect_branch_right(bool right);

        public abstract Direction reverse { get; }
        public abstract PointAxial vec { get; }

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

        public static readonly Direction East = new East();
        public static readonly Direction SouthEast = new SouthEast();
        public static readonly Direction SouthWest = new SouthWest();
        public static readonly Direction West = new West();
        public static readonly Direction NorthWest = new NorthWest();
        public static readonly Direction NorthEast = new NorthEast();
    }

    sealed class NorthEast : Direction
    {
        public override Direction right { get { return Direction.East; } }
        public override Direction left { get { return Direction.NorthWest; } }

        public override Direction reflect_diag_up { get { return Direction.NorthEast; } }
        public override Direction reflect_diag_down { get { return Direction.West; } }
        public override Direction reflect_hori { get { return Direction.SouthEast; } }
        public override Direction reflect_vert { get { return Direction.NorthWest; } }
        public override Direction reflect_branch_left(bool right) { return Direction.SouthWest; }
        public override Direction reflect_branch_right(bool right) { return Direction.East; }

        public override Direction reverse { get { return Direction.SouthWest; } }
        public override PointAxial vec { get { return new PointAxial(1, -1); } }
        public override int GetHashCode() { return 245; }
    }

    sealed class NorthWest : Direction
    {
        public override Direction right { get { return Direction.NorthEast; } }
        public override Direction left { get { return Direction.West; } }

        public override Direction reflect_diag_up { get { return Direction.East; } }
        public override Direction reflect_diag_down { get { return Direction.NorthWest; } }
        public override Direction reflect_hori { get { return Direction.SouthWest; } }
        public override Direction reflect_vert { get { return Direction.NorthEast; } }
        public override Direction reflect_branch_left(bool right) { return Direction.West; }
        public override Direction reflect_branch_right(bool right) { return Direction.SouthEast; }

        public override Direction reverse { get { return Direction.SouthEast; } }
        public override PointAxial vec { get { return new PointAxial(0, -1); } }
        public override int GetHashCode() { return 2456; }
    }

    sealed class West : Direction
    {
        public override Direction right { get { return Direction.NorthWest; } }
        public override Direction left { get { return Direction.SouthWest; } }

        public override Direction reflect_diag_up { get { return Direction.SouthEast; } }
        public override Direction reflect_diag_down { get { return Direction.NorthEast; } }
        public override Direction reflect_hori { get { return Direction.West; } }
        public override Direction reflect_vert { get { return Direction.East; } }
        public override Direction reflect_branch_left(bool right) { return Direction.East; }
        public override Direction reflect_branch_right(bool right) { return right ? (Direction) Direction.NorthWest : Direction.SouthWest; }

        public override Direction reverse { get { return Direction.East; } }
        public override PointAxial vec { get { return new PointAxial(-1, 0); } }
        public override int GetHashCode() { return 24567; }
    }

    sealed class SouthWest : Direction
    {
        public override Direction right { get { return Direction.West; } }
        public override Direction left { get { return Direction.SouthEast; } }

        public override Direction reflect_diag_up { get { return Direction.SouthWest; } }
        public override Direction reflect_diag_down { get { return Direction.East; } }
        public override Direction reflect_hori { get { return Direction.NorthWest; } }
        public override Direction reflect_vert { get { return Direction.SouthEast; } }
        public override Direction reflect_branch_left(bool right) { return Direction.West; }
        public override Direction reflect_branch_right(bool right) { return Direction.NorthEast; }

        public override Direction reverse { get { return Direction.NorthEast; } }
        public override PointAxial vec { get { return new PointAxial(-1, 1); } }
        public override int GetHashCode() { return 245678; }
    }

    sealed class SouthEast : Direction
    {
        public override Direction right { get { return Direction.SouthWest; } }
        public override Direction left { get { return Direction.East; } }

        public override Direction reflect_diag_up { get { return Direction.West; } }
        public override Direction reflect_diag_down { get { return Direction.SouthEast; } }
        public override Direction reflect_hori { get { return Direction.NorthEast; } }
        public override Direction reflect_vert { get { return Direction.SouthWest; } }
        public override Direction reflect_branch_left(bool right) { return Direction.NorthWest; }
        public override Direction reflect_branch_right(bool right) { return Direction.East; }

        public override Direction reverse { get { return Direction.NorthWest; } }
        public override PointAxial vec { get { return new PointAxial(0, 1); } }
        public override int GetHashCode() { return 2456783; }
    }

    sealed class East : Direction
    {
        public override Direction right { get { return Direction.SouthEast; } }
        public override Direction left { get { return Direction.NorthEast; } }

        public override Direction reflect_diag_up { get { return Direction.NorthWest; } }
        public override Direction reflect_diag_down { get { return Direction.SouthWest; } }
        public override Direction reflect_hori { get { return Direction.East; } }
        public override Direction reflect_vert { get { return Direction.West; } }
        public override Direction reflect_branch_left(bool right) { return right ? (Direction) Direction.SouthEast : Direction.NorthEast; }
        public override Direction reflect_branch_right(bool right) { return Direction.West; }

        public override Direction reverse { get { return Direction.West; } }
        public override PointAxial vec { get { return new PointAxial(1, 0); } }
        public override int GetHashCode() { return 24567837; }
    }
}