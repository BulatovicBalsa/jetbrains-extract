namespace jetbrains_extract.ExprOptimizer.Abstractions;

public interface IExpression { }
public interface IConstantExpression : IExpression { int Value { get; } }
public interface IVariableExpression : IExpression { string Name { get; } }
public interface IBinaryExpression : IExpression
{
    IExpression Left { get; }
    IExpression Right { get; }
    OperatorSign Sign { get; }
}
public interface IFunction : IExpression
{
    FunctionKind Kind { get; }
    IExpression Argument { get; }
}