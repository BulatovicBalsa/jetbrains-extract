namespace jetbrains_extract.ExprOptimizer.Abstractions;

public interface IExpressionFactory
{
    IConstantExpression Constant(int value);
    IVariableExpression Variable(string name);
    IBinaryExpression Binary(OperatorSign sign, IExpression left, IExpression right);
    IFunction Function(FunctionKind kind, IExpression argument);
}
