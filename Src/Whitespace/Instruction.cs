namespace EsotericIDE.Whitespace
{
    enum Instruction
    {
        // Stack Manipulation
        [Instruction("Push the number {0} onto the stack", "  ", ArgKind.Number)]
        Push,
        [Instruction("Duplicate the top item on the stack.", " \n ")]
        Dup,
        [Instruction("Copy the {0}{1} item on the stack onto the top of the stack.", " \t ", ArgKind.Number)]
        Copy,
        [Instruction("Swap the top two items on the stack.", " \n\t")]
        Swap,
        [Instruction("Discard the top item on the stack.", " \n\n")]
        Discard,
        [Instruction("Slide {0} items off the stack, keeping the top item.", " \t\n", ArgKind.Number)]
        Slide,

        // Arithmetic
        [Instruction("Addition.", "\t   ")]
        Add,
        [Instruction("Subtraction.", "\t  \t")]
        Subtract,
        [Instruction("Multiplication.", "\t  \n")]
        Multiply,
        [Instruction("Integer division.", "\t \t ")]
        Div,
        [Instruction("Modulo.", "\t \t\t")]
        Modulo,

        // Heap Access
        [Instruction("Store a value on the heap.", "\t\t ")]
        Store,
        [Instruction("Retrieve a value from the heap.", "\t\t\t")]
        Retrieve,

        // Flow Control
        [Instruction("Label {2}.", "\n  ", ArgKind.Label)]
        Mark,
        [Instruction("Call subroutine at label {2}.", "\n \t", ArgKind.Label)]
        Call,
        [Instruction("Jump unconditionally to label {2}.", "\n \n", ArgKind.Label)]
        Jump,
        [Instruction("Jump to label {2} if top of stack is zero.", "\n\t ", ArgKind.Label)]
        JumpIfZero,
        [Instruction("Jump to label {2} if the top of the stack is negative.", "\n\t\t", ArgKind.Label)]
        JumpIfNeg,
        [Instruction("End a subroutine and transfer control back to the caller.", "\n\t\n")]
        Return,
        [Instruction("End the program.", "\n\n\n")]
        Exit,

        // I/O
        [Instruction("Output the character at the top of the stack.", "\t\n  ")]
        OutputChar,
        [Instruction("Output the number at the top of the stack.", "\t\n \t")]
        OutputNumber,
        [Instruction("Read a character and place it in the location given by the top of the stack.", "\t\n\t ")]
        ReadChar,
        [Instruction("Read a number and place it in the location given by the top of the stack.", "\t\n\t\t")]
        ReadNumber
    }
}
