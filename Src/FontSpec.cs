using System.Drawing;
using RT.Util.Serialization;

namespace EsotericIDE
{
    sealed class FontSpec
    {
        public string Name { get; private set; }
        public float Size { get; private set; }
        public FontStyle Style { get; private set; }
        public Color Color { get; private set; }

        public FontSpec(string name, float size, FontStyle style, Color color)
        {
            Name = name;
            Size = size;
            Style = style;
            Color = color;
        }

        public FontSpec(Font font, Color color) : this(font.Name, font.Size, font.Style, color) { }

        private FontSpec() { }  // for Classify

        public Font Font { get { return new Font(Name, Size, Style); } }
        public FontSpec SetFont(string name, float size, FontStyle style) { return new FontSpec(name, size, style, Color); }
        public FontSpec SetFont(Font font) { return new FontSpec(font.Name, font.Size, font.Style, Color); }
        public FontSpec SetColor(Color color) { return new FontSpec(Name, Size, Style, color); }
    }
}
