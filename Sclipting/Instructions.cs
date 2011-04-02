
namespace EsotericIDE.Sclipting
{
    enum ScliptingInstruction
    {
        // General
        [Instruction('標', "mark", "Pushes a mark.", InstructionType.SingularInstruction)]
        Mark,
        [Instruction('丟', "discard", "Pops an item.", InstructionType.SingularInstruction)]
        Discard,

        // String or list manipulation
        [Instruction('長', "length", "Returns length of list/string.", InstructionType.SingularInstruction)]
        Length,
        [Instruction('復', "repeat", "Repeats a list, string, or byte array.", InstructionType.SingularInstruction)]
        Repeat,
        [Instruction('併', "combine", "Concatenates everything above the last mark into a string.", InstructionType.SingularInstruction)]
        CombineString,
        [Instruction('并', "combine", "Places everything above the last mark in a list.", InstructionType.SingularInstruction)]
        CombineList,
        [Instruction('掘', "excavate", "Retrieves the nth item/character in a list/string (pop list/string).", InstructionType.SingularInstruction)]
        Excavate,
        [Instruction('挖', "dig out", "Retrieves the nth item/character in a list/string (keep list/string).", InstructionType.SingularInstruction)]
        DigOut,
        [Instruction('插', "insert", "Replaces the nth item/character in a list/string.", InstructionType.SingularInstruction)]
        Insert,
        [Instruction('反', "reverse", "Reverses a string.", InstructionType.SingularInstruction)]
        Reverse,
        [Instruction('捃', "sort", "Sort a list/string by string value.", InstructionType.SingularInstruction)]
        Sort,
        [Instruction('訂', "arrange", "Sort a list/string by integer value/codepoint.", InstructionType.SingularInstruction)]
        Arrange,

        // String manipulation only
        [Instruction('講', "explain", "Unicode codepoint for first character in a string.", InstructionType.SingularInstruction)]
        Explain,
        [Instruction('字', "character", "Character from Unicode codepoint.", InstructionType.SingularInstruction)]
        Character,

        // Regular expressions
        [Instruction('現', "appear", "Current regular expression match.", InstructionType.SingularInstruction)]
        Appear,
        [Instruction('坼', "split", "Split string using regular expression (pop).", InstructionType.SingularInstruction)]
        SplitPop,
        [Instruction('裂', "split", "Split string using regular expression (no pop).", InstructionType.SingularInstruction)]
        SplitNoPop,

        // Arithmetic
        [Instruction('加', "add", "Adds two numbers.", InstructionType.SingularInstruction)]
        Add,
        [Instruction('減', "subtract", "Subtracts two numbers.", InstructionType.SingularInstruction)]
        Subtract,
        [Instruction('乘', "multiply", "Multiplies two numbers.", InstructionType.SingularInstruction)]
        Multiply,
        [Instruction('除', "divide", "Divides two numbers as floats.", InstructionType.SingularInstruction)]
        DivideFloat,
        [Instruction('分', "divide", "Divides two integers using integer division.", InstructionType.SingularInstruction)]
        DivideInt,
        [Instruction('剩', "leftovers", "Remainder (modulo).", InstructionType.SingularInstruction)]
        Leftovers,
        [Instruction('方', "power", "Exponentiation.", InstructionType.SingularInstruction)]
        Power,
        [Instruction('負', "negative", "Negative (unary minus).", InstructionType.SingularInstruction)]
        Negative,
        [Instruction('對', "correct", "Absolute value.", InstructionType.SingularInstruction)]
        Correct,
        [Instruction('增', "increase", "Increment by one.", InstructionType.SingularInstruction)]
        Increase,
        [Instruction('貶', "decrease", "Decrement by one.", InstructionType.SingularInstruction)]
        Decrease,
        [Instruction('左', "left", "Shift left.", InstructionType.SingularInstruction)]
        Left,
        [Instruction('右', "right", "Shift right.", InstructionType.SingularInstruction)]
        Right,
        [Instruction('雙', "both", "Bitwise and.", InstructionType.SingularInstruction)]
        Both,
        [Instruction('另', "other", "Bitwise or.", InstructionType.SingularInstruction)]
        Other,
        [Instruction('倆', "clever", "Bitwise xor.", InstructionType.SingularInstruction)]
        Clever,

        // Logic
        [Instruction('小', "small", "Less than.", InstructionType.SingularInstruction)]
        Small,
        [Instruction('大', "great", "Greater than.", InstructionType.SingularInstruction)]
        Great,
        [Instruction('與', "and", "Logical (boolean) and.", InstructionType.SingularInstruction)]
        And,
        [Instruction('或', "or", "Logical (boolean) or.", InstructionType.SingularInstruction)]
        Or,
        [Instruction('隻', "one of pair", "Logical (boolean) xor.", InstructionType.SingularInstruction)]
        OneOfPair,
        [Instruction('同', "same", "Exactly the same.", InstructionType.SingularInstruction)]
        Same,
        [Instruction('侔', "equal", "Equal as integers.", InstructionType.SingularInstruction)]
        Equal,
        [Instruction('肖', "resemble", "Equal as strings.", InstructionType.SingularInstruction)]
        Resemble,
        [Instruction('嗎', "is it?", "Conditional operator.", InstructionType.SingularInstruction)]
        IsIt,

        // Block instructions
        [Instruction('是', "yes", "If (pop)", InstructionType.BlockHead)]
        Yes,
        [Instruction('倘', "if", "If (no pop)", InstructionType.BlockHead)]
        If,
        [Instruction('數', "count", "For loop", InstructionType.BlockHead)]
        Count,
        [Instruction('各', "each", "Foreach loop (pop)", InstructionType.BlockHead)]
        Each,
        [Instruction('每', "every", "Foreach loop (no pop)", InstructionType.BlockHead)]
        Every,
        [Instruction('套', "loop", "While loop (pop)", InstructionType.BlockHead)]
        Loop,
        [Instruction('要', "necessity", "While loop (no pop)", InstructionType.BlockHead)]
        Necessity,
        [Instruction('迄', "until", "Until loop (pop)", InstructionType.BlockHead)]
        Until,
        [Instruction('到', "arrive", "Until loop (no pop)", InstructionType.BlockHead)]
        Arrive,
        [Instruction('換', "substitute", "Regular expression replace first (pop)", InstructionType.BlockHead)]
        ReplaceFirstPop,
        [Instruction('代', "replace", "Regular expression replace first (no pop)", InstructionType.BlockHead)]
        ReplaceFirstNoPop,
        [Instruction('替', "replace", "Regular expression replace all (pop)", InstructionType.BlockHead)]
        ReplaceAllPop,
        [Instruction('更', "replace", "Regular expression replace all (no pop)", InstructionType.BlockHead)]
        ReplaceAllNoPop,

        [Instruction('不', "no", "Else (pop)", InstructionType.BlockElse)]
        No,
        [Instruction('逆', "opposite", "Else (no pop)", InstructionType.BlockElse)]
        Opposite,
        [Instruction('終', "end", "End", InstructionType.BlockEnd)]
        End,
    }

    enum InstructionType
    {
        SingularInstruction,
        BlockHead,
        BlockElse,
        BlockEnd
    }

    enum StackOrRegexInstructionType
    {
        CopyFromTop,
        CopyFromBottom,
        MoveFromTop,
        MoveFromBottom,
        SwapFromBottom,
        RegexCapture
    }
}
