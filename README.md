# ExprOptimizer

A C# library for **expression optimization** through **normalization** and **structural deduplication**.

---

## Features

* **Normalization** – commutative and associative operators (`+`, `*`) are flattened and ordered.
* **Deduplication** – identical subexpressions share the same instance in memory.
* **Extensible design** – expression types and normalization rules can be expanded independently.

---

## Example

```csharp
using jetbrains_extract.ExprOptimizer;
using jetbrains_extract.ExprOptimizer.Abstractions;
using jetbrains_extract.ExprOptimizer.Factories;
using jetbrains_extract.ExprOptimizer.Normalization;

var f = new DefaultExpressionFactory();
var opt = new Optimizer(normalize: true, factory: f, normalizer: new CanonicalNormalizer(f));

var x     = f.Variable("x");
var two   = f.Constant(2);
var seven = f.Constant(7);
var sum   = f.Binary(OperatorSign.Plus, two, x);
var prod  = f.Binary(OperatorSign.Multiply, seven, sum);

IExpression expr =
    f.Binary(OperatorSign.Plus,
        f.Binary(OperatorSign.Minus, f.Function(FunctionKind.Sin, prod), prod),
        f.Function(FunctionKind.Cos, x));

var optimized = opt.Optimize(expr);
```

After optimization, the resulting tree reuses the same instance for subexpressions like `x` and `7*(2+x)`.

---

## Structure

```
src/ExprOptimizer/
  Abstractions/      // Expression interfaces and enums
  Model/             // Expression implementations
  Factories/         // Expression factory
  Interning/         // Expression interner
  Normalization/     // Canonical normalizer
  Optimizer.cs         // Main optimizer entry
tests/
  UnitTest1 and UnitTest2 // xUnit test suite
```

---

## Tests

Run:

```bash
dotnet test
```

The tests cover:

* Normalization of `+` and `*` expressions
* Flattening and deterministic ordering
* Reuse of identical subtrees
* Detection of repeated expression groups
* Idempotence of optimization

---

## Usage

Instantiate `Optimizer` once per optimization pass and call:

```csharp
var result = optimizer.Optimize(expression);
```

The result is an equivalent expression where duplicated subtrees are merged and structure is normalized.
