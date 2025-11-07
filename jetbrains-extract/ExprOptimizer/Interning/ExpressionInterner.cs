using jetbrains_extract.ExprOptimizer.Abstractions;
using jetbrains_extract.ExprOptimizer.Model;

namespace jetbrains_extract.ExprOptimizer.Interning;

public sealed class ExpressionInterner :
    IExpressionInterner,
    IExpressionVisitor<IExpression>
{
    private readonly Dictionary<IExpression, IExpression> _pool = new();

    public IExpression Intern(IExpression expression)
    {
        return expression is not IVisitableExpression visitable ? 
            throw new NotSupportedException($"Expression does not support visiting: {expression.GetType().Name}") : 
            visitable.Accept(this);
    }

    // Visitor methods — build candidate with interned children, then intern structurally
    public IExpression VisitConstant(ConstantExpression c)
        => InternOrReuse(c);

    public IExpression VisitVariable(VariableExpression v)
        => InternOrReuse(v);

    public IExpression VisitFunction(FunctionExpression f)
    {
        var arg = Intern(f.Argument);
        var candidate = ReferenceEquals(arg, f.Argument) ? f : new FunctionExpression(f.Kind, arg);
        return InternOrReuse(candidate);
    }

    public IExpression VisitBinary(BinaryExpression b)
    {
        var l = Intern(b.Left);
        var r = Intern(b.Right);
        var candidate = (ReferenceEquals(l, b.Left) && ReferenceEquals(r, b.Right))
            ? b
            : new BinaryExpression(b.Sign, l, r);
        return InternOrReuse(candidate);
    }

    private IExpression InternOrReuse(IExpression candidate)
    {
        if (_pool.TryGetValue(candidate, out var existing)) return existing;
        _pool[candidate] = candidate;
        return candidate;
    }
}