using System;
using System.Drawing;
using System.Text.RegularExpressions;
using RT.Serialization;
using RT.Util.ExtensionMethods;

namespace EsotericIDE
{
    sealed class ClassifyColorSubstitute : IClassifySubstitute<Color, string>
    {
        public Color FromSubstitute(string instance)
        {
            if (instance.Length == 6 && Regex.IsMatch(instance, @"^[0-9a-fA-F]{6}$"))
                return Color.FromArgb(Convert.ToInt32(instance.Substring(0, 2), 16), Convert.ToInt32(instance.Substring(2, 2), 16), Convert.ToInt32(instance.Substring(4, 2), 16));
            if (instance.Length == 8 && Regex.IsMatch(instance, @"^[0-9a-fA-F]{8}$"))
                return Color.FromArgb(Convert.ToInt32(instance.Substring(0, 2), 16), Convert.ToInt32(instance.Substring(2, 2), 16), Convert.ToInt32(instance.Substring(4, 2), 16), Convert.ToInt32(instance.Substring(6, 2), 16));
            return Color.Black;
        }

        public string ToSubstitute(Color instance)
        {
            var ret = "{0:X2}{1:X2}{2:X2}".Fmt(instance.R, instance.G, instance.B);
            if (instance.A != 255)
                ret = "{0:X2}{1}".Fmt(instance.A, ret);
            return ret;
        }
    }
}
