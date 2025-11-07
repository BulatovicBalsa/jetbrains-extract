namespace jetbrains_extract.ExprOptimizer.Abstractions;

public interface ISemanticNormalizer
{
    /// Returns a semantically equivalent expression in a canonical form.
    IExpression Normalize(IExpression expression);
}