using jetbrains_extract.ExprOptimizer.Abstractions;
using jetbrains_extract.ExprOptimizer.Interning;

namespace jetbrains_extract.ExprOptimizer;

public static class Optimizer
{
    public static IExpression Optimize(IExpression expression)
        => new ExpressionInterner().Intern(expression);
}