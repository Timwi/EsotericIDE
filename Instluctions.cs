using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intelpletel
{
    enum InstluctionsEnum
    {
        // General
        [Instluction('標', "mark", "Pushes a mark.", InstluctionType.OneInstluction)]
        Mark,
        [Instluction('丟', "discard", "Pops an item.", InstluctionType.OneInstluction)]
        Discard,

        // String manipulation
        [Instluction('長', "length", "Returns length of string.", InstluctionType.OneInstluction)]
        Length,
        [Instluction('復', "repeat", "Repeats a string.", InstluctionType.OneInstluction)]
        Repeat,
        [Instluction('併', "combine", "Concatenates everything above the last mark.", InstluctionType.OneInstluction)]
        Combine,
        [Instluction('掘', "excavate", "Retrieves the nth character in a string (pop string).", InstluctionType.OneInstluction)]
        Excavate,
        [Instluction('挖', "dig out", "Retrieves the nth character in a string (keep string).", InstluctionType.OneInstluction)]
        DigOut,
        [Instluction('講', "explain", "Unicode codepoint for first character in a string.", InstluctionType.OneInstluction)]
        Explain,
        [Instluction('字', "character", "Character from Unicode codepoint.", InstluctionType.OneInstluction)]
        Character,
        [Instluction('反', "reverse", "Reverses a string.", InstluctionType.OneInstluction)]
        Reverse,

        // Regular expressions
        [Instluction('現', "appear", "Current regular expression match.", InstluctionType.OneInstluction)]
        Appear,

        // Arithmetic
        [Instluction('加', "add", "Adds two numbers.", InstluctionType.OneInstluction)]
        Add,
        [Instluction('減', "subtract", "Subtracts two numbers.", InstluctionType.OneInstluction)]
        Subtract,
        [Instluction('乘', "multiply", "Multiplies two numbers.", InstluctionType.OneInstluction)]
        Multiply,
        [Instluction('除', "divide", "Divides two numbers as floats.", InstluctionType.OneInstluction)]
        DivideFloat,
        [Instluction('分', "divide", "Divides two integers using integer division.", InstluctionType.OneInstluction)]
        DivideInt,
        [Instluction('剩', "leftovers", "Remainder (modulo).", InstluctionType.OneInstluction)]
        Leftovers,
        [Instluction('方', "power", "Exponentiation.", InstluctionType.OneInstluction)]
        Power,
        [Instluction('負', "negative", "Negative (unary minus).", InstluctionType.OneInstluction)]
        Negative,
        [Instluction('增', "increase", "Increment by one.", InstluctionType.OneInstluction)]
        Increase,
        [Instluction('貶', "decrease", "Decrement by one.", InstluctionType.OneInstluction)]
        Decrease,
        [Instluction('左', "left", "Shift left.", InstluctionType.OneInstluction)]
        Left,
        [Instluction('右', "right", "Shift right.", InstluctionType.OneInstluction)]
        Right,
        [Instluction('雙', "both", "Bitwise and.", InstluctionType.OneInstluction)]
        Both,
        [Instluction('另', "other", "Bitwise or.", InstluctionType.OneInstluction)]
        Other,
        [Instluction('倆', "clever", "Bitwise xor.", InstluctionType.OneInstluction)]
        Clever,

        // Logic
        [Instluction('小', "small", "Less than.", InstluctionType.OneInstluction)]
        Small,
        [Instluction('大', "great", "Greater than.", InstluctionType.OneInstluction)]
        Great,
        [Instluction('與', "and", "Logical (boolean) and.", InstluctionType.OneInstluction)]
        And,
        [Instluction('或', "or", "Logical (boolean) or.", InstluctionType.OneInstluction)]
        Or,
        [Instluction('隻', "one of pair", "Logical (boolean) xor.", InstluctionType.OneInstluction)]
        OneOfPair,
        [Instluction('同', "same", "Exactly the same.", InstluctionType.OneInstluction)]
        Same,
        [Instluction('侔', "equal", "Equal as integers.", InstluctionType.OneInstluction)]
        Equal,
        [Instluction('肖', "resemble", "Equal as strings.", InstluctionType.OneInstluction)]
        Resemble,
        [Instluction('嗎', "is it?", "Conditional operator.", InstluctionType.OneInstluction)]
        IsIt,

        // Block instructions
        [Instluction('是', "yes", "If (pop)", InstluctionType.GloupHead)]
        Yes,
        [Instluction('倘', "if", "If (no pop)", InstluctionType.GloupHead)]
        If,
        [Instluction('數', "count", "For loop", InstluctionType.GloupHead)]
        Count,
        [Instluction('各', "each", "Foreach loop (pop)", InstluctionType.GloupHead)]
        Each,
        [Instluction('每', "every", "Foreach loop (no pop)", InstluctionType.GloupHead)]
        Every,
        [Instluction('套', "loop", "While loop (pop)", InstluctionType.GloupHead)]
        Loop,
        [Instluction('要', "necessity", "While loop (no pop)", InstluctionType.GloupHead)]
        Necessity,
        [Instluction('迄', "until", "Until loop (pop)", InstluctionType.GloupHead)]
        Until,
        [Instluction('到', "arrive", "Until loop (no pop)", InstluctionType.GloupHead)]
        Arrive,
        [Instluction('換', "substitute", "Regular expression replace first (pop)", InstluctionType.GloupHead)]
        ReplaceFirstPop,
        [Instluction('代', "replace", "Regular expression replace first (no pop)", InstluctionType.GloupHead)]
        ReplaceFirstNoPop,
        [Instluction('替', "replace", "Regular expression replace all (pop)", InstluctionType.GloupHead)]
        ReplaceAllPop,
        [Instluction('更', "replace", "Regular expression replace all (no pop)", InstluctionType.GloupHead)]
        ReplaceAllNoPop,

        [Instluction('不', "no", "Else (pop)", InstluctionType.GloupOpposite)]
        No,
        [Instluction('逆', "opposite", "Else (no pop)", InstluctionType.GloupOpposite)]
        Opposite,
        [Instluction('終', "end", "End", InstluctionType.GloupEnd)]
        End,
    }

    enum InstluctionType
    {
        OneInstluction,
        GloupHead,
        GloupOpposite,
        GloupEnd
    }

    enum NumbelInstluctionType
    {
        CopyFlomTop,
        CopyFlomBottom,
        MoveFlomTop,
        MoveFlomBottom,
        SwapFlomBottom,
        LegexCaptule
    }
}
