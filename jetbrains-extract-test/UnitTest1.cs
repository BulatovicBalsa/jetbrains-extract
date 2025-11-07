using jetbrains_extract.ExprOptimizer;
using jetbrains_extract.ExprOptimizer.Abstractions;
using jetbrains_extract.ExprOptimizer.Factories;

namespace jetbrains_extract_test;

public class OptimizeTests
{
    private readonly DefaultExpressionFactory _f = new();

    private IExpression BuildSample()
    {
        // Build: sin(7 * (2 + x)) - 7 * (2 + x) + cos(x)
        var x = _f.Variable("x");
        var two = _f.Constant(2);
        var seven = _f.Constant(7);

        var sum2X = _f.Binary(OperatorSign.Plus, two, x);        // (2 + x)
        var prod = _f.Binary(OperatorSign.Multiply, seven, sum2X); // 7 * (2 + x)

        var sin = _f.Function(FunctionKind.Sin, prod);           // sin(7*(2+x))
        var cosx = _f.Function(FunctionKind.Cos, x);             // cos(x)

        var left = _f.Binary(OperatorSign.Minus, sin, prod);     // sin(...) - 7*(2+x)
        var whole = _f.Binary(OperatorSign.Plus, left, cosx);    // (sin(...) - 7*(2+x)) + cos(x)
        return whole;
    }

    [Fact]
    public void Optimize_Reuses_Common_Subexpressions()
    {
        var expr = BuildSample();

        // Before optimize, there are two distinct prod nodes and multiple distinct x nodes
        var opt = Optimizer.Optimize(expr);

        // Walk down the optimized tree and check object identity (reference equality)
        var plus = Assert.IsAssignableFrom<IBinaryExpression>(opt);
        var left = Assert.IsAssignableFrom<IBinaryExpression>(plus.Left);               // sin(...) - 7*(2+x)
        var cosx = Assert.IsAssignableFrom<IFunction>(plus.Right);

        var sin = Assert.IsAssignableFrom<IFunction>(left.Left);
        var prod1 = Assert.IsAssignableFrom<IBinaryExpression>(left.Right);
        var prod0 = Assert.IsAssignableFrom<IBinaryExpression>(sin.Argument);

        // Both products should be SAME instance
        Assert.Same(prod0, prod1);

        // x inside cos(x) and x inside (2 + x) should be SAME instance
        var sum2X = Assert.IsAssignableFrom<IBinaryExpression>(prod0.Left is IConstantExpression ? prod0.Right : prod0.Left);
        // sum2X is (2 + x)
        var xInSum = Assert.IsAssignableFrom<IVariableExpression>(sum2X.Right);
        var xInCos = Assert.IsAssignableFrom<IVariableExpression>(cosx.Argument);
        Assert.Same(xInSum, xInCos);
    }

    [Fact]
    public void Optimize_DoesNotMerge_Different_Shapes()
    {
        // (x + 2) vs (2 + x) are kept distinct (we are NOT normalizing commutativity here)
        var x = _f.Variable("x");
        var two = _f.Constant(2);

        var a = _f.Binary(OperatorSign.Plus, x, two);
        var b = _f.Binary(OperatorSign.Plus, two, x);
        var root = _f.Binary(OperatorSign.Plus, a, b);

        var opt = Optimizer.Optimize(root);
        var p = Assert.IsAssignableFrom<IBinaryExpression>(opt);
        var la = Assert.IsAssignableFrom<IBinaryExpression>(p.Left);
        var rb = Assert.IsAssignableFrom<IBinaryExpression>(p.Right);

        // different structure → not the same instance
        Assert.NotSame(la, rb);
    }

    [Fact]
    public void Optimize_Single_Nodes_Are_Interned()
    {
        var x1 = _f.Variable("x");
        var x2 = _f.Variable("x");
        var root = _f.Binary(OperatorSign.Plus, x1, x2);

        var opt = Optimizer.Optimize(root);
        var sum = Assert.IsAssignableFrom<IBinaryExpression>(opt);
        var lx = Assert.IsAssignableFrom<IVariableExpression>(sum.Left);
        var rx = Assert.IsAssignableFrom<IVariableExpression>(sum.Right);

        // After optimization, both "x" references are the same object
        Assert.Same(lx, rx);
    }

    [Fact]
    public void Optimize_Constant_Reuse()
    {
        var c1 = _f.Constant(7);
        var c2 = _f.Constant(7);
        var root = _f.Binary(OperatorSign.Multiply, c1, c2);

        var opt = Optimizer.Optimize(root);
        var mul = Assert.IsAssignableFrom<IBinaryExpression>(opt);
        var lc = Assert.IsAssignableFrom<IConstantExpression>(mul.Left);
        var rc = Assert.IsAssignableFrom<IConstantExpression>(mul.Right);

        Assert.Same(lc, rc);
        Assert.Equal(7, lc.Value);
    }
}
