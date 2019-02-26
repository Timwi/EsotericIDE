using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsotericIDE.Runic.math {
	public struct Vector3 {
		public static readonly Vector3 zero = new Vector3(0, 0, 0);
		public float x;
		public float y;
		public float z;
		public float magnitude;

		public Vector3(float x, float y, float z) {
			this.x = x;
			this.y = y;
			this.z = z;
			magnitude = (float)Math.Sqrt(x * x + y * y + z * z);
		}

		public static Vector3 operator +(Vector3 a, Vector3 b) {
			return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
		}

		public static Vector3 operator -(Vector3 a, Vector3 b) {
			return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
		}

		public static Vector3 Cross(Vector3 a, Vector3 b) {
			return new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.z);
		}

		public static float Dot(Vector3 a, Vector3 b) {
			return a.x * b.x + a.y * b.y + a.z * b.z;
		}

		public static Vector3 operator *(Vector3 a, float b) {
			return new Vector3(a.x * b, a.y * b, a.z * b);
		}

		public static Vector3 operator /(Vector3 a, float b) {
			return new Vector3(a.x / b, a.y / b, a.z / b);
		}

		public static bool operator ==(Vector3 a, Vector3 b) {
			return MathHelper.Approximately(a.x, b.x) && MathHelper.Approximately(a.y, b.y) && MathHelper.Approximately(a.z, b.z);
		}

		public static bool operator !=(Vector3 a, Vector3 b) {
			return !(MathHelper.Approximately(a.x, b.x) && MathHelper.Approximately(a.y, b.y) && MathHelper.Approximately(a.z, b.z));
		}

		public override bool Equals(object obj) {
			if(obj is Vector3) {
				return this == (Vector3)obj;
			}
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			unchecked {// Overflow is fine, just wrap
				int hash = 17;
				// Suitable nullity checks etc, of course :)
				hash = hash * 23 + x.GetHashCode();
				hash = hash * 23 + y.GetHashCode();
				hash = hash * 23 + z.GetHashCode();
				return hash;
			}
		}
	}
}
