namespace TKOM.Node
{
    public interface INodeVisitor
    {
        public void Visit(Program program);
        public void Visit(FunctionDefinition functionDefinition);

        // statements
        public void Visit(Block block);
        public void Visit(IfStatement ifStatement);
        public void Visit(TryCatchFinally tryCatchFinally);
        public void Visit(WhileStatement whileStatement);
        public void Visit(Assignment assignment);
        public void Visit(Declaration declaration);
        public void Visit(FunctionCall functionCall);
        public void Visit(ReturnStatement returnStatement);
        public void Visit(ThrowStatement throwStatement);

        // expressions
        public void Visit(Variable variable);
        public void Visit(IntConst intConst);
        public void Visit(LogicalOr logicalOr);
        public void Visit(LogicalAnd logicalAnd);
        public void Visit(EqualityOperator equalityOperator);
        public void Visit(RelationOperator relationOperator);
        public void Visit(AdditiveOperator additiveOperator);
        public void Visit(MultiplicativeOperator multiplicativeOperator);
        public void Visit(UnaryOperator binaryOperator);
    }

    public interface INode
    {
        public void Accept(INodeVisitor visitor);
    }
}
