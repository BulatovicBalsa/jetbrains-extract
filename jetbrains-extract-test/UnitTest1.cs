using jetbrains_extract.ExprOptimizer;
using jetbrains_extract.ExprOptimizer.Abstractions;
using jetbrains_extract.ExprOptimizer.Factories;

namespace jetbrains_extract_test;

public class OptimizeTests
{
    private readonly DefaultExpressionFactory _f = new();
    private readonly Optimizer _opt = new(normalize: false);

    [Fact]
    public void Reuses_Common_Subexpressions_Structurally()
    {
        var x = _f.Variable("x");
        var two = _f.Constant(2);
        var seven = _f.Constant(7);

        var sum2X = _f.Binary(OperatorSign.Plus, two, x);
        var prod = _f.Binary(OperatorSign.Multiply, seven, sum2X);

        var sin = _f.Function(FunctionKind.Sin, prod);
        var cosx = _f.Function(FunctionKind.Cos, x);
        var left = _f.Binary(OperatorSign.Minus, sin, prod);
        var root = _f.Binary(OperatorSign.Plus, left, cosx);

        var opt = _opt.Optimize(root);

        var plus = Assert.IsAssignableFrom<IBinaryExpression>(opt);
        var leftPart = Assert.IsAssignableFrom<IBinaryExpression>(plus.Left);
        var cos = Assert.IsAssignableFrom<IFunction>(plus.Right);

        var sinPart = Assert.IsAssignableFrom<IFunction>(leftPart.Left);
        var prod1 = Assert.IsAssignableFrom<IBinaryExpression>(leftPart.Right);
        var prod0 = Assert.IsAssignableFrom<IBinaryExpression>(sinPart.Argument);

        Assert.Same(prod0, prod1);

        var sum2XOpt = Assert.IsAssignableFrom<IBinaryExpression>(prod0.Right);
        var xInSum = Assert.IsAssignableFrom<IVariableExpression>(sum2XOpt.Right);
        var xInCos = Assert.IsAssignableFrom<IVariableExpression>(cos.Argument);
        Assert.Same(xInSum, xInCos);
    }

    [Fact]
    public void Different_Shapes_Are_Not_Merged_Without_Normalization()
    {
        var x = _f.Variable("x");
        var two = _f.Constant(2);

        var a = _f.Binary(OperatorSign.Plus, x, two);
        var b = _f.Binary(OperatorSign.Plus, two, x);
        var root = _f.Binary(OperatorSign.Plus, a, b);

        var opt = _opt.Optimize(root);
        var p = Assert.IsAssignableFrom<IBinaryExpression>(opt);
        var la = Assert.IsAssignableFrom<IBinaryExpression>(p.Left);
        var rb = Assert.IsAssignableFrom<IBinaryExpression>(p.Right);

        Assert.NotSame(la, rb);
    }

    [Fact]
    public void Reuses_Primitives()
    {
        var x1 = _f.Variable("x");
        var x2 = _f.Variable("x");
        var root = _f.Binary(OperatorSign.Plus, x1, x2);

        var sum = Assert.IsAssignableFrom<IBinaryExpression>(_opt.Optimize(root));
        var lx = Assert.IsAssignableFrom<IVariableExpression>(sum.Left);
        var rx = Assert.IsAssignableFrom<IVariableExpression>(sum.Right);
        
        Assert.Same(lx, rx);
    }}
