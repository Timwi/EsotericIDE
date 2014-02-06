
namespace EsotericIDE.Languages
{
    partial class Sclipting
    {
        private enum instruction
        {
            // Block structure
            [instruction('況', "condition", "End of condition block.", nodeType.BlockConditionEnd, instructionGroup.BlockStructure)]
            Condition,
            [instruction('不', "no", "Else (pop).", nodeType.BlockElse, instructionGroup.BlockStructure)]
            No,
            [instruction('逆', "opposite", "Else (no pop).", nodeType.BlockElse, instructionGroup.BlockStructure)]
            Opposite,
            [instruction('終', "end", "End of block.", nodeType.BlockEnd, instructionGroup.BlockStructure)]
            End,

            // Stack manipulation
            [instruction('丟', "discard", "Pops an item.", nodeType.SingularNode, instructionGroup.StackManipulation)]
            Discard,
            [instruction('棄', "abandon", "Pops two items.", nodeType.SingularNode, instructionGroup.StackManipulation)]
            Abandon,

            // Loops and conditionals
            [instruction('是', "yes", "If true (pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Yes,
            [instruction('倘', "if", "If true (no pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            If,
            [instruction('沒', "not", "If false (pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            NotPop,
            [instruction('毋', "not", "If false (no pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            NotNoPop,
            [instruction('夠', "enough", "If non-empty (pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Enough,
            [instruction('含', "contain", "If non-empty (no pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Contain,
            [instruction('套', "loop", "While true (pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Loop,
            [instruction('要', "necessity", "While true (no pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Necessity,
            [instruction('迄', "until", "While false (pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Until,
            [instruction('到', "arrive", "While false (no pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Arrive,
            [instruction('滿', "full", "While non-empty (pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Full,
            [instruction('充', "be full", "While non-empty (no pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            BeFull,
            [instruction('上', "up", "Integer for loop.", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Up,
            [instruction('下', "down", "Integer for loop (backwards).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Down,
            [instruction('各', "each", "Foreach loop (pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Each,
            [instruction('每', "every", "Foreach loop (no pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Every,

            // Arithmetic
            [instruction('加', "add", "Adds two numbers.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Add,
            [instruction('減', "subtract", "Subtracts two numbers.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Subtract,
            [instruction('縮', "reduce", "Subtracts two numbers (reverse order).", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Reduce,
            [instruction('乘', "multiply", "Multiplies two numbers.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Multiply,
            [instruction('除', "divide", "Divides two numbers as floats.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            DivideFloat,
            [instruction('分', "divide", "Divides two integers using integer division.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            DivideInt,
            [instruction('剩', "leftovers", "Remainder (modulo).", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Leftovers,
            [instruction('重', "double", "Doubles a number.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Double,
            [instruction('半', "half", "Halves a number (result is float).", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Half,
            [instruction('隔', "separate", "Halves an integer (equivalent to shift-right 1).", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Separate,
            [instruction('方', "power", "Exponentiation.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Power,
            [instruction('平', "flat", "Square.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Flat,
            [instruction('根', "root", "Square root.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Root,
            [instruction('負', "negative", "Negative (unary minus).", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Negative,
            [instruction('對', "correct", "Absolute value.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Correct,
            [instruction('增', "increase", "Increment by one.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Increase,
            [instruction('貶', "decrease", "Decrement by one.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Decrease,
            [instruction('數', "number", "Logarithm to base e.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Number,
            [instruction('位', "position", "Logarithm to base 10.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Position,
            [instruction('級', "level", "Logarithm to base 2.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Level,
            [instruction('圜', "circle", "Rounds towards 0.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Circle1,
            [instruction('圍', "circle", "Rounds away from 0.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Circle2,
            [instruction('團', "sphere", "Rounds down.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Sphere,
            [instruction('圓', "surround", "Rounds up.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Surround1,
            [instruction('繞', "surround", "Rounds to the nearest integer (halves away from zero).", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Surround2,
            [instruction('輪', "wheel", "Rounds to the nearest integer (halves to even).", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Wheel,

            // Arithmetic (bitwise)
            [instruction('左', "left", "Shift left.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Left,
            [instruction('右', "right", "Shift right.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Right,
            [instruction('雙', "both", "Bitwise and.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Both,
            [instruction('另', "other", "Bitwise or.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Other,
            [instruction('倆', "clever", "Bitwise xor.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Clever,
            [instruction('無', "not", "Bitwise not.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            BitwiseNot,
            [instruction('啃', "gnaw", "Split numbers into bits.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Gnaw,
            [instruction('嚙', "bite", "Split numbers into bits.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Bite,
            // Arithmetic (random)
            [instruction('沌', "chaotic", "Random integer between 0 and 2³².", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Chaotic,
            [instruction('紛', "disarray", "Random integer between 0 and specified maximum.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Disarray,
            [instruction('胡', "wild", "Random integer within specified bounds.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Wild1,
            [instruction('亂', "chaos", "Random float between 0 and 1.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Chaos,
            [instruction('野', "wild", "Random float between 0 and specified maximum.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Wild2,
            [instruction('猖', "wild", "Random float within specified bounds.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Wild3,

            // Logic
            [instruction('小', "small", "Less than.", nodeType.SingularNode, instructionGroup.Logic)]
            Small,
            [instruction('大', "great", "Greater than.", nodeType.SingularNode, instructionGroup.Logic)]
            Great,
            [instruction('少', "less", "Less than or equal to.", nodeType.SingularNode, instructionGroup.Logic)]
            Less,
            [instruction('瀰', "overflow", "Greater than or equal to.", nodeType.SingularNode, instructionGroup.Logic)]
            Overflow,
            [instruction('同', "same", "Exactly the same.", nodeType.SingularNode, instructionGroup.Logic)]
            Same,
            [instruction('侔', "equal", "Equal as integers.", nodeType.SingularNode, instructionGroup.Logic)]
            Equal,
            [instruction('肖', "resemble", "Equal as strings.", nodeType.SingularNode, instructionGroup.Logic)]
            Resemble,
            [instruction('差', "different", "Not exactly the same.", nodeType.SingularNode, instructionGroup.Logic)]
            Different1,
            [instruction('异', "different", "Not equal as integers.", nodeType.SingularNode, instructionGroup.Logic)]
            Different2,
            [instruction('殊', "different", "Not equal as strings.", nodeType.SingularNode, instructionGroup.Logic)]
            Different3,
            [instruction('與', "and", "Logical (boolean) and.", nodeType.SingularNode, instructionGroup.Logic)]
            And,
            [instruction('或', "or", "Logical (boolean) or.", nodeType.SingularNode, instructionGroup.Logic)]
            Or,
            [instruction('隻', "one of pair", "Logical (boolean) xor.", nodeType.SingularNode, instructionGroup.Logic)]
            OneOfPair,
            [instruction('非', "not", "Logical (boolean) not.", nodeType.SingularNode, instructionGroup.Logic)]
            Not,
            [instruction('嗎', "is it?", "Conditional operator.", nodeType.SingularNode, instructionGroup.Logic)]
            IsIt,

            // Lambda function instructions
            [instruction('塊', "block", "Introduces an anonymous function (lambda expression).", nodeType.BlockHead, instructionGroup.Functions)]
            Block,
            [instruction('掳', "capture", "Introduces an anonymous function (lambda expression) that captures one item.", nodeType.BlockHead, instructionGroup.Functions)]
            Capture,
            [instruction('開', "initiate", "Pops and executes a function (lambda).", nodeType.FunctionExecutionNode, instructionGroup.Functions)]
            Initiate,
            [instruction('辦', "handle", "Pops a function (lambda), executes it, and then pushes it back on when it returns.", nodeType.FunctionExecutionNode, instructionGroup.Functions)]
            Handle,
            [instruction('演', "perform", "Executes a function (lambda) without popping it.", nodeType.FunctionExecutionNode, instructionGroup.Functions)]
            Perform,

            // List/string manipulation (indexed list/string instruction equivalents)
            [instruction('掘', "excavate", "Retrieves the nth item/character in a list/string (pop).", nodeType.SingularNode, instructionGroup.ListStringManipulation), singularListStringInstruction(ListStringInstruction.RetrievePop, false)]
            Excavate,
            [instruction('掊', "dig", "Retrieves the nth-last item/character in a list/string (pop).", nodeType.SingularNode, instructionGroup.ListStringManipulation), singularListStringInstruction(ListStringInstruction.RetrievePop, true)]
            Dig,
            [instruction('挖', "dig out", "Retrieves the nth item/character in a list/string (no pop).", nodeType.SingularNode, instructionGroup.ListStringManipulation), singularListStringInstruction(ListStringInstruction.RetrieveNoPop, false)]
            DigOut,
            [instruction('采', "collect", "Retrieves the nth-last item/character in a list/string (no pop).", nodeType.SingularNode, instructionGroup.ListStringManipulation), singularListStringInstruction(ListStringInstruction.RetrieveNoPop, true)]
            Collect,
            [instruction('栽', "cultivate", "Inserts an item/character into a list/string at a specified index.", nodeType.SingularNode, instructionGroup.ListStringManipulation), singularListStringInstruction(ListStringInstruction.Insert, false)]
            Cultivate,
            [instruction('種', "seed", "Inserts an item/character into a list/string at a specified reverse index.", nodeType.SingularNode, instructionGroup.ListStringManipulation), singularListStringInstruction(ListStringInstruction.Insert, true)]
            Seed,
            [instruction('殲', "annihilate", "Deletes the nth item/character in a list/string.", nodeType.SingularNode, instructionGroup.ListStringManipulation), singularListStringInstruction(ListStringInstruction.Delete, false)]
            Annihilate,
            [instruction('摧', "destroy", "Deletes the nth-last item/character in a list/string.", nodeType.SingularNode, instructionGroup.ListStringManipulation), singularListStringInstruction(ListStringInstruction.Delete, true)]
            Destroy,
            [instruction('裒', "take out", "Retrieves and deletes the nth item/character in a list/string.", nodeType.SingularNode, instructionGroup.ListStringManipulation), singularListStringInstruction(ListStringInstruction.RetrieveDelete, false)]
            TakeOut,
            [instruction('抽', "pull out", "Retrieves and deletes the nth-last item/character in a list/string.", nodeType.SingularNode, instructionGroup.ListStringManipulation), singularListStringInstruction(ListStringInstruction.RetrieveDelete, true)]
            PullOut,
            [instruction('插', "insert", "Replaces the nth item/character in a list/string.", nodeType.SingularNode, instructionGroup.ListStringManipulation), singularListStringInstruction(ListStringInstruction.Replace, false)]
            Insert,
            [instruction('恢', "restore", "Replaces the nth-last item/character in a list/string.", nodeType.SingularNode, instructionGroup.ListStringManipulation), singularListStringInstruction(ListStringInstruction.Replace, true)]
            Restore,
            [instruction('混', "mix", "Exchanges the nth item/character in a list/string with the value on the stack.", nodeType.SingularNode, instructionGroup.ListStringManipulation), singularListStringInstruction(ListStringInstruction.Exchange, false)]
            MixForward,
            [instruction('拌', "mix", "Exchanges the nth-last item/character in a list/string with the value on the stack.", nodeType.SingularNode, instructionGroup.ListStringManipulation), singularListStringInstruction(ListStringInstruction.Exchange, true)]
            MixBackward,

            // List/string manipulation (other)
            [instruction('匱', "lack", "Pushes the empty list.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Lack,
            [instruction('虛', "empty", "Pushes the empty string.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Empty,
            [instruction('長', "length", "Returns length of list/string (pop).", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Length,
            [instruction('梴', "long", "Returns length of list/string (no pop).", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Long,
            [instruction('復', "repeat", "Repeats a list, string, or byte array (list/string expected first).", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Repeat,
            [instruction('伸', "extend", "Repeats a list, string, or byte array (amount expected first).", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Extend,
            [instruction('疊', "repeat", "Creates a list of repetitions of an item (item expected first).", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            RepeatIntoList,
            [instruction('張', "stretch", "Creates a list of repetitions of an item (amount expected first).", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Stretch,
            [instruction('標', "mark", "Pushes a mark.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Mark,
            [instruction('併', "combine", "Concatenates everything above the last mark into a string.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            CombineString,
            [instruction('并', "combine", "Places everything above the last mark into a list.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            CombineList,
            [instruction('合', "combine", "Concatenates two lists or strings.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Combine,
            [instruction('融', "blend", "Concatenates two lists or strings (reverse order).", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Blend,
            [instruction('反', "reverse", "Reverses a list or string.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Reverse,
            [instruction('捃', "sort", "Sort a list/string by string value.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Sort,
            [instruction('訂', "arrange", "Sort a list/string by integer value/codepoint.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Arrange,
            [instruction('折', "snap", "Splits list/string before first matching item/character (pop list/string).", nodeType.BlockHead, instructionGroup.ListStringManipulation)]
            Snap,
            [instruction('破', "rupture", "Splits list/string before first matching item/character (keep list/string).", nodeType.BlockHead, instructionGroup.ListStringManipulation)]
            Rupture,
            [instruction('擘', "break", "Splits list/string after last matching item/character (pop list/string).", nodeType.BlockHead, instructionGroup.ListStringManipulation)]
            Break,
            [instruction('断', "sever", "Splits list/string after last matching item/character (keep list/string).", nodeType.BlockHead, instructionGroup.ListStringManipulation)]
            Sever,
            [instruction('會', "assemble", "Join a list of strings into a single string with a separator.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Assemble,
            [instruction('癲', "mad", "Random list/string (length expected first).", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Mad,
            [instruction('癡', "silly", "Random list/string (length expected last).", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Silly,
            [instruction('繓', "shuffle", "Shuffles a list or string.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Shuffle,

            // String-only manipulation
            [instruction('換', "substitute", "Replace first regular expression match (pop).", nodeType.BlockHead, instructionGroup.StringManipulation)]
            ReplaceRegexFirstPop,
            [instruction('代', "replace", "Replace first regular expression match (no pop).", nodeType.BlockHead, instructionGroup.StringManipulation)]
            ReplaceRegexFirstNoPop,
            [instruction('替', "replace", "Replace all regular expression matches (pop).", nodeType.BlockHead, instructionGroup.StringManipulation)]
            ReplaceRegexAllPop,
            [instruction('更', "replace", "Replace all regular expression matches (no pop).", nodeType.BlockHead, instructionGroup.StringManipulation)]
            ReplaceRegexAllNoPop,
            [instruction('取', "take", "Replace first case-sensitive substring match (pop).", nodeType.BlockHead, instructionGroup.StringManipulation)]
            ReplaceCsSubstrFirstPop,
            [instruction('挐', "hold", "Replace first case-sensitive substring match (no pop).", nodeType.BlockHead, instructionGroup.StringManipulation)]
            ReplaceCsSubstrFirstNoPop,
            [instruction('拿', "take", "Replace all case-sensitive substring matches (pop).", nodeType.BlockHead, instructionGroup.StringManipulation)]
            ReplaceCsSubstrAllPop,
            [instruction('拏', "hold", "Replace all case-sensitive substring matches (no pop).", nodeType.BlockHead, instructionGroup.StringManipulation)]
            ReplaceCsSubstrAllNoPop,
            [instruction('用', "use", "Replace first case-insensitive substring match (pop).", nodeType.BlockHead, instructionGroup.StringManipulation)]
            ReplaceCiSubstrFirstPop,
            [instruction('喫', "eat", "Replace first case-insensitive substring match (no pop).", nodeType.BlockHead, instructionGroup.StringManipulation)]
            ReplaceCiSubstrFirstNoPop,
            [instruction('買', "buy", "Replace all case-insensitive substring matches (pop).", nodeType.BlockHead, instructionGroup.StringManipulation)]
            ReplaceCiSubstrAllPop,
            [instruction('進', "advance", "Replace all case-insensitive substring matches (no pop).", nodeType.BlockHead, instructionGroup.StringManipulation)]
            ReplaceCiSubstrAllNoPop,
            [instruction('現', "appear", "Current regular expression match.", nodeType.SingularNode, instructionGroup.StringManipulation)]
            Appear,
            [instruction('坼', "split", "Split string using regular expression (pop).", nodeType.SingularNode, instructionGroup.StringManipulation)]
            SplitPop,
            [instruction('裂', "split", "Split string using regular expression (no pop).", nodeType.SingularNode, instructionGroup.StringManipulation)]
            SplitNoPop,
            [instruction('講', "explain", "Unicode codepoint for first character in a string.", nodeType.SingularNode, instructionGroup.StringManipulation)]
            Explain,
            [instruction('字', "character", "Character from Unicode codepoint.", nodeType.SingularNode, instructionGroup.StringManipulation)]
            Character,
            [instruction('移', "change", "Replace all matches of a regular expression.", nodeType.SingularNode, instructionGroup.StringManipulation)]
            ChangeRegex,
            [instruction('變', "change", "Replace all occurrences of a substring (case-sensitive).", nodeType.SingularNode, instructionGroup.StringManipulation)]
            ChangeCs,
            [instruction('改', "change", "Replace all occurrences of a substring (case-insensitive).", nodeType.SingularNode, instructionGroup.StringManipulation)]
            ChangeCi,
            [instruction('壯', "big", "Upper-case.", nodeType.SingularNode, instructionGroup.StringManipulation)]
            Big,
            [instruction('微', "tiny", "Lower-case.", nodeType.SingularNode, instructionGroup.StringManipulation)]
            Tiny,
            [instruction('題', "title", "Title-case.", nodeType.SingularNode, instructionGroup.StringManipulation)]
            Title,
            [instruction('瘋', "crazy", "Random string from a-z, A-Z.", nodeType.SingularNode, instructionGroup.StringManipulation)]
            Crazy,
            [instruction('狂', "insane", "Random string from a-z, A-Z, 0-9.", nodeType.SingularNode, instructionGroup.StringManipulation)]
            Insane,
        }

        private enum nodeType
        {
            SingularNode,
            FunctionExecutionNode,
            BlockHead,
            BlockConditionEnd,
            BlockElse,
            BlockEnd
        }

        private enum instructionGroup
        {
            [instructionGroup("&Block structure")]
            BlockStructure,
            [instructionGroup("S&tack manipulation")]
            StackManipulation,
            [instructionGroup("Loops && &Conditionals")]
            LoopsConditionals,
            [instructionGroup("A&rithmetic")]
            Arithmetic,
            [instructionGroup("&Logic")]
            Logic,
            [instructionGroup("&Functions")]
            Functions,
            [instructionGroup("String &manipulation")]
            StringManipulation,
            [instructionGroup("List/String mani&pulation")]
            ListStringManipulation
        }

        private enum stackOrRegexNodeType
        {
            CopyFromTop,
            CopyFromBottom,
            MoveFromTop,
            MoveFromBottom,
            SwapFromBottom,
            RegexCapture
        }
    }
}
