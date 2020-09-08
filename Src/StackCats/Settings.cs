using System.Xml.Linq;
using RT.Serialization;

namespace EsotericIDE.StackCats
{
    sealed class StackCatsSettings : LanguageSettings, IClassifyXmlObjectProcessor
    {
        public IOType InputType = IOType.Bytes;
        public IOType OutputType = IOType.Bytes;
        public MirrorType ImplicitlyMirror = MirrorType.None;

        public void BeforeSerialize() { }
        public void AfterSerialize(XElement element) { }
        public void AfterDeserialize(XElement element) { }
        public void BeforeDeserialize(XElement element)
        {
            var el = element.Element("ImplicitlyMirror");
            if (el == null)
                return;
            if (!ExactConvert.Try(typeof(MirrorType), el.Value, out var m))
                el.Remove();
        }
    }
}
