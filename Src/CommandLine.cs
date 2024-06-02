using System;
using System.IO;
using System.Linq;
using RT.CommandLine;
using RT.PostBuild;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace EsotericIDE
{
    [CommandLine, DocumentationLiteral("Opens the Interpreter//Debugger Engine for esoteric programming languages (Esoteric IDE).")]
    sealed class CommandLine : ICommandLineValidatable
    {
        [IsPositional, DocumentationLiteral("Specifies a path and filename of a file to open.")]
        public string Filename = null;

        [Option("-l", "--language"), DocumentationLiteral("Specifies a programming language to pre-select in the language drop-down.")]
        public string Language = null;

        [Ignore]
        public ProgrammingLanguage LanguagePreselect = null;

        private static void PostBuildCheck(IPostBuildReporter rep)
        {
            CommandLineParser.PostBuildStep<CommandLine>(rep);
        }

        public ConsoleColoredString Validate()
        {
            if (Filename != null && !File.Exists(Filename))
                return "The specified file does not exist.";

            if (Language != null)
            {
                LanguagePreselect = Mainform.Languages.FirstOrDefault(pl => pl.LanguageName.Equals(Language, StringComparison.InvariantCultureIgnoreCase));
                if (LanguagePreselect == null)
                    return "The specified programming language does not exist. Currently support programming languages are: " + Mainform.Languages.Select(pl => pl.LanguageName).JoinString(", ");
            }

            return null;
        }
    }
}
