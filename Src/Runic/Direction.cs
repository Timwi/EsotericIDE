using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsotericIDE.Runic {
	public enum Direction {
		UP, DOWN, LEFT, RIGHT
	}

	public static class DirectionHelper {
		public static int GetX(Direction d) {
			if(d == Direction.LEFT) {
				return -1;
			}
			if(d == Direction.RIGHT) {
				return 1;
			}
			return 0;
		}

		public static int GetY(Direction d) {
			if(d == Direction.UP) {
				return -1;
			}
			if(d == Direction.DOWN) {
				return 1;
			}
			return 0;
		}

		public static Direction Reflect(Direction d) {
			if((int)d % 2 == 0) {
				return d + 1;
			}
			return d - 1;
		}

		public static Direction RotateCW(Direction d) {
			switch(d) {
				case Direction.DOWN:
					return Direction.LEFT;
				case Direction.LEFT:
					return Direction.UP;
				case Direction.UP:
					return Direction.RIGHT;
				case Direction.RIGHT:
					return Direction.DOWN;
			}
			return d;
		}

		public static Direction RotateCCW(Direction d) {
			switch(d) {
				case Direction.DOWN:
					return Direction.RIGHT;
				case Direction.LEFT:
					return Direction.DOWN;
				case Direction.UP:
					return Direction.LEFT;
				case Direction.RIGHT:
					return Direction.UP;
			}
			return d;
		}
	}
}