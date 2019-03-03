using System;
using System.Collections.Generic;
using System.Linq;

namespace EsotericIDE.Runic.Math
{
    public static class Extentions
    {
        public static bool IsArrayOf<T>(this Type type)
        {
            return type == typeof(T[]);
        }

        public static void RotateListLeft<T>(this List<T> list)
        {
            if (list.Count <= 1) return;
            T o = list[0];
            list.RemoveAt(0);
            list.Add(o);
        }

        public static void RotateListRight<T>(this List<T> list)
        {
            if (list.Count <= 1) return;
            list.Reverse();
            list.RotateListLeft();
            list.Reverse();
        }

        public static IEnumerable<string> ChunksUpto(this string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, System.Math.Min(maxChunkSize, str.Length - i));
        }

        public static string Reverse(this string s)
        {
            char[] charArray = s.ToCharArray();
            List<char> l = charArray.ToList();
            l.Reverse();
            return new string(l.ToArray());
        }

        public static string RotateLeft(this string s)
        {
            char[] charArray = s.ToCharArray();
            List<char> l = charArray.ToList();
            l.RotateListLeft();
            return new string(l.ToArray());
        }

        public static string RotateRight(this string s)
        {
            char[] charArray = s.ToCharArray();
            List<char> l = charArray.ToList();
            l.RotateListRight();
            return new string(l.ToArray());
        }
    }
}
