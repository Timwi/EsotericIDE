using System;

namespace EsotericIDE.Runic.Math
{
    public static class MathHelper
    {
        public enum NumericRelationship
        {
            GreaterThan = 1,
            EqualTo = 0,
            LessThan = -1
        };

        public static double PIOver180D = System.Math.PI / 180.0;
        public static double PID = 3.14159265358979323846264338327950288419716939937510;

        public static int getLayerForMask(int maskVal)
        {
            int j = 1;
            while ((maskVal & 1) == 0)
            {
                maskVal /= 2;
                j++;
            }
            return j;
        }

        public static NumericRelationship Compare(ValueType v1, ValueType v2)
        {
            double a = GetValue(v1);
            double b = GetValue(v2);
            if (Approximately((float) a, (float) b)) return NumericRelationship.EqualTo;
            if (a > b) return NumericRelationship.GreaterThan;
            return NumericRelationship.LessThan;
        }

        public static bool Approximately(float a, float b)
        {
            return (System.Math.Abs(a - b) <= 10 * float.Epsilon);
        }

        public static double GetValue(ValueType v1)
        {
            if (IsInteger(v1))
                return (int) v1;
            if (v1 is char)
                return (char) v1 - 0;
            else if (v1 is Vector3)
            {
                return ((Vector3) v1).magnitude;
            }
            else return Convert.ToDouble(v1);
        }

        public static float EaseLinear(float time, float from, float to, float duration)
        {
            time /= duration;
            float by = to - from;
            return by * time + from;
        }

        public static float EaseInQuadratic(float time, float from, float to, float duration)
        {
            time /= duration;
            float by = to - from;
            return by * time * time + from;
        }

        public static float EaseInCubic(float time, float from, float to, float duration)
        {
            time /= duration;
            float by = to - from;
            return by * time * time * time + from;
        }

        public static float EaseInQuartic(float time, float from, float to, float duration)
        {
            time /= duration;
            float by = to - from;
            return by * time * time * time * time + from;
        }

        public static float EaseOutQuadratic(float time, float from, float to, int duration)
        {
            time /= duration;
            float by = to - from;
            return -by * time * (time - 2) + from;
        }

        public static double DegreesToRadiansD(double Degrees)
        {
            return Degrees * PIOver180D;
        }

        public static float RadiansToDegrees(float Radians)
        {
            return Radians / ((float) System.Math.PI / 180f);
        }

        public static double SetAngleAndEnsureWithinRange(double newAngle, double angleCeiling)
        {
            double acutalAngle;

            acutalAngle = newAngle;
            if (acutalAngle < 0f)
                acutalAngle += angleCeiling;
            if (acutalAngle >= angleCeiling)
                acutalAngle -= angleCeiling;
            return acutalAngle;
        }

        public static void shuffleArray<T>(ref T[] array, System.Random random)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int index = random.Next(i + 1);
                T a = array[index];
                array[index] = array[i];
                array[i] = a;
            }
        }

        public static bool IsInteger(ValueType value)
        {
            return (value is sbyte || value is short || value is int
                    || value is long || value is byte || value is ushort
                    || value is uint || value is ulong);
        }
    }
}