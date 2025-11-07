using jetbrains_extract.ExprOptimizer.Abstractions;

namespace jetbrains_extract.ExprOptimizer.Model;

public sealed class FunctionExpression(FunctionKind kind, IExpression argument) : IFunction, IVisitableExpression
{
    public FunctionKind Kind { get; } = kind;
    public IExpression Argument { get; } = argument ?? throw new ArgumentNullException(nameof(argument));

    public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitFunction(this);

    public override bool Equals(object? obj)
        => obj is FunctionExpression f && f.Kind == Kind && Equals(f.Argument, Argument);

    public override int GetHashCode() => HashCode.Combine(3, (int)Kind, Argument?.GetHashCode() ?? 0);
    public override string ToString() => $"{Kind}({Argument})";
}