using jetbrains_extract.ExprOptimizer.Abstractions;

namespace jetbrains_extract.ExprOptimizer.Model;

public sealed class ConstantExpression(int value) : IConstantExpression, IVisitableExpression
{
    public int Value { get; } = value;

    public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitConstant(this);

    public override bool Equals(object? obj)
        => obj is ConstantExpression c && c.Value == Value;

    public override int GetHashCode() => HashCode.Combine(1, Value);
    public override string ToString() => Value.ToString();
}