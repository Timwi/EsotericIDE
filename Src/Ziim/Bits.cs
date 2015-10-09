using System;
using System.Numerics;
using System.Text;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Ziim
{
    sealed class Bits
    {
        private BigInteger _raw;
        private int _length;
        private Bits() { }

        public static Bits Zero = new Bits { _raw = 0, _length = 1 };
        public static Bits Empty = new Bits { _raw = 0, _length = 0 };
        public Bits Concat(Bits other) { return new Bits { _raw = _raw | (other._raw << _length), _length = _length + other._length }; }
        public bool IsEmpty { get { return _length == 0; } }
        public bool IsFirstBitZero { get { return (_raw & 1) == 0; } }
        public Bits RemoveFirstBit() { return _length == 0 ? null : new Bits { _raw = _raw >> 1, _length = _length - 1 }; }
        public Bits Invert() { return new Bits { _raw = (~_raw) & ((BigInteger.One << _length) - 1), _length = _length }; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("{");
            for (int i = 0; i < _length; i++)
            {
                if (i > 0)
                    sb.Append(",");
                sb.Append((_raw & (BigInteger.One << i)) == 0 ? " 0" : " 1");
            }
            sb.Append(" }");
            return sb.ToString();
        }

        public static Bits FromString(string str)
        {
            var bytes = str.ToUtf8();
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = (byte) ((bytes[i] >> 7) | ((bytes[i] >> 5) & 2) | ((bytes[i] >> 3) & 4) | ((bytes[i] >> 1) & 8) | ((bytes[i] << 1) & 16) | ((bytes[i] << 3) & 32) | ((bytes[i] << 5) & 64) | ((bytes[i] << 7) & 128));
            return new Bits { _length = bytes.Length * 8, _raw = new BigInteger(bytes) };
        }

        public string ToOutput()
        {
            var rBytes = _raw.ToByteArray();
            for (int i = 0; i < rBytes.Length; i++)
                rBytes[i] = (byte) ((rBytes[i] >> 7) | ((rBytes[i] >> 5) & 2) | ((rBytes[i] >> 3) & 4) | ((rBytes[i] >> 1) & 8) | ((rBytes[i] << 1) & 16) | ((rBytes[i] << 3) & 32) | ((rBytes[i] << 5) & 64) | ((rBytes[i] << 7) & 128));
            var bytes = new byte[_length / 8];
            Buffer.BlockCopy(rBytes, 0, bytes, 0, Math.Min(bytes.Length, rBytes.Length));
            return bytes.FromUtf8();
        }
    }
}