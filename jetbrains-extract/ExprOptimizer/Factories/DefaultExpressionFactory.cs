using jetbrains_extract.ExprOptimizer.Abstractions;
using jetbrains_extract.ExprOptimizer.Model;

namespace jetbrains_extract.ExprOptimizer.Factories;

public sealed class DefaultExpressionFactory : IExpressionFactory
{
    public IConstantExpression Constant(int value) => new ConstantExpression(value);
    public IVariableExpression Variable(string name) => new VariableExpression(name);
    public IBinaryExpression Binary(OperatorSign sign, IExpression left, IExpression right)
        => new BinaryExpression(sign, left, right);
    public IFunction Function(FunctionKind kind, IExpression argument)
        => new FunctionExpression(kind, argument);
}