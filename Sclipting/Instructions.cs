
namespace EsotericIDE.Languages
{
    partial class Sclipting
    {
        private enum instruction
        {
            // General
            [instruction('標', "mark", "Pushes a mark.", nodeType.SingularNode)]
            Mark,
            [instruction('丟', "discard", "Pops an item.", nodeType.SingularNode)]
            Discard,

            // String or list manipulation
            [instruction('長', "length", "Returns length of list/string.", nodeType.SingularNode)]
            Length,
            [instruction('復', "repeat", "Repeats a list, string, or byte array.", nodeType.SingularNode)]
            Repeat,
            [instruction('併', "combine", "Concatenates everything above the last mark into a string.", nodeType.SingularNode)]
            CombineString,
            [instruction('并', "combine", "Places everything above the last mark in a list.", nodeType.SingularNode)]
            CombineList,
            [instruction('掘', "excavate", "Retrieves the nth item/character in a list/string (pop list/string).", nodeType.SingularNode)]
            Excavate,
            [instruction('挖', "dig out", "Retrieves the nth item/character in a list/string (keep list/string).", nodeType.SingularNode)]
            DigOut,
            [instruction('插', "insert", "Replaces the nth item/character in a list/string.", nodeType.SingularNode)]
            Insert,
            [instruction('反', "reverse", "Reverses a string.", nodeType.SingularNode)]
            Reverse,
            [instruction('捃', "sort", "Sort a list/string by string value.", nodeType.SingularNode)]
            Sort,
            [instruction('訂', "arrange", "Sort a list/string by integer value/codepoint.", nodeType.SingularNode)]
            Arrange,

            // String manipulation only
            [instruction('講', "explain", "Unicode codepoint for first character in a string.", nodeType.SingularNode)]
            Explain,
            [instruction('字', "character", "Character from Unicode codepoint.", nodeType.SingularNode)]
            Character,

            // Regular expressions
            [instruction('現', "appear", "Current regular expression match.", nodeType.SingularNode)]
            Appear,
            [instruction('坼', "split", "Split string using regular expression (pop).", nodeType.SingularNode)]
            SplitPop,
            [instruction('裂', "split", "Split string using regular expression (no pop).", nodeType.SingularNode)]
            SplitNoPop,

            // Arithmetic
            [instruction('加', "add", "Adds two numbers.", nodeType.SingularNode)]
            Add,
            [instruction('減', "subtract", "Subtracts two numbers.", nodeType.SingularNode)]
            Subtract,
            [instruction('乘', "multiply", "Multiplies two numbers.", nodeType.SingularNode)]
            Multiply,
            [instruction('除', "divide", "Divides two numbers as floats.", nodeType.SingularNode)]
            DivideFloat,
            [instruction('分', "divide", "Divides two integers using integer division.", nodeType.SingularNode)]
            DivideInt,
            [instruction('剩', "leftovers", "Remainder (modulo).", nodeType.SingularNode)]
            Leftovers,
            [instruction('方', "power", "Exponentiation.", nodeType.SingularNode)]
            Power,
            [instruction('負', "negative", "Negative (unary minus).", nodeType.SingularNode)]
            Negative,
            [instruction('對', "correct", "Absolute value.", nodeType.SingularNode)]
            Correct,
            [instruction('增', "increase", "Increment by one.", nodeType.SingularNode)]
            Increase,
            [instruction('貶', "decrease", "Decrement by one.", nodeType.SingularNode)]
            Decrease,
            [instruction('左', "left", "Shift left.", nodeType.SingularNode)]
            Left,
            [instruction('右', "right", "Shift right.", nodeType.SingularNode)]
            Right,
            [instruction('雙', "both", "Bitwise and.", nodeType.SingularNode)]
            Both,
            [instruction('另', "other", "Bitwise or.", nodeType.SingularNode)]
            Other,
            [instruction('倆', "clever", "Bitwise xor.", nodeType.SingularNode)]
            Clever,

            // Logic
            [instruction('小', "small", "Less than.", nodeType.SingularNode)]
            Small,
            [instruction('大', "great", "Greater than.", nodeType.SingularNode)]
            Great,
            [instruction('與', "and", "Logical (boolean) and.", nodeType.SingularNode)]
            And,
            [instruction('或', "or", "Logical (boolean) or.", nodeType.SingularNode)]
            Or,
            [instruction('隻', "one of pair", "Logical (boolean) xor.", nodeType.SingularNode)]
            OneOfPair,
            [instruction('同', "same", "Exactly the same.", nodeType.SingularNode)]
            Same,
            [instruction('侔', "equal", "Equal as integers.", nodeType.SingularNode)]
            Equal,
            [instruction('肖', "resemble", "Equal as strings.", nodeType.SingularNode)]
            Resemble,
            [instruction('嗎', "is it?", "Conditional operator.", nodeType.SingularNode)]
            IsIt,

            // Function execution
            [instruction('開', "initiate", "Pops and executes a function (lambda).", nodeType.FunctionExecutionNode)]
            Initiate,
            [instruction('辦', "handle", "Pops a function (lambda), executes it, and then pushes it back on when it returns.", nodeType.FunctionExecutionNode)]
            Handle,
            [instruction('演', "perform", "Executes a function (lambda) without popping it.", nodeType.FunctionExecutionNode)]
            Perform,

            // Block instructions
            [instruction('是', "yes", "If (pop)", nodeType.BlockHead)]
            Yes,
            [instruction('倘', "if", "If (no pop)", nodeType.BlockHead)]
            If,
            [instruction('數', "count", "For loop", nodeType.BlockHead)]
            Count,
            [instruction('各', "each", "Foreach loop (pop)", nodeType.BlockHead)]
            Each,
            [instruction('每', "every", "Foreach loop (no pop)", nodeType.BlockHead)]
            Every,
            [instruction('套', "loop", "While loop (pop)", nodeType.BlockHead)]
            Loop,
            [instruction('要', "necessity", "While loop (no pop)", nodeType.BlockHead)]
            Necessity,
            [instruction('迄', "until", "Until loop (pop)", nodeType.BlockHead)]
            Until,
            [instruction('到', "arrive", "Until loop (no pop)", nodeType.BlockHead)]
            Arrive,
            [instruction('換', "substitute", "Regular expression replace first (pop)", nodeType.BlockHead)]
            ReplaceFirstPop,
            [instruction('代', "replace", "Regular expression replace first (no pop)", nodeType.BlockHead)]
            ReplaceFirstNoPop,
            [instruction('替', "replace", "Regular expression replace all (pop)", nodeType.BlockHead)]
            ReplaceAllPop,
            [instruction('更', "replace", "Regular expression replace all (no pop)", nodeType.BlockHead)]
            ReplaceAllNoPop,
            [instruction('塊', "block", "Introduces an anonymous function (lambda expression).", nodeType.BlockHead)]
            Block,
            [instruction('掳', "capture", "Introduces an anonymous function (lambda expression) that captures one item.", nodeType.BlockHead)]
            Capture,

            [instruction('不', "no", "Else (pop)", nodeType.BlockElse)]
            No,
            [instruction('逆', "opposite", "Else (no pop)", nodeType.BlockElse)]
            Opposite,
            [instruction('終', "end", "End", nodeType.BlockEnd)]
            End,
        }

        private enum nodeType
        {
            SingularNode,
            FunctionExecutionNode,
            BlockHead,
            BlockElse,
            BlockEnd
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
