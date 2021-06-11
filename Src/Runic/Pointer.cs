using System;
using System.Collections.Generic;
using System.Globalization;
using EsotericIDE.Runic.Math;

namespace EsotericIDE.Runic
{
    public class Pointer
    {
        protected int age = 0;
        protected int mana;

        protected int skip = 0;
        protected int delay = 0;
        protected ReadType readType = ReadType.EXECUTE;

        protected List<object> stack;
        protected List<List<object>> substacks;

        public Vector2Int position;
        public Direction direction;

        public Pointer(int m, Direction d, Vector2Int pos)
        {
            mana = m;
            stack = new List<object>();
            substacks = new List<List<object>>();
            direction = d;
            position = pos;
        }

        public int GetAge()
        {
            return age;
        }

        public int GetMana()
        {
            return mana;
        }

        public void DeductMana(int amt)
        {
            mana -= amt;
        }

        public void Push(object o)
        {
            stack.Add(o);
        }

        public object Pop()
        {
            if (stack.Count > 0)
            {
                object o = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
                return o;
            }
            mana = 0;
            return null;
        }

        public void ReverseStack()
        {
            stack.Reverse();
        }

        public int GetStacksStackSize()
        {
            return substacks.Count + 1;
        }

        public int GetStackSize()
        {
            return stack.Count;
        }

        public void Merge(Pointer newer)
        {
            mana += newer.GetMana();
            //keep stack?
        }

        public void PopNewStack(int size)
        {
            List<object> newStack = new List<object>();
            while (size-- > 0)
            {
                object o = Pop();
                newStack.Add(o);
            }
            newStack.Reverse();
            substacks.Add(stack);
            stack = newStack;
            if (size > 0)
                DeductMana(1);
        }

        public void PushNewStack()
        {
            if (substacks.Count == 0)
            {
                while (GetStackSize() > 0)
                {
                    Pop();
                }
                return;
            }
            //stack.Reverse();
            List<object> oldStack = stack;
            int last = substacks.Count - 1;
            List<object> newStack = substacks[last];
            substacks.RemoveAt(last);
            stack = newStack;
            while (oldStack.Count > 0)
            {
                Push(oldStack[0]);
                oldStack.RemoveAt(0);
            }
        }

        public void ReverseStacksStack()
        {
            substacks.Add(stack);
            substacks.Reverse();
            stack = PopStack();
        }

        public void RotateStack(bool rotLeft)
        {
            if (rotLeft) stack.RotateListLeft();
            else stack.RotateListRight();
        }

        public void RotateStacksStack(bool rotLeft)
        {
            substacks.Add(stack);
            if (rotLeft) substacks.RotateListLeft();
            else substacks.RotateListRight();
            stack = PopStack();
        }

        public void SwapStacksStack()
        {
            if (substacks.Count < 1) return;
            substacks.Add(stack);
            List<object> a = PopStack();
            List<object> b = PopStack();
            substacks.Add(a);
            substacks.Add(b);
            stack = PopStack();
        }

        public void SwapNStacksStack(int n)
        {
            bool right = n > 0;
            if (n < 0) n *= -1;
            if (substacks.Count < n - 1) n = substacks.Count;
            substacks.Add(stack);
            List<List<object>> list = new List<List<object>>();
            for (int i = 0; i < n; i++)
            {
                list.Add(PopStack());
            }
            list.Reverse();
            if (right)
                list.RotateListRight();
            else
                list.RotateListLeft();
            for (int i = 0; i < n; i++)
            {
                substacks.Add(list[i]);
            }
            stack = PopStack();
        }

        private List<object> PopStack()
        {
            if (substacks.Count > 0)
            {
                List<object> o = substacks[substacks.Count - 1];
                substacks.RemoveAt(substacks.Count - 1);
                return o;
            }
            mana = 0;
            return null;
        }

        public void PopDiscardStack()
        {
            if (substacks.Count > 0)
            {
                List<object> o = substacks[substacks.Count - 1];
                substacks.RemoveAt(substacks.Count - 1);
                return;
            }
            mana = 0;
        }

        public void CloneTopSubStack()
        {
            List<object> newStack = new List<object>();
            int size = GetStackSize();
            while (size-- > 0)
            {
                object o = Pop();
                newStack.Add(o);
            }
            newStack.Reverse();
            for (int i = 0; i < newStack.Count; i++)
            {
                Push(newStack[i]);
            }
            substacks.Add(stack);
            stack = newStack;
            if (size > 0)
                DeductMana(1);
        }

        public void Execute()
        {
            age++;
            delay = System.Math.Max(delay - 1, 0);
        }

        public bool isSkipping(bool reduce)
        {
            bool r = skip > 0;
            if (reduce)
                skip = System.Math.Max(skip - 1, 0);
            return r;
        }

        public int GetDelayAmt()
        {
            return delay;
        }

        public void SetSkip(int amt)
        {
            skip += amt;
        }

        public void SetDelay(int amt)
        {
            delay += amt;
        }

        public void SetReadType(ReadType t)
        {
            readType = t;
        }

        public ReadType GetReadType()
        {
            return readType;
        }

        public enum ReadType
        {
            READ_CHAR, READ_CHAR_CONTINUOUS, READ_STR, READ_NUM, EXECUTE
        }

        public string PrintStack()
        {
            int num = 0;
            string s = "";
            for (int i = stack.Count - 1; i >= 0; i--)
            {
                s += Syntax(stack[i]) + Environment.NewLine;
                num++;
            }
            return s;
        }

        private string Syntax(object v)
        {
            string val = v.ToString();
            if (v is string)
            {
                return "\"" + val + "\"";
            }
            if (v is char)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory((char) v);
                if (uc == UnicodeCategory.NonSpacingMark || uc == UnicodeCategory.EnclosingMark || uc == UnicodeCategory.OtherNotAssigned)
                {
                    return "\'◌" + val + "\'";
                }
                else if ((char) v == '\n')
                {
                    return "\'\\n\'";
                }
                return "\'" + val + "\'";
            }
            return val;
        }
    }
}
