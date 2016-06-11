using System.Xml.Linq;
using RT.Util;
using RT.Util.Serialization;

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
            object m;
            if (!ExactConvert.Try(typeof(MirrorType), el.Value, out m))
                el.Remove();
        }
    }
}
