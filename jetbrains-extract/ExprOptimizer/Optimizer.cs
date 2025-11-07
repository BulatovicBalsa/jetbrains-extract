using jetbrains_extract.ExprOptimizer.Abstractions;
using jetbrains_extract.ExprOptimizer.Factories;
using jetbrains_extract.ExprOptimizer.Interning;
using jetbrains_extract.ExprOptimizer.Normalization;

namespace jetbrains_extract.ExprOptimizer;

public sealed class Optimizer(bool normalize)
{
    public IExpression Optimize(IExpression expression)
    {
        var factory = new DefaultExpressionFactory();
        var normal = new CanonicalNormalizer(factory);
        var intern = new ExpressionInterner();

        if (normalize)
            expression = normal.Normalize(expression);
        
        return intern.Intern(expression);
    }
}