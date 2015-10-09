using System.Numerics;

namespace EsotericIDE
{
    static class Util
    {
        public static BigInteger DivideRubyStyle(BigInteger num, BigInteger den)
        {
            BigInteger rem;
            return DivRemRubyStyle(num, den, out rem);
        }

        public static BigInteger ModuloRubyStyle(BigInteger num, BigInteger den)
        {
            BigInteger rem;
            var div = DivRemRubyStyle(num, den, out rem);
            return rem;
        }

        public static BigInteger DivRemRubyStyle(BigInteger num, BigInteger den, out BigInteger rem)
        {
            var div = BigInteger.DivRem(num, den, out rem);
            if (rem != 0 && (num < 0 ^ den < 0))
            {
                rem += den;
                div--;
            }
            return div;
        }

        public static int DivideRubyStyle(int num, int den)
        {
            int rem;
            return DivRemRubyStyle(num, den, out rem);
        }

        public static int ModuloRubyStyle(int num, int den)
        {
            int rem;
            var div = DivRemRubyStyle(num, den, out rem);
            return rem;
        }

        public static int DivRemRubyStyle(int num, int den, out int rem)
        {
            var div = num / den;
            rem = num % den;
            if (rem != 0 && (num < 0 ^ den < 0))
            {
                rem += den;
                div--;
            }
            return div;
        }
    }
}
