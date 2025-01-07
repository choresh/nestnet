using System.Linq.Expressions;

public static class ExpressionHelpers
{
    public static string GetPropertyPath<T, TProperty>(this Expression<Func<T, TProperty>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        
        throw new ArgumentException("Expression must be a property access expression", nameof(expression));
    }
} 