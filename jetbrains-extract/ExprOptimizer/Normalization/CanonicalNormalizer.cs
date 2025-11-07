using jetbrains_extract.ExprOptimizer.Abstractions;
using jetbrains_extract.ExprOptimizer.Model;

namespace jetbrains_extract.ExprOptimizer.Normalization;

public sealed class CanonicalNormalizer(IExpressionFactory factory)
    : ISemanticNormalizer, IExpressionVisitor<IExpression>
{
    private readonly IExpressionFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    public IExpression Normalize(IExpression expression)
        => (expression as IVisitableExpression ?? throw new NotSupportedException())
           .Accept(this);

    public IExpression VisitConstant(ConstantExpression c) => c;
    public IExpression VisitVariable(VariableExpression v) => v;

    public IExpression VisitFunction(FunctionExpression f)
    {
        var argN = Normalize(f.Argument);
        return ReferenceEquals(argN, f.Argument) ? f : _factory.Function(f.Kind, argN);
    }

    public IExpression VisitBinary(BinaryExpression b)
    {
        var l = Normalize(b.Left);
        var r = Normalize(b.Right);

        return b.Sign switch
        {
            OperatorSign.Plus     => CanonicalPlus(l, r),
            OperatorSign.Multiply => CanonicalMultiply(l, r),
            OperatorSign.Minus    => CanonicalMinus(l, r),
            OperatorSign.Divide   => CanonicalDivide(l, r),
            _ => throw new NotSupportedException()
        };
    }

    private IExpression CanonicalPlus(IExpression l, IExpression r)
    {
        var terms = new List<IExpression>();
        Collect(l, terms, OperatorSign.Plus);
        Collect(r, terms, OperatorSign.Plus);

        terms.Sort((a, b) => string.CompareOrdinal(a.ToString(), b.ToString()));

        return BuildBalanced(terms, OperatorSign.Plus);
    }

    private IExpression CanonicalMultiply(IExpression l, IExpression r)
    {
        var factors = new List<IExpression>();
        Collect(l, factors, OperatorSign.Multiply);
        Collect(r, factors, OperatorSign.Multiply);

        factors.Sort((a, b) => string.CompareOrdinal(a.ToString(), b.ToString()));

        return BuildBalanced(factors, OperatorSign.Multiply);
    }

    private static BinaryExpression CanonicalMinus(IExpression l, IExpression r) => new(OperatorSign.Minus, l, r);

    private static BinaryExpression CanonicalDivide(IExpression l, IExpression r) => new(OperatorSign.Divide, l, r);

    private static void Collect(IExpression e, List<IExpression> acc, OperatorSign sign)
    {
        if (e is BinaryExpression b && b.Sign == sign)
        {
            Collect(b.Left, acc, sign);
            Collect(b.Right, acc, sign);
        }
        else acc.Add(e);
    }

    private IExpression BuildBalanced(List<IExpression> items, OperatorSign sign)
    {
        return items.Count switch
        {
            0 => _factory.Constant(0),
            1 => items[0],
            _ => BuildRec(0, items.Count - 1)
        };

        IExpression BuildRec(int lo, int hi)
        {
            if (lo == hi) return items[lo];
            var mid = (lo + hi) / 2;
            var left = BuildRec(lo, mid);
            var right = BuildRec(mid + 1, hi);
            return _factory.Binary(sign, left, right);
        }
    }
}
