using jetbrains_extract.ExprOptimizer.Abstractions;

namespace jetbrains_extract.ExprOptimizer.Model;

public sealed class VariableExpression(string name) : IVariableExpression, IVisitableExpression
{
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitVariable(this);

    public override bool Equals(object? obj)
        => obj is VariableExpression v && StringComparer.Ordinal.Equals(v.Name, Name);

    public override int GetHashCode() => HashCode.Combine(2, StringComparer.Ordinal.GetHashCode(Name));
    public override string ToString() => Name;
}