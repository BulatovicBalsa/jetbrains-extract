namespace jetbrains_extract.ExprOptimizer.Abstractions;

public interface IExpressionInterner
{
    IExpression Intern(IExpression expression);
}