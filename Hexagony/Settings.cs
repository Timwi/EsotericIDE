using System.Collections.Generic;
using System.Drawing;
using RT.Util.Serialization;

namespace EsotericIDE.Hexagony
{
    sealed class HexagonySettings : LanguageSettings
    {
        [ClassifySubstitute(typeof(ClassifyColorSubstitute))]
        public Color MemoryBackgroundColor = Color.White;
        [ClassifySubstitute(typeof(ClassifyColorSubstitute))]
        public Color MemoryGridZeroColor = Color.FromArgb(192, 192, 192);
        [ClassifySubstitute(typeof(ClassifyColorSubstitute))]
        public Color MemoryGridNonZeroColor = Color.Black;
        [ClassifySubstitute(typeof(ClassifyColorSubstitute))]
        public Color MemoryPointerColor = Color.FromArgb(255, 64, 0);
        public FontSpec MemoryValueFont;
        public FontSpec MemoryAnnotationFont;

        [ClassifyNotNull]
        public Dictionary<string, Dictionary<Direction, Dictionary<PointAxial, string>>> MemoryAnnotations = new Dictionary<string, Dictionary<Direction, Dictionary<PointAxial, string>>>();
        [ClassifyNotNull]
        public string LastMemoryAnnotationSet = "(default)";
    }
}
