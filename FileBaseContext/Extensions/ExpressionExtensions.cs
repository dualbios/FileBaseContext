using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace kDg.FileBaseContext.Extensions;

public static class ExpressionExtensions
{
    public static readonly MethodInfo ValueBufferTryReadValueMethod
        = typeof(ExpressionExtensions).GetTypeInfo().GetDeclaredMethod(nameof(ValueBufferTryReadValue))!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TValue ValueBufferTryReadValue<TValue>(
#pragma warning disable IDE0060 // Remove unused parameter
        in ValueBuffer valueBuffer,
        int index,
        IPropertyBase property)
#pragma warning restore IDE0060 // Remove unused parameter
        => (TValue)valueBuffer[index]!;

    public static Expression UnwrapTypeConversion(this Expression expression, out Type convertedType)
    {
        convertedType = null;
        while (expression is UnaryExpression unaryExpression
               && (unaryExpression.NodeType == ExpressionType.Convert
                   || unaryExpression.NodeType == ExpressionType.ConvertChecked
                   || unaryExpression.NodeType == ExpressionType.TypeAs))
        {
            expression = unaryExpression.Operand;
            if (unaryExpression.Type != typeof(object) // Ignore object conversion
                && !unaryExpression.Type.IsAssignableFrom(expression.Type)) // Ignore casting to base type/interface
            {
                convertedType = unaryExpression.Type;
            }
        }

        return expression;
    }

    public static bool IsNullConstantExpression(this Expression expression)
        => RemoveConvert(expression) is ConstantExpression constantExpression
           && constantExpression.Value == null;

    private static Expression RemoveConvert(Expression expression)
    {
        if (expression is UnaryExpression unaryExpression
            && (expression.NodeType == ExpressionType.Convert
                || expression.NodeType == ExpressionType.ConvertChecked))
        {
            return RemoveConvert(unaryExpression.Operand);
        }

        return expression;
    }

    public static T GetConstantValue<T>(this Expression expression)
        => expression is ConstantExpression constantExpression
            ? (T)constantExpression.Value!
            : throw new InvalidOperationException();
}