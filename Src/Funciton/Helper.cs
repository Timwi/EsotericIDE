using System.Numerics;
using System.Text;

namespace EsotericIDE.Languages
{
    static class FuncitonHelper
    {
        public static BigInteger StringToInteger(string str)
        {
            BigInteger result = BigInteger.Zero;
            int atBit = 0;
            int i = 0;
            while (i < str.Length)
            {
                result += (BigInteger) char.ConvertToUtf32(str, i) << atBit;
                if (char.IsSurrogate(str, i))
                    i += 2;
                else
                    i++;
                atBit += 21;
            }
            if (str.Length > 0 && str[str.Length - 1] == '\0')
                result |= (BigInteger.MinusOne << atBit);
            return result;
        }

        public static string IntegerToString(BigInteger integer)
        {
            var sb = new StringBuilder();
            while (integer != BigInteger.Zero && integer != BigInteger.MinusOne)
            {
                sb.Append(char.ConvertFromUtf32((int) (integer & ((1 << 21) - 1))));
                integer >>= 21;
            }
            return sb.ToString();
        }
    }
}
