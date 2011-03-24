
namespace EsotericIDE.Sclipting
{
    enum ScliptingInstructions
    {
        // General
        [Instruction('標', "mark", "Pushes a mark.", InstructionType.SingleInstruction)]
        Mark,
        [Instruction('丟', "discard", "Pops an item.", InstructionType.SingleInstruction)]
        Discard,

        // String manipulation
        [Instruction('長', "length", "Returns length of string.", InstructionType.SingleInstruction)]
        Length,
        [Instruction('復', "repeat", "Repeats a string.", InstructionType.SingleInstruction)]
        Repeat,
        [Instruction('併', "combine", "Concatenates everything above the last mark.", InstructionType.SingleInstruction)]
        Combine,
        [Instruction('掘', "excavate", "Retrieves the nth character in a string (pop string).", InstructionType.SingleInstruction)]
        Excavate,
        [Instruction('挖', "dig out", "Retrieves the nth character in a string (keep string).", InstructionType.SingleInstruction)]
        DigOut,
        [Instruction('講', "explain", "Unicode codepoint for first character in a string.", InstructionType.SingleInstruction)]
        Explain,
        [Instruction('字', "character", "Character from Unicode codepoint.", InstructionType.SingleInstruction)]
        Character,
        [Instruction('反', "reverse", "Reverses a string.", InstructionType.SingleInstruction)]
        Reverse,

        // Regular expressions
        [Instruction('現', "appear", "Current regular expression match.", InstructionType.SingleInstruction)]
        Appear,

        // Arithmetic
        [Instruction('加', "add", "Adds two numbers.", InstructionType.SingleInstruction)]
        Add,
        [Instruction('減', "subtract", "Subtracts two numbers.", InstructionType.SingleInstruction)]
        Subtract,
        [Instruction('乘', "multiply", "Multiplies two numbers.", InstructionType.SingleInstruction)]
        Multiply,
        [Instruction('除', "divide", "Divides two numbers as floats.", InstructionType.SingleInstruction)]
        DivideFloat,
        [Instruction('分', "divide", "Divides two integers using integer division.", InstructionType.SingleInstruction)]
        DivideInt,
        [Instruction('剩', "leftovers", "Remainder (modulo).", InstructionType.SingleInstruction)]
        Leftovers,
        [Instruction('方', "power", "Exponentiation.", InstructionType.SingleInstruction)]
        Power,
        [Instruction('負', "negative", "Negative (unary minus).", InstructionType.SingleInstruction)]
        Negative,
        [Instruction('增', "increase", "Increment by one.", InstructionType.SingleInstruction)]
        Increase,
        [Instruction('貶', "decrease", "Decrement by one.", InstructionType.SingleInstruction)]
        Decrease,
        [Instruction('左', "left", "Shift left.", InstructionType.SingleInstruction)]
        Left,
        [Instruction('右', "right", "Shift right.", InstructionType.SingleInstruction)]
        Right,
        [Instruction('雙', "both", "Bitwise and.", InstructionType.SingleInstruction)]
        Both,
        [Instruction('另', "other", "Bitwise or.", InstructionType.SingleInstruction)]
        Other,
        [Instruction('倆', "clever", "Bitwise xor.", InstructionType.SingleInstruction)]
        Clever,

        // Logic
        [Instruction('小', "small", "Less than.", InstructionType.SingleInstruction)]
        Small,
        [Instruction('大', "great", "Greater than.", InstructionType.SingleInstruction)]
        Great,
        [Instruction('與', "and", "Logical (boolean) and.", InstructionType.SingleInstruction)]
        And,
        [Instruction('或', "or", "Logical (boolean) or.", InstructionType.SingleInstruction)]
        Or,
        [Instruction('隻', "one of pair", "Logical (boolean) xor.", InstructionType.SingleInstruction)]
        OneOfPair,
        [Instruction('同', "same", "Exactly the same.", InstructionType.SingleInstruction)]
        Same,
        [Instruction('侔', "equal", "Equal as integers.", InstructionType.SingleInstruction)]
        Equal,
        [Instruction('肖', "resemble", "Equal as strings.", InstructionType.SingleInstruction)]
        Resemble,
        [Instruction('嗎', "is it?", "Conditional operator.", InstructionType.SingleInstruction)]
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
        SingleInstruction,
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
