
namespace EsotericIDE.Languages
{
    partial class Sclipting
    {
        private enum instruction
        {
            // Block structure
            [instruction('不', "no", "Else (pop).", nodeType.BlockElse, instructionGroup.BlockStructure)]
            No,
            [instruction('逆', "opposite", "Else (no pop).", nodeType.BlockElse, instructionGroup.BlockStructure)]
            Opposite,
            [instruction('終', "end", "End of block.", nodeType.BlockEnd, instructionGroup.BlockStructure)]
            End,

            // Stack manipulation
            [instruction('丟', "discard", "Pops an item.", nodeType.SingularNode, instructionGroup.StackManipulation)]
            Discard,

            // Loops and conditionals
            [instruction('是', "yes", "If (pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Yes,
            [instruction('倘', "if", "If (no pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            If,
            [instruction('上', "up", "Integer for loop.", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Up,
            [instruction('下', "down", "Integer for loop (backwards).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Down,
            [instruction('各', "each", "Foreach loop (pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Each,
            [instruction('每', "every", "Foreach loop (no pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Every,
            [instruction('套', "loop", "While loop (pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Loop,
            [instruction('要', "necessity", "While loop (no pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Necessity,
            [instruction('迄', "until", "Until loop (pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Until,
            [instruction('到', "arrive", "Until loop (no pop).", nodeType.BlockHead, instructionGroup.LoopsConditionals)]
            Arrive,

            // Arithmetic
            [instruction('加', "add", "Adds two numbers.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Add,
            [instruction('減', "subtract", "Subtracts two numbers.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Subtract,
            [instruction('乘', "multiply", "Multiplies two numbers.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Multiply,
            [instruction('除', "divide", "Divides two numbers as floats.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            DivideFloat,
            [instruction('分', "divide", "Divides two integers using integer division.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            DivideInt,
            [instruction('剩', "leftovers", "Remainder (modulo).", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Leftovers,
            [instruction('方', "power", "Exponentiation.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Power,
            [instruction('負', "negative", "Negative (unary minus).", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Negative,
            [instruction('對', "correct", "Absolute value.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Correct,
            [instruction('增', "increase", "Increment by one.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Increase,
            [instruction('貶', "decrease", "Decrement by one.", nodeType.SingularNode, instructionGroup.Arithmetic)]
            Decrease,
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

            // List/string manipulation
            [instruction('長', "length", "Returns length of list/string.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Length,
            [instruction('復', "repeat", "Repeats a list, string, or byte array.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Repeat,
            [instruction('標', "mark", "Pushes a mark.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Mark,
            [instruction('併', "combine", "Concatenates everything above the last mark into a string.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            CombineString,
            [instruction('并', "combine", "Places everything above the last mark into a list.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            CombineList,
            [instruction('合', "combine", "Concatenates two lists or strings.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Combine,
            [instruction('掘', "excavate", "Retrieves the nth item/character in a list/string (pop list/string).", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Excavate,
            [instruction('挖', "dig out", "Retrieves the nth item/character in a list/string (keep list/string).", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            DigOut,
            [instruction('掊', "dig", "Retrieves the nth-last item/character in a list/string (pop list/string).", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Dig,
            [instruction('采', "collect", "Retrieves the nth-last item/character in a list/string (keep list/string).", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Collect,
            [instruction('插', "insert", "Replaces the nth item/character in a list/string.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Insert,
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
            [instruction('講', "explain", "Unicode codepoint for first character in a string.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Explain,
            [instruction('字', "character", "Character from Unicode codepoint.", nodeType.SingularNode, instructionGroup.ListStringManipulation)]
            Character,

            // Regular expressions
            [instruction('現', "appear", "Current regular expression match.", nodeType.SingularNode, instructionGroup.Regex)]
            Appear,
            [instruction('坼', "split", "Split string using regular expression (pop).", nodeType.SingularNode, instructionGroup.Regex)]
            SplitPop,
            [instruction('裂', "split", "Split string using regular expression (no pop).", nodeType.SingularNode, instructionGroup.Regex)]
            SplitNoPop,
            [instruction('換', "substitute", "Regular expression replace first (pop).", nodeType.BlockHead, instructionGroup.Regex)]
            ReplaceFirstPop,
            [instruction('代', "replace", "Regular expression replace first (no pop).", nodeType.BlockHead, instructionGroup.Regex)]
            ReplaceFirstNoPop,
            [instruction('替', "replace", "Regular expression replace all (pop).", nodeType.BlockHead, instructionGroup.Regex)]
            ReplaceAllPop,
            [instruction('更', "replace", "Regular expression replace all (no pop).", nodeType.BlockHead, instructionGroup.Regex)]
            ReplaceAllNoPop,
        }

        private enum nodeType
        {
            SingularNode,
            FunctionExecutionNode,
            BlockHead,
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
            [instructionGroup("List/String &manipulation")]
            ListStringManipulation,
            [instructionGroup("Regular e&xpressions")]
            Regex
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
