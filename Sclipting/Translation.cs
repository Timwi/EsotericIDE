using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RT.Util.Lingo;

namespace EsotericIDE.Sclipting
{
    enum TranslationGroup
    {
        [LingoGroup("Insert menu", "Strings used to construct the “Insert” menu.")]
        InsertMenu,
        [LingoGroup("Parse errors", "Strings used to inform the user of malformed code.")]
        ParseErrors
    }

    sealed class Translation : TranslationBase
    {
        public Translation() : base(Language.EnglishUK) { }

        public TrString ByteArray = "Byte array:";
        public TrString Integer = "Integer:";
        public TrString String = "String:";
        public TrString Float = "Float:";
        public TrString Mark = "Mark";
        public TrString NegativeNumber = "Negative number:";
        public TrString BlockHeadInstruction = "Block head instruction:";
        public TrString BlockElseInstruction = "Block else instruction:";
        public TrString BlockEndInstruction = "Block end instruction:";
        public TrString SingularInstruction = "Instruction:";
        public TrString StackInstruction = "Stack instruction:";
        public TrString RegexInstruction = "Regex instruction:";

        public TrString ActiveRegexBlocks = "Active regular expression blocks:";

        public InsertMenuTranslation InsertMenu = new InsertMenuTranslation();
        public ParseErrorsTranslation ParseErrors = new ParseErrorsTranslation();
    }

    [LingoInGroup(TranslationGroup.InsertMenu)]
    sealed class InsertMenuTranslation
    {
        public TrString SingularInstruction = "Si&ngular instruction";
        public TrString BlockInstruction = "&Block instruction";
        public TrString StackInstruction = "S&tack instruction";
        public TrString StackInstructionCopyFromBottom = "&Copy from bottom";
        public TrString StackInstructionMoveFromBottom = "&Move from bottom";
        public TrString StackInstructionSwapFromBottom = "&Swap from bottom";
        public TrString StackInstructionCopyFromTop = "C&opy from top";
        public TrString StackInstructionMoveFromTop = "Mo&ve from top";
        public TrString RegexInstruction = "&Regex instruction";
        public TrString Integer = "&Integer...";
        public TrString String = "&String...";
        public TrString ByteArray = "Byte &array...";

        public TrString IntegerPrompt = "Type an integer to insert (must be greater than −7077):";
        public TrString IntegerError = "The integer you typed is not a valid literal integer for Sclipting. Literal integers must be greater than −7077.";
        public TrString StringPrompt = "Type a string to insert (in C-escaped format; backslashes must be escaped):";
        public TrString StringError = "The string you typed is not a valid C-escaped string. Please ensure that your backslashes are escaped.";
        public TrString ByteArrayPrompt = "Type a byte array to insert (in hexdump format; two hexadecimal digits per byte):";
        public TrString ByteArrayError = "The text you entered is not valid hexadecimal. Please ensure that you enter an even number of characters 0-9/a-f.";
    }

    [LingoInGroup(TranslationGroup.InsertMenu)]
    sealed class ParseErrorsTranslation
    {
        public TrString UnrecognisedInstruction = "Unrecognised instruction: “{0}”.";
        public TrString ForLoopCannotUseNonPopElse = "The block else instruction “逆” cannot be used with the block head instruction “數”.";
        [LingoNotes("Indicates that the programmer of the Sclipting interpreter forgot to implement an instruction.")]
        public TrString MissingInstruction = "Instruction “{0}” missing.";
        public TrString MissingEnd = "Block instruction “{0}” is missing a matching end instruction.";
        public TrString UnexpectedCharacter = "Unexpected character in program: “{0}”.";
        public TrString UnexpectedEndOfBlock = "Else or end instruction encountered without a matching block head instruction.";
    }
}
