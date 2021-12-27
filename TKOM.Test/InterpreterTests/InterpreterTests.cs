using Shouldly;
using System;
using TKOM.Interpreter;
using TKOM.Node;
using Xunit;

namespace TKOMTest.InterpreterTests
{
    public class InterpreterTests
    {
        private Interpreter sut = new Interpreter();
        private class TestNode : INode
        {
            public void Accept(INodeVisitor visitor)
            {
                visitor.Visit(this);
            }
        }

        [Fact]
        public void WhenVisitingNodeOfUnknownType_ThrowsArgumentException()
        {
            var unknownNode = new TestNode();

            Action action = new Action(() => sut.Visit(unknownNode));

            action.ShouldThrow<ArgumentException>();
        }
    }
}
