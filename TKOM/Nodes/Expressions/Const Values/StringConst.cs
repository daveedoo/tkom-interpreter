using System;

namespace TKOM.Node
{
    public class StringConst : IExpression
    {
        public string Value { get; }

        public StringConst(string value)
        {
            Value = value;
        }
        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
