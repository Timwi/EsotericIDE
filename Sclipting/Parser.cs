using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Sclipting
{
    static class Parser
    {
        public static List<Node> Parse(string source, int addIndex)
        {
            Instruction instruction;
            NodeType type;
            ListStringInstruction listStringType;
            bool backwards;

            var ret = new List<Node>();
            int index = 0;
            while (index < source.Length)
            {
                var ch = source[index];
                if (ch < 0x0100)
                {
                    // can wlite comments using infeliol Amelican and Eulopean chalactels
                    index++;
                }
                else if (ch >= 0xac00 && ch < 0xbc00)
                {
                    // Beginning of byte allay
                    var origIndex = index;
                    var hangeul = ParseByteArrayToken(source, index);
                    index += hangeul.Length;
                    try
                    {
                        ret.Add(new ByteArray { Array = ScliptingUtil.DecodeByteArray(hangeul), Index = origIndex + addIndex, Count = index - origIndex });
                    }
                    catch (CompileException ce)
                    {
                        throw new CompileException(ce.Message, ce.Index + origIndex + addIndex, ce.Length);
                    }
                }
                else if (ch >= 0xbc00 && ch <= 0xd7a3)
                    ret.Add(new NegativeNumber { Number = 0xbbff - ch, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '①' && ch <= '⑳')
                    ret.Add(new StackOrRegexNode { Type = StackOrRegexNodeType.CopyFromBottom, Value = ch - '①' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '㉑' && ch <= '㉟')
                    ret.Add(new StackOrRegexNode { Type = StackOrRegexNodeType.CopyFromBottom, Value = ch - '㉑' + 21, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '㊱' && ch <= '㊿')
                    ret.Add(new StackOrRegexNode { Type = StackOrRegexNodeType.CopyFromBottom, Value = ch - '㊱' + 36, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⓵' && ch <= '⓾')
                    ret.Add(new StackOrRegexNode { Type = StackOrRegexNodeType.MoveFromTop, Value = ch - '⓵' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '❶' && ch <= '❿')
                    ret.Add(new StackOrRegexNode { Type = StackOrRegexNodeType.CopyFromTop, Value = ch - '❶' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⓫' && ch <= '⓴')
                    ret.Add(new StackOrRegexNode { Type = StackOrRegexNodeType.CopyFromTop, Value = ch - '⓫' + 11, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⑴' && ch <= '⒇')
                    ret.Add(new StackOrRegexNode { Type = StackOrRegexNodeType.MoveFromBottom, Value = ch - '⑴' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⒈' && ch <= '⒛')
                    ret.Add(new StackOrRegexNode { Type = StackOrRegexNodeType.SwapFromBottom, Value = ch - '⒈' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= 'Ⓐ' && ch <= 'Ⓩ')
                    ret.Add(new StackOrRegexNode { Type = StackOrRegexNodeType.RegexCapture, Value = ch - 'Ⓐ' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ListStringElementNode.Characters.Contains(ch))
                    ret.Add(new ListStringElementNode(ch, index++ + addIndex));
                else if (ScliptingUtil.GetInstructionInfo(ch, out listStringType, out backwards))
                    ret.Add(new ListStringElementNode(listStringType, backwards, index++ + addIndex));
                else if (ScliptingUtil.GetInstructionInfo(ch, out instruction, out type))
                {
                    switch (type)
                    {
                        case NodeType.SingularNode:
                            ret.Add(new SingularNode { ThisInstruction = instruction, Index = index++ + addIndex, Count = 1 });
                            break;

                        case NodeType.FunctionExecutionNode:
                            ret.Add(new ExecuteFunction { Instruction = instruction, Index = index++ + addIndex, Count = 1 });
                            break;

                        case NodeType.BlockHead:
                            int? conditionEndIndex;
                            int? elseIndex;
                            bool elsePops;
                            int endIndex;
                            findMatchingEnd(source, index, addIndex, out conditionEndIndex, out elseIndex, out elsePops, out endIndex);
                            var primaryBlock = Parse(source.Substring(index + 1, (conditionEndIndex ?? elseIndex ?? endIndex) - index - 1), index + 1 + addIndex);
                            List<Node> conditionBlock = null;
                            if (conditionEndIndex != null)
                            {
                                conditionBlock = primaryBlock;
                                primaryBlock = Parse(source.Substring(conditionEndIndex.Value + 1, (elseIndex ?? endIndex) - conditionEndIndex.Value - 1), conditionEndIndex.Value + 1 + addIndex);
                            }
                            var elseBlock = elseIndex == null ? null : Parse(source.Substring(elseIndex.Value + 1, endIndex - elseIndex.Value - 1), elseIndex.Value + 1 + addIndex);
                            var blockInstr = BlockNode.Create(ch, instruction, index, addIndex, conditionEndIndex, elseIndex, elsePops);
                            blockInstr.PrimaryBlock = primaryBlock;
                            blockInstr.ConditionBlock = conditionBlock;
                            blockInstr.ConditionEndIndex = (conditionEndIndex ?? 0) + addIndex;
                            blockInstr.ElseBlock = elseBlock;
                            blockInstr.ElseBlockPops = elsePops;
                            blockInstr.Index = index + addIndex;
                            blockInstr.Count = endIndex - index + 1;
                            blockInstr.ElseIndex = (elseIndex ?? 0) + addIndex;
                            ret.Add(blockInstr);
                            index = endIndex + 1;
                            break;

                        case NodeType.BlockElse:
                        case NodeType.BlockEnd:
                            throw new CompileException("Else or end instruction encountered without a matching block head instruction.", index + addIndex, 1);

                        default:
                            throw new CompileException("Unrecognised instruction: “{0}”.".Fmt(ch), index + addIndex, 1);
                    }
                }
                else
                    throw new CompileException("Unrecognised instruction: “{0}”.".Fmt(ch), index + addIndex, 1);
            }
            return ret;
        }

        public static string ParseByteArrayToken(string source, int index)
        {
            var origIndex = index;
            var ch = source[index];
            if (ch < 0xac00 || ch >= 0xbc00)
                throw new InternalErrorException("A byte array token does not start here.");
            do
            {
                index++;
                if (index == source.Length)
                    goto done;
                ch = source[index];
            }
            while (ch >= 0xac00 && ch < 0xbc00);
            if ((index - origIndex) % 2 == 1 && ch >= 0xbc00 && ch < 0xbc10)
                index++;
            done:
            return source.Substring(origIndex, index - origIndex);
        }

        private static void findMatchingEnd(string source, int start, int addIndex, out int? conditionEndIndex, out int? elseIndex, out bool elsePops, out int endIndex)
        {
            conditionEndIndex = null;
            elseIndex = null;
            elsePops = false;
            var depth = 0;
            Instruction instruction;
            NodeType type;
            for (int i = start; i < source.Length; i++)
            {
                if (ScliptingUtil.GetInstructionInfo(source[i], out instruction, out type))
                {
                    if (type == NodeType.BlockHead)
                        depth++;
                    else if (type == NodeType.BlockConditionEnd && depth == 1)
                    {
                        if (conditionEndIndex != null)
                            throw new CompileException("“{0}” block cannot have multiple condition-block instructions.".Fmt(source[start]), start + addIndex, i - start + addIndex + 1);
                        conditionEndIndex = i;
                    }
                    else if (type == NodeType.BlockElse && depth == 1)
                    {
                        if (elseIndex != null)
                            throw new CompileException("“{0}” block cannot have multiple else-block instructions.".Fmt(source[start]), start + addIndex, i - start + addIndex + 1);
                        elseIndex = i;
                        elsePops = source[i] == '不';
                    }
                    else if (type == NodeType.BlockEnd)
                    {
                        depth--;
                        if (depth == 0)
                        {
                            endIndex = i;
                            return;
                        }
                    }
                }
            }
            throw new CompileException("Block instruction “{0}” is missing a matching end instruction.".Fmt(source[start]), start + addIndex, 1);
        }
    }
}
