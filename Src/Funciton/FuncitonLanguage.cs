using System;
using System.IO;
using System.Numerics;
using System.Windows.Forms;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Dialogs;

namespace EsotericIDE.Languages
{
    sealed partial class Funciton : ProgrammingLanguage
    {
        public override string LanguageName { get { return "Funciton"; } }
        public override string DefaultFileExtension { get { return "fnc"; } }
        public override string GetInfo(string source, int cursorPosition) { return null; }

        public override ExecutionEnvironment Compile(string source, string input)
        {
            BigInteger? stdin = null;

            if (input != null)
            {
                switch (_settings.InputType)
                {
                    case inputType.InterpretAsInteger:
                        BigInteger result;
                        if (!BigInteger.TryParse(input, out result))
                            throw new CompileException("The input is not a valid integer.");
                        stdin = result;
                        break;

                    case inputType.InterpretAsString:
                        stdin = FuncitonHelper.StringToInteger(input);
                        break;

                    default:
                        throw new CompileException("The settings specify an invalid input type.");
                }
            }

            return compileAndAnalyse(_settings.AdditionalSourceFiles.Select(f => Tuple.Create(f, File.ReadAllText(f))).Concat(new Tuple<string, string>(null, source)), stdin);
        }
    }
}
