namespace EsotericIDE.Whitespace
{
    sealed class WhitespaceSettings : LanguageSettings
    {
        public NumberInputSemantics NumberInputSemantics = NumberInputSemantics.LinewiseLenient;
        public CharacterSemantics OutputSemantics = CharacterSemantics.Bytewise;
    }
}
