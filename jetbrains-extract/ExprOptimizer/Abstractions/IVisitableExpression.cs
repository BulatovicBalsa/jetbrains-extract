namespace jetbrains_extract.ExprOptimizer.Abstractions;

public interface IVisitableExpression : IExpression
{
    T Accept<T>(IExpressionVisitor<T> visitor);
}