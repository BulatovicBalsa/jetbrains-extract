using jetbrains_extract.ExprOptimizer.Abstractions;

namespace jetbrains_extract.ExprOptimizer.Model;

public sealed class BinaryExpression(OperatorSign sign, IExpression left, IExpression right)
    : IBinaryExpression, IVisitableExpression
{
    public IExpression Left  { get; } = left  ?? throw new ArgumentNullException(nameof(left));
    public IExpression Right { get; } = right ?? throw new ArgumentNullException(nameof(right));
    public OperatorSign Sign { get; } = sign;

    public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitBinary(this);

    public override bool Equals(object? obj)
        => obj is BinaryExpression b &&
           b.Sign == Sign &&
           Equals(b.Left, Left) &&
           Equals(b.Right, Right);

    public override int GetHashCode()
        => HashCode.Combine(4, (int)Sign, Left?.GetHashCode() ?? 0, Right?.GetHashCode() ?? 0);

    public override string ToString() => $"({Left} {Sign} {Right})";
}