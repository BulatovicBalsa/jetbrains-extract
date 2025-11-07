using jetbrains_extract.ExprOptimizer;
using jetbrains_extract.ExprOptimizer.Abstractions;
using jetbrains_extract.ExprOptimizer.Factories;
using jetbrains_extract.ExprOptimizer.Model;

namespace jetbrains_extract_test;

public class UnitTest2
{
    private readonly DefaultExpressionFactory _f = new();
    private readonly Optimizer _opt = new(normalize: true);

    [Fact]
    public void Plus_Permutations_Dedupe_When_Part_Of_Same_Root()
    {
        // Variants of x+2+3 wrapped with Sin() to keep them as identifiable subtrees:
        var x = _f.Variable("x");
        var two = _f.Constant(2);
        var three = _f.Constant(3);

        var e1 = _f.Binary(OperatorSign.Plus, _f.Binary(OperatorSign.Plus, x, two), three);
        var e2 = _f.Binary(OperatorSign.Plus, _f.Binary(OperatorSign.Plus, two, x), three);
        var e3 = _f.Binary(OperatorSign.Plus, three, _f.Binary(OperatorSign.Plus, two, x));
        var e4 = _f.Binary(OperatorSign.Plus, x, _f.Binary(OperatorSign.Plus, three, two));

        // Root: Sin(e1) + Sin(e2) + Sin(e3) + Sin(e4)
        IExpression root =
            _f.Binary(OperatorSign.Plus,
                _f.Binary(OperatorSign.Plus,
                    _f.Function(FunctionKind.Sin, e1),
                    _f.Function(FunctionKind.Sin, e2)),
                _f.Binary(OperatorSign.Plus,
                    _f.Function(FunctionKind.Sin, e3),
                    _f.Function(FunctionKind.Sin, e4)));

        var optimized = _opt.Optimize(root);

        // Collect all Sin nodes under the top-level plus
        var sins = CollectFunctions(optimized, FunctionKind.Sin);
        Assert.Equal(4, sins.Count);

        // All Sin nodes must be the SAME instance (interned) in this single tree
        AssertAllSame(sins);

        // And their Arguments should also be the SAME canonical
        var args = sins.Select(s => ((IFunction)s).Argument).ToList();
        AssertAllSame(args);
    }

    [Fact]
    public void Plus_Deeply_Nested_Shapes_Dedupe_When_Wrapped_And_In_Same_Root()
    {
        var x = _f.Variable("x");
        var a = _f.Variable("a");
        var b = _f.Variable("b");
        var c = _f.Variable("c");

        var leftHeavy  = _f.Binary(OperatorSign.Plus, _f.Binary(OperatorSign.Plus, _f.Binary(OperatorSign.Plus, x, a), b), c);
        var rightHeavy = _f.Binary(OperatorSign.Plus, x, _f.Binary(OperatorSign.Plus, a, _f.Binary(OperatorSign.Plus, b, c)));

        // Sin(left) + Sin(right)  → both Sin nodes and their arguments should be interned to SAME
        var root = _f.Binary(OperatorSign.Plus, _f.Function(FunctionKind.Sin, leftHeavy), _f.Function(FunctionKind.Sin, rightHeavy));
        var optimized = _opt.Optimize(root);

        var sins = CollectFunctions(optimized, FunctionKind.Sin);
        Assert.Equal(2, sins.Count);
        AssertAllSame(sins);

        var args = sins.Select(s => ((IFunction)s).Argument).ToList();
        AssertAllSame(args);
    }

    [Fact]
    public void Multiply_Permutations_Dedupe_When_Part_Of_Same_Root()
    {
        var x = _f.Variable("x");
        var two = _f.Constant(2);
        var three = _f.Constant(3);

        var m1 = _f.Binary(OperatorSign.Multiply, _f.Binary(OperatorSign.Multiply, x, two), three);
        var m2 = _f.Binary(OperatorSign.Multiply, _f.Binary(OperatorSign.Multiply, two, x), three);
        var m3 = _f.Binary(OperatorSign.Multiply, three, _f.Binary(OperatorSign.Multiply, two, x));
        var m4 = _f.Binary(OperatorSign.Multiply, x, _f.Binary(OperatorSign.Multiply, three, two));

        // Root: Cos(m1) + Cos(m2) + Cos(m3) + Cos(m4)
        IExpression root =
            _f.Binary(OperatorSign.Plus,
                _f.Binary(OperatorSign.Plus,
                    _f.Function(FunctionKind.Cos, m1),
                    _f.Function(FunctionKind.Cos, m2)),
                _f.Binary(OperatorSign.Plus,
                    _f.Function(FunctionKind.Cos, m3),
                    _f.Function(FunctionKind.Cos, m4)));

        var optimized = _opt.Optimize(root);

        var coses = CollectFunctions(optimized, FunctionKind.Cos);
        Assert.Equal(4, coses.Count);
        AssertAllSame(coses);

        var args = new List<IExpression>();
        foreach (var c in coses) args.Add(((IFunction)c).Argument);
        AssertAllSame(args);
    }

    [Fact]
    public void Multiply_Deeply_Nested_Shapes_Dedupe_When_Wrapped_And_In_Same_Root()
    {
        var x = _f.Variable("x");
        var a = _f.Variable("a");
        var b = _f.Variable("b");
        var c = _f.Variable("c");

        var leftHeavy  = _f.Binary(OperatorSign.Multiply, _f.Binary(OperatorSign.Multiply, _f.Binary(OperatorSign.Multiply, x, a), b), c);
        var rightHeavy = _f.Binary(OperatorSign.Multiply, x, _f.Binary(OperatorSign.Multiply, a, _f.Binary(OperatorSign.Multiply, b, c)));

        var root = _f.Binary(OperatorSign.Plus, _f.Function(FunctionKind.Cos, leftHeavy), _f.Function(FunctionKind.Cos, rightHeavy));
        var optimized = _opt.Optimize(root);

        var coses = CollectFunctions(optimized, FunctionKind.Cos);
        Assert.Equal(2, coses.Count);
        AssertAllSame(coses);

        var args = coses.Select(cfn => ((IFunction)cfn).Argument).ToList();
        AssertAllSame(args);
    }

    [Fact]
    public void Plus_Canonical_Order_Is_Deterministic_In_Same_Root()
    {
        var x = _f.Variable("x");
        var a = _f.Variable("a");
        var b = _f.Variable("b");
        var two = _f.Constant(2);
        var three = _f.Constant(3);

        var p1 = _f.Binary(OperatorSign.Plus, x, _f.Binary(OperatorSign.Plus, a, _f.Binary(OperatorSign.Plus, three, _f.Binary(OperatorSign.Plus, two, b))));
        var p2 = _f.Binary(OperatorSign.Plus, b, _f.Binary(OperatorSign.Plus, three, _f.Binary(OperatorSign.Plus, a, _f.Binary(OperatorSign.Plus, two, x))));
        var p3 = _f.Binary(OperatorSign.Plus, _f.Binary(OperatorSign.Plus, a, _f.Binary(OperatorSign.Plus, b, x)), _f.Binary(OperatorSign.Plus, three, two));

        var root = _f.Binary(OperatorSign.Plus,
                     _f.Function(FunctionKind.Sin, p1),
                     _f.Binary(OperatorSign.Plus,
                         _f.Function(FunctionKind.Sin, p2),
                         _f.Function(FunctionKind.Sin, p3)));

        var optimized = _opt.Optimize(root);

        var sins = CollectFunctions(optimized, FunctionKind.Sin);
        Assert.Equal(3, sins.Count);
        AssertAllSame(sins);

        var args = sins.Select(s => ((IFunction)s).Argument).ToList();
        AssertAllSame(args);
    }

    [Fact]
    public void Multiply_Canonical_Order_Is_Deterministic_In_Same_Root()
    {
        var x = _f.Variable("x");
        var a = _f.Variable("a");
        var b = _f.Variable("b");
        var two = _f.Constant(2);
        var three = _f.Constant(3);

        var m1 = _f.Binary(OperatorSign.Multiply, x, _f.Binary(OperatorSign.Multiply, a, _f.Binary(OperatorSign.Multiply, three, _f.Binary(OperatorSign.Multiply, two, b))));
        var m2 = _f.Binary(OperatorSign.Multiply, b, _f.Binary(OperatorSign.Multiply, three, _f.Binary(OperatorSign.Multiply, a, _f.Binary(OperatorSign.Multiply, two, x))));
        var m3 = _f.Binary(OperatorSign.Multiply, _f.Binary(OperatorSign.Multiply, a, _f.Binary(OperatorSign.Multiply, b, x)), _f.Binary(OperatorSign.Multiply, three, two));

        var root = _f.Binary(OperatorSign.Plus,
                     _f.Function(FunctionKind.Cos, m1),
                     _f.Binary(OperatorSign.Plus,
                         _f.Function(FunctionKind.Cos, m2),
                         _f.Function(FunctionKind.Cos, m3)));

        var optimized = _opt.Optimize(root);

        var coses = CollectFunctions(optimized, FunctionKind.Cos);
        Assert.Equal(3, coses.Count);
        AssertAllSame(coses);

        var args = coses.Select(c => ((IFunction)c).Argument).ToList();
        AssertAllSame(args);
    }

    // ---------- Helpers ----------

    private static void AssertAllSame(IReadOnlyList<IExpression> nodes)
    {
        // Ensure all references are the same object
        for (var i = 1; i < nodes.Count; i++)
            Assert.Same(nodes[0], nodes[i]);
    }

    private static List<IExpression> CollectFunctions(IExpression root, FunctionKind kind)
    {
        var result = new List<IExpression>();
        var stack = new Stack<IExpression>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var cur = stack.Pop();
            switch (cur)
            {
                case BinaryExpression b:
                    stack.Push(b.Left);
                    stack.Push(b.Right);
                    break;
                case FunctionExpression f when f.Kind == kind:
                    result.Add(f);
                    stack.Push(f.Argument);
                    break;
                case FunctionExpression f:
                    stack.Push(f.Argument);
                    break;
            }
        }
        return result;
    }
}