
namespace EsotericIDE.Sclipting
{
    enum Instruction
    {
        // Block structure
        [Instruction('況', "condition", "End of condition block.", NodeType.BlockConditionEnd, InstructionGroup.BlockStructure)]
        Condition,
        [Instruction('不', "no", "Else (pop).", NodeType.BlockElse, InstructionGroup.BlockStructure)]
        No,
        [Instruction('逆', "opposite", "Else (no pop).", NodeType.BlockElse, InstructionGroup.BlockStructure)]
        Opposite,
        [Instruction('終', "end", "End of block.", NodeType.BlockEnd, InstructionGroup.BlockStructure)]
        End,

        // Stack manipulation
        [Instruction('丟', "discard", "Pops an item.", NodeType.SingularNode, InstructionGroup.StackManipulation)]
        Discard,
        [Instruction('棄', "abandon", "Pops two items.", NodeType.SingularNode, InstructionGroup.StackManipulation)]
        Abandon,

        // Loops and conditionals
        [Instruction('是', "yes", "If true (pop).", NodeType.BlockHead, InstructionGroup.LoopsConditionals)]
        Yes,
        [Instruction('倘', "if", "If true (no pop).", NodeType.BlockHead, InstructionGroup.LoopsConditionals)]
        If,
        [Instruction('沒', "not", "If false (pop).", NodeType.BlockHead, InstructionGroup.LoopsConditionals)]
        NotPop,
        [Instruction('毋', "not", "If false (no pop).", NodeType.BlockHead, InstructionGroup.LoopsConditionals)]
        NotNoPop,
        [Instruction('夠', "enough", "If non-empty (pop).", NodeType.BlockHead, InstructionGroup.LoopsConditionals)]
        Enough,
        [Instruction('含', "contain", "If non-empty (no pop).", NodeType.BlockHead, InstructionGroup.LoopsConditionals)]
        Contain,
        [Instruction('套', "loop", "While true (pop).", NodeType.BlockHead, InstructionGroup.LoopsConditionals)]
        Loop,
        [Instruction('要', "necessity", "While true (no pop).", NodeType.BlockHead, InstructionGroup.LoopsConditionals)]
        Necessity,
        [Instruction('迄', "until", "While false (pop).", NodeType.BlockHead, InstructionGroup.LoopsConditionals)]
        Until,
        [Instruction('到', "arrive", "While false (no pop).", NodeType.BlockHead, InstructionGroup.LoopsConditionals)]
        Arrive,
        [Instruction('滿', "full", "While non-empty (pop).", NodeType.BlockHead, InstructionGroup.LoopsConditionals)]
        Full,
        [Instruction('充', "be full", "While non-empty (no pop).", NodeType.BlockHead, InstructionGroup.LoopsConditionals)]
        BeFull,
        [Instruction('上', "up", "Integer for loop.", NodeType.BlockHead, InstructionGroup.LoopsConditionals)]
        Up,
        [Instruction('下', "down", "Integer for loop (backwards).", NodeType.BlockHead, InstructionGroup.LoopsConditionals)]
        Down,
        [Instruction('各', "each", "Foreach loop (pop).", NodeType.BlockHead, InstructionGroup.LoopsConditionals)]
        Each,
        [Instruction('每', "every", "Foreach loop (no pop).", NodeType.BlockHead, InstructionGroup.LoopsConditionals)]
        Every,

        // Arithmetic
        [Instruction('加', "add", "Adds two numbers.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Add,
        [Instruction('減', "subtract", "Subtracts two numbers.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Subtract,
        [Instruction('縮', "reduce", "Subtracts two numbers (reverse order).", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Reduce,
        [Instruction('乘', "multiply", "Multiplies two numbers.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Multiply,
        [Instruction('除', "divide", "Divides two numbers as floats.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        DivideFloat,
        [Instruction('分', "divide", "Divides two integers using integer division.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        DivideInt,
        [Instruction('剩', "leftovers", "Remainder (modulo).", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Leftovers,
        [Instruction('重', "double", "Doubles a number.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Double,
        [Instruction('半', "half", "Halves a number (result is float).", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Half,
        [Instruction('隔', "separate", "Halves an integer (equivalent to shift-right 1).", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Separate,
        [Instruction('方', "power", "Exponentiation.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Power,
        [Instruction('平', "flat", "Square.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Flat,
        [Instruction('根', "root", "Square root.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Root,
        [Instruction('負', "negative", "Negative (unary minus).", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Negative,
        [Instruction('對', "correct", "Absolute value.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Correct,
        [Instruction('增', "increase", "Increment by one.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Increase,
        [Instruction('貶', "decrease", "Decrement by one.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Decrease,
        [Instruction('數', "number", "Logarithm to base e.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Number,
        [Instruction('位', "position", "Logarithm to base 10.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Position,
        [Instruction('級', "level", "Logarithm to base 2.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Level,
        [Instruction('圜', "circle", "Rounds towards 0.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Circle1,
        [Instruction('圍', "circle", "Rounds away from 0.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Circle2,
        [Instruction('團', "sphere", "Rounds down.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Sphere,
        [Instruction('圓', "surround", "Rounds up.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Surround1,
        [Instruction('繞', "surround", "Rounds to the nearest integer (halves away from zero).", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Surround2,
        [Instruction('輪', "wheel", "Rounds to the nearest integer (halves to even).", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Wheel,

        // Arithmetic (bitwise)
        [Instruction('左', "left", "Shift left.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Left,
        [Instruction('右', "right", "Shift right.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Right,
        [Instruction('雙', "both", "Bitwise and.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Both,
        [Instruction('另', "other", "Bitwise or.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Other,
        [Instruction('倆', "clever", "Bitwise xor.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Clever,
        [Instruction('無', "not", "Bitwise not.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        BitwiseNot,
        [Instruction('啃', "gnaw", "Split numbers into bits.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Gnaw,
        [Instruction('嚙', "bite", "Split numbers into bits.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Bite,
        // Arithmetic (random)
        [Instruction('沌', "chaotic", "Random integer between 0 and 2³².", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Chaotic,
        [Instruction('紛', "disarray", "Random integer between 0 and specified maximum.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Disarray,
        [Instruction('胡', "wild", "Random integer within specified bounds.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Wild1,
        [Instruction('亂', "chaos", "Random float between 0 and 1.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Chaos,
        [Instruction('野', "wild", "Random float between 0 and specified maximum.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Wild2,
        [Instruction('猖', "wild", "Random float within specified bounds.", NodeType.SingularNode, InstructionGroup.Arithmetic)]
        Wild3,

        // Logic
        [Instruction('小', "small", "Less than.", NodeType.SingularNode, InstructionGroup.Logic)]
        Small,
        [Instruction('大', "great", "Greater than.", NodeType.SingularNode, InstructionGroup.Logic)]
        Great,
        [Instruction('少', "less", "Less than or equal to.", NodeType.SingularNode, InstructionGroup.Logic)]
        Less,
        [Instruction('瀰', "overflow", "Greater than or equal to.", NodeType.SingularNode, InstructionGroup.Logic)]
        Overflow,
        [Instruction('同', "same", "Exactly the same.", NodeType.SingularNode, InstructionGroup.Logic)]
        Same,
        [Instruction('侔', "equal", "Equal as integers.", NodeType.SingularNode, InstructionGroup.Logic)]
        Equal,
        [Instruction('肖', "resemble", "Equal as strings.", NodeType.SingularNode, InstructionGroup.Logic)]
        Resemble,
        [Instruction('差', "different", "Not exactly the same.", NodeType.SingularNode, InstructionGroup.Logic)]
        Different1,
        [Instruction('异', "different", "Not equal as integers.", NodeType.SingularNode, InstructionGroup.Logic)]
        Different2,
        [Instruction('殊', "different", "Not equal as strings.", NodeType.SingularNode, InstructionGroup.Logic)]
        Different3,
        [Instruction('與', "and", "Logical (boolean) and.", NodeType.SingularNode, InstructionGroup.Logic)]
        And,
        [Instruction('或', "or", "Logical (boolean) or.", NodeType.SingularNode, InstructionGroup.Logic)]
        Or,
        [Instruction('隻', "one of pair", "Logical (boolean) xor.", NodeType.SingularNode, InstructionGroup.Logic)]
        OneOfPair,
        [Instruction('非', "not", "Logical (boolean) not.", NodeType.SingularNode, InstructionGroup.Logic)]
        Not,
        [Instruction('嗎', "is it?", "Conditional operator.", NodeType.SingularNode, InstructionGroup.Logic)]
        IsIt,

        // Lambda function instructions
        [Instruction('塊', "block", "Introduces an anonymous function (lambda expression).", NodeType.BlockHead, InstructionGroup.Functions)]
        Block,
        [Instruction('掳', "capture", "Introduces an anonymous function (lambda expression) that captures one item.", NodeType.BlockHead, InstructionGroup.Functions)]
        Capture,
        [Instruction('開', "initiate", "Pops and executes a function (lambda).", NodeType.FunctionExecutionNode, InstructionGroup.Functions)]
        Initiate,
        [Instruction('辦', "handle", "Pops a function (lambda), executes it, and then pushes it back on when it returns.", NodeType.FunctionExecutionNode, InstructionGroup.Functions)]
        Handle,
        [Instruction('演', "perform", "Executes a function (lambda) without popping it.", NodeType.FunctionExecutionNode, InstructionGroup.Functions)]
        Perform,

        // List/string manipulation (indexed list/string instruction equivalents)
        [Instruction('掘', "excavate", "Retrieves the nth item/character in a list/string (pop).", NodeType.SingularNode, InstructionGroup.ListStringManipulation), SingularListStringInstruction(ListStringInstruction.RetrievePop, false)]
        Excavate,
        [Instruction('掊', "dig", "Retrieves the nth-last item/character in a list/string (pop).", NodeType.SingularNode, InstructionGroup.ListStringManipulation), SingularListStringInstruction(ListStringInstruction.RetrievePop, true)]
        Dig,
        [Instruction('挖', "dig out", "Retrieves the nth item/character in a list/string (no pop).", NodeType.SingularNode, InstructionGroup.ListStringManipulation), SingularListStringInstruction(ListStringInstruction.RetrieveNoPop, false)]
        DigOut,
        [Instruction('采', "collect", "Retrieves the nth-last item/character in a list/string (no pop).", NodeType.SingularNode, InstructionGroup.ListStringManipulation), SingularListStringInstruction(ListStringInstruction.RetrieveNoPop, true)]
        Collect,
        [Instruction('栽', "cultivate", "Inserts an item/character into a list/string at a specified index.", NodeType.SingularNode, InstructionGroup.ListStringManipulation), SingularListStringInstruction(ListStringInstruction.Insert, false)]
        Cultivate,
        [Instruction('種', "seed", "Inserts an item/character into a list/string at a specified reverse index.", NodeType.SingularNode, InstructionGroup.ListStringManipulation), SingularListStringInstruction(ListStringInstruction.Insert, true)]
        Seed,
        [Instruction('殲', "annihilate", "Deletes the nth item/character in a list/string.", NodeType.SingularNode, InstructionGroup.ListStringManipulation), SingularListStringInstruction(ListStringInstruction.Delete, false)]
        Annihilate,
        [Instruction('摧', "destroy", "Deletes the nth-last item/character in a list/string.", NodeType.SingularNode, InstructionGroup.ListStringManipulation), SingularListStringInstruction(ListStringInstruction.Delete, true)]
        Destroy,
        [Instruction('裒', "take out", "Retrieves and deletes the nth item/character in a list/string.", NodeType.SingularNode, InstructionGroup.ListStringManipulation), SingularListStringInstruction(ListStringInstruction.RetrieveDelete, false)]
        TakeOut,
        [Instruction('抽', "pull out", "Retrieves and deletes the nth-last item/character in a list/string.", NodeType.SingularNode, InstructionGroup.ListStringManipulation), SingularListStringInstruction(ListStringInstruction.RetrieveDelete, true)]
        PullOut,
        [Instruction('插', "insert", "Replaces the nth item/character in a list/string.", NodeType.SingularNode, InstructionGroup.ListStringManipulation), SingularListStringInstruction(ListStringInstruction.Replace, false)]
        Insert,
        [Instruction('恢', "restore", "Replaces the nth-last item/character in a list/string.", NodeType.SingularNode, InstructionGroup.ListStringManipulation), SingularListStringInstruction(ListStringInstruction.Replace, true)]
        Restore,
        [Instruction('混', "mix", "Exchanges the nth item/character in a list/string with the value on the stack.", NodeType.SingularNode, InstructionGroup.ListStringManipulation), SingularListStringInstruction(ListStringInstruction.Exchange, false)]
        MixForward,
        [Instruction('拌', "mix", "Exchanges the nth-last item/character in a list/string with the value on the stack.", NodeType.SingularNode, InstructionGroup.ListStringManipulation), SingularListStringInstruction(ListStringInstruction.Exchange, true)]
        MixBackward,

        // List/string manipulation (other)
        [Instruction('匱', "lack", "Pushes the empty list.", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Lack,
        [Instruction('虛', "empty", "Pushes the empty string.", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Empty,
        [Instruction('長', "length", "Returns length of list/string (pop).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Length,
        [Instruction('梴', "long", "Returns length of list/string (no pop).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Long,
        [Instruction('復', "repeat", "Repeats a list, string, or byte array (list/string expected first).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Repeat,
        [Instruction('伸', "extend", "Repeats a list, string, or byte array (amount expected first).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Extend,
        [Instruction('疊', "repeat", "Creates a list of repetitions of an item (item expected first).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        RepeatIntoList,
        [Instruction('張', "stretch", "Creates a list of repetitions of an item (amount expected first).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Stretch,
        [Instruction('標', "mark", "Pushes a mark.", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Mark,
        [Instruction('併', "combine", "Concatenates everything above the last mark into a string.", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        CombineString,
        [Instruction('并', "combine", "Places everything above the last mark into a list.", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        CombineList,
        [Instruction('合', "combine", "Concatenates two lists or strings.", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Combine,
        [Instruction('融', "blend", "Concatenates two lists or strings (reverse order).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Blend,
        [Instruction('子', "child", "Sublist/substring (pop).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Child,
        [Instruction('部', "part", "Sublist/substring (no pop).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Part,
        [Instruction('昉', "start", "Beginning of list/string (pop).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Start1,
        [Instruction('俶', "start", "Beginning of list/string (no pop).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Start2,
        [Instruction('始', "begin", "Beginning of list/string (counting from end) (pop).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Begin,
        [Instruction('初', "beginning", "Beginning of list/string (counting from end) (no pop).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Beginning,
        [Instruction('末', "final", "End of list/string (pop).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Final,
        [Instruction('尾', "tail", "End of list/string (no pop).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Tail,
        [Instruction('端', "end", "End of list/string (counting from start) (pop).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        EndStr,
        [Instruction('止', "stop", "End of list/string (counting from start) (no pop).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Stop,
        [Instruction('反', "reverse", "Reverses a list or string.", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Reverse,
        [Instruction('捃', "sort", "Sort a list/string by string value.", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Sort,
        [Instruction('訂', "arrange", "Sort a list/string by integer value/codepoint.", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Arrange,
        [Instruction('折', "snap", "Splits list/string before first matching item/character (pop list/string).", NodeType.BlockHead, InstructionGroup.ListStringManipulation)]
        Snap,
        [Instruction('破', "rupture", "Splits list/string before first matching item/character (keep list/string).", NodeType.BlockHead, InstructionGroup.ListStringManipulation)]
        Rupture,
        [Instruction('擘', "break", "Splits list/string after last matching item/character (pop list/string).", NodeType.BlockHead, InstructionGroup.ListStringManipulation)]
        Break,
        [Instruction('断', "sever", "Splits list/string after last matching item/character (keep list/string).", NodeType.BlockHead, InstructionGroup.ListStringManipulation)]
        Sever,
        [Instruction('會', "assemble", "Join a list of strings into a single string with a separator.", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Assemble,
        [Instruction('癲', "mad", "Random list/string (length expected first).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Mad,
        [Instruction('癡', "silly", "Random list/string (length expected last).", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Silly,
        [Instruction('繓', "shuffle", "Shuffles a list or string.", NodeType.SingularNode, InstructionGroup.ListStringManipulation)]
        Shuffle,

        // String-only manipulation
        [Instruction('換', "substitute", "Replace first regular expression match (pop).", NodeType.BlockHead, InstructionGroup.StringManipulation)]
        ReplaceRegexFirstPop,
        [Instruction('代', "replace", "Replace first regular expression match (no pop).", NodeType.BlockHead, InstructionGroup.StringManipulation)]
        ReplaceRegexFirstNoPop,
        [Instruction('替', "replace", "Replace all regular expression matches (pop).", NodeType.BlockHead, InstructionGroup.StringManipulation)]
        ReplaceRegexAllPop,
        [Instruction('更', "replace", "Replace all regular expression matches (no pop).", NodeType.BlockHead, InstructionGroup.StringManipulation)]
        ReplaceRegexAllNoPop,
        [Instruction('取', "take", "Replace first case-sensitive substring match (pop).", NodeType.BlockHead, InstructionGroup.StringManipulation)]
        ReplaceCsSubstrFirstPop,
        [Instruction('挐', "hold", "Replace first case-sensitive substring match (no pop).", NodeType.BlockHead, InstructionGroup.StringManipulation)]
        ReplaceCsSubstrFirstNoPop,
        [Instruction('拿', "take", "Replace all case-sensitive substring matches (pop).", NodeType.BlockHead, InstructionGroup.StringManipulation)]
        ReplaceCsSubstrAllPop,
        [Instruction('拏', "hold", "Replace all case-sensitive substring matches (no pop).", NodeType.BlockHead, InstructionGroup.StringManipulation)]
        ReplaceCsSubstrAllNoPop,
        [Instruction('用', "use", "Replace first case-insensitive substring match (pop).", NodeType.BlockHead, InstructionGroup.StringManipulation)]
        ReplaceCiSubstrFirstPop,
        [Instruction('喫', "eat", "Replace first case-insensitive substring match (no pop).", NodeType.BlockHead, InstructionGroup.StringManipulation)]
        ReplaceCiSubstrFirstNoPop,
        [Instruction('買', "buy", "Replace all case-insensitive substring matches (pop).", NodeType.BlockHead, InstructionGroup.StringManipulation)]
        ReplaceCiSubstrAllPop,
        [Instruction('進', "advance", "Replace all case-insensitive substring matches (no pop).", NodeType.BlockHead, InstructionGroup.StringManipulation)]
        ReplaceCiSubstrAllNoPop,
        [Instruction('現', "appear", "Current regular expression match.", NodeType.SingularNode, InstructionGroup.StringManipulation)]
        Appear,
        [Instruction('坼', "split", "Split string using regular expression (pop).", NodeType.SingularNode, InstructionGroup.StringManipulation)]
        SplitPop,
        [Instruction('裂', "split", "Split string using regular expression (no pop).", NodeType.SingularNode, InstructionGroup.StringManipulation)]
        SplitNoPop,
        [Instruction('講', "explain", "Unicode codepoint for first character in a string.", NodeType.SingularNode, InstructionGroup.StringManipulation)]
        Explain,
        [Instruction('字', "character", "Character from Unicode codepoint.", NodeType.SingularNode, InstructionGroup.StringManipulation)]
        Character,
        [Instruction('移', "change", "Replace all matches of a regular expression.", NodeType.SingularNode, InstructionGroup.StringManipulation)]
        ChangeRegex,
        [Instruction('變', "change", "Replace all occurrences of a substring (case-sensitive).", NodeType.SingularNode, InstructionGroup.StringManipulation)]
        ChangeCs,
        [Instruction('改', "change", "Replace all occurrences of a substring (case-insensitive).", NodeType.SingularNode, InstructionGroup.StringManipulation)]
        ChangeCi,
        [Instruction('壯', "big", "Upper-case.", NodeType.SingularNode, InstructionGroup.StringManipulation)]
        Big,
        [Instruction('微', "tiny", "Lower-case.", NodeType.SingularNode, InstructionGroup.StringManipulation)]
        Tiny,
        [Instruction('題', "title", "Title-case.", NodeType.SingularNode, InstructionGroup.StringManipulation)]
        Title,
        [Instruction('瘋', "crazy", "Random string from a-z, A-Z.", NodeType.SingularNode, InstructionGroup.StringManipulation)]
        Crazy,
        [Instruction('狂', "insane", "Random string from a-z, A-Z, 0-9.", NodeType.SingularNode, InstructionGroup.StringManipulation)]
        Insane,
    }

    enum NodeType
    {
        SingularNode,
        FunctionExecutionNode,
        BlockHead,
        BlockConditionEnd,
        BlockElse,
        BlockEnd
    }

    enum InstructionGroup
    {
        [InstructionGroup("&Block structure")]
        BlockStructure,
        [InstructionGroup("S&tack manipulation")]
        StackManipulation,
        [InstructionGroup("Loops && &Conditionals")]
        LoopsConditionals,
        [InstructionGroup("A&rithmetic")]
        Arithmetic,
        [InstructionGroup("&Logic")]
        Logic,
        [InstructionGroup("&Functions")]
        Functions,
        [InstructionGroup("String &manipulation")]
        StringManipulation,
        [InstructionGroup("List/String mani&pulation")]
        ListStringManipulation
    }

    enum StackOrRegexNodeType
    {
        CopyFromTop,
        CopyFromBottom,
        MoveFromTop,
        MoveFromBottom,
        SwapFromBottom,
        RegexCapture
    }
}
