using jetbrains_extract.ExprOptimizer.Model;
using ConstantExpression = jetbrains_extract.ExprOptimizer.Model.ConstantExpression;

namespace jetbrains_extract.ExprOptimizer.Abstractions;

public interface IExpressionVisitor<out T>
{
    T VisitConstant(ConstantExpression c);
    T VisitVariable(VariableExpression v);
    T VisitFunction(FunctionExpression f);
    T VisitBinary(BinaryExpression b);
}