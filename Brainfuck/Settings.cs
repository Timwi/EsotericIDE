using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EsotericIDE.Languages
{
    partial class Brainfuck
    {
        private brainfuckSettings _settings = new brainfuckSettings
        {
            InputType = ioType.Numbers,
            OutputType = ioType.Numbers,
            CellType = cellType.BigInts
        };

        private sealed class brainfuckSettings : LanguageSettings
        {
            public ioType InputType;
            public ioType OutputType;
            public cellType CellType;
        }

        public override LanguageSettings GetSettings()
        {
            return _settings;
        }

        public override void SetSettings(LanguageSettings settings)
        {
            if (!(settings is brainfuckSettings))
                throw new InvalidOperationException();
            _settings = (brainfuckSettings) settings;
        }
    }
}
