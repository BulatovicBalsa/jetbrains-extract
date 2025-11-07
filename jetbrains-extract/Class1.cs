using System.Diagnostics.CodeAnalysis;

namespace jetbrains_extract;

using System;
using System.Collections.Generic;

public interface IExpression { }
public interface IConstantExpression : IExpression { int Value { get; } }
public interface IVariableExpression : IExpression { string Name { get; } }
public interface IBinaryExpression : IExpression
{
    IExpression Left { get; }
    IExpression Right { get; }
    OperatorSign Sign { get; }
}
public interface IFunction : IExpression
{
    FunctionKind Kind { get; }
    IExpression Argument { get; }
}

public enum FunctionKind { Sin, Cos, Max }
public enum OperatorSign { Plus, Minus, Multiply, Divide }

// ===== Immutable implementations (SRP: just data containers) =====
public sealed record ConstantExpression(int Value) : IConstantExpression;
public sealed record VariableExpression(string Name) : IVariableExpression;

public sealed record BinaryExpression(IExpression Left, IExpression Right, OperatorSign Sign)
    : IBinaryExpression;

public sealed record FunctionExpression(FunctionKind Kind, IExpression Argument)
    : IFunction;

// ===== Factory abstraction (DIP: callers depend on abstraction, not concrete types) =====
public interface IExpressionFactory
{
    IConstantExpression Constant(int value);
    IVariableExpression Variable(string name);
    IBinaryExpression Binary(OperatorSign sign, IExpression left, IExpression right);
    IFunction Function(FunctionKind kind, IExpression argument);
}

public sealed class DefaultExpressionFactory : IExpressionFactory
{
    public IConstantExpression Constant(int value) => new ConstantExpression(value);
    public IVariableExpression Variable(string name) => new VariableExpression(name);
    public IBinaryExpression Binary(OperatorSign sign, IExpression left, IExpression right)
        => new BinaryExpression(left, right, sign);
    public IFunction Function(FunctionKind kind, IExpression argument)
        => new FunctionExpression(kind, argument);
}

// ===== Structural comparer (SRP: only compares structure; OCP: easy to extend) =====
public sealed class StructuralExpressionComparer : IEqualityComparer<IExpression>
{
    public bool Equals(IExpression? x, IExpression? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;

        return (x, y) switch
        {
            (IConstantExpression a, IConstantExpression b) => a.Value == b.Value,
            (IVariableExpression a, IVariableExpression b) => StringComparer.Ordinal.Equals(a.Name, b.Name),
            (IFunction a, IFunction b) =>
                a.Kind == b.Kind && Equals(a.Argument, b.Argument),
            (IBinaryExpression a, IBinaryExpression b) =>
                a.Sign == b.Sign && Equals(a.Left, b.Left) && Equals(a.Right, b.Right),
            _ => false
        };
    }

    public int GetHashCode(IExpression? obj)
    {
        if (obj is null) return 0;
        return obj switch
        {
            IConstantExpression c => HashCode.Combine(1, c.Value),
            IVariableExpression v => HashCode.Combine(2, StringComparer.Ordinal.GetHashCode(v.Name)),
            IFunction f => HashCode.Combine(3, (int)f.Kind, GetHashCode(f.Argument)),
            IBinaryExpression b => HashCode.Combine(4, (int)b.Sign, GetHashCode(b.Left), GetHashCode(b.Right)),
            _ => 0
        };
    }
}

// ===== Interner (SRP: only deduplicates instances) =====
public sealed class ExpressionInterner
{
    private readonly IExpressionFactory _factory;
    private readonly Dictionary<IExpression, IExpression> _pool;

    public ExpressionInterner(IExpressionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        var comparer = new StructuralExpressionComparer();
        _pool = new Dictionary<IExpression, IExpression>(comparer);
    }

    public IExpression Intern(IExpression expression)
    {
        // First intern children, then build a canonical parent and pool it.
        IExpression canonical = expression switch
        {
            IConstantExpression c => _factory.Constant(c.Value),
            IVariableExpression v => _factory.Variable(v.Name),
            IFunction f => _factory.Function(f.Kind, Intern(f.Argument)),
            IBinaryExpression b => _factory.Binary(b.Sign, Intern(b.Left), Intern(b.Right)),
            _ => throw new NotSupportedException($"Unknown expression type: {expression.GetType().Name}")
        };

        if (_pool.TryGetValue(canonical, out var existing))
            return existing;

        _pool[canonical] = canonical;
        return canonical;
    }
}

// ===== API surface required by the prompt =====
public static class Optimizer
{
    // High-level orchestration (SRP: just coordinates; DIP: depends on interfaces)
    public static IExpression Optimize(IExpression expression)
        => new ExpressionInterner(new DefaultExpressionFactory()).Intern(expression);
}
