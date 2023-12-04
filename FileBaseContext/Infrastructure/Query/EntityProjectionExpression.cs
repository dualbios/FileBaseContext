using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace kDg.FileBaseContext.Infrastructure.Query;

public class EntityProjectionExpression : Expression, IPrintableExpression
{
    private readonly IReadOnlyDictionary<IProperty, MethodCallExpression> _readExpressionMap;
    private readonly Dictionary<INavigation, StructuralTypeShaperExpression> _navigationExpressionsCache = new();

    public EntityProjectionExpression(
        IEntityType entityType,
        IReadOnlyDictionary<IProperty, MethodCallExpression> readExpressionMap)
    {
        EntityType = entityType;
        _readExpressionMap = readExpressionMap;
    }

    public virtual IEntityType EntityType { get; }

    public override Type Type
        => EntityType.ClrType;

    public override sealed ExpressionType NodeType
        => ExpressionType.Extension;

    public virtual EntityProjectionExpression UpdateEntityType(IEntityType derivedType)
    {
        if (!derivedType.GetAllBaseTypes().Contains(EntityType))
        {
            throw new InvalidOperationException(
                @"InMemoryStrings.InvalidDerivedTypeInEntityProjection(
                    derivedType.DisplayName(), EntityType.DisplayName())");
        }

        var readExpressionMap = new Dictionary<IProperty, MethodCallExpression>();
        foreach (var (property, methodCallExpression) in _readExpressionMap)
        {
            if (derivedType.IsAssignableFrom(property.DeclaringEntityType)
                || property.DeclaringEntityType.IsAssignableFrom(derivedType))
            {
                readExpressionMap[property] = methodCallExpression;
            }
        }

        return new EntityProjectionExpression(derivedType, readExpressionMap);
    }

    public virtual MethodCallExpression BindProperty(IProperty property)
    {
        if (!EntityType.IsAssignableFrom(property.DeclaringEntityType)
            && !property.DeclaringEntityType.IsAssignableFrom(EntityType))
        {
            throw new InvalidOperationException(
                "InMemoryStrings.UnableToBindMemberToEntityProjection(\"property\", property.Name, EntityType.DisplayName())");
        }

        return _readExpressionMap[property];
    }

    public virtual void AddNavigationBinding(INavigation navigation, StructuralTypeShaperExpression entityShaper)
    {
        if (!EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
            && !navigation.DeclaringEntityType.IsAssignableFrom(EntityType))
        {
            throw new InvalidOperationException(
                "InMemoryStrings.UnableToBindMemberToEntityProjection(\"navigation\", navigation.Name, EntityType.DisplayName())");
        }

        _navigationExpressionsCache[navigation] = entityShaper;
    }

    public virtual StructuralTypeShaperExpression BindNavigation(INavigation navigation)
    {
        if (!EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
            && !navigation.DeclaringEntityType.IsAssignableFrom(EntityType))
        {
            throw new InvalidOperationException(
                "InMemoryStrings.UnableToBindMemberToEntityProjection(\"navigation\", navigation.Name, EntityType.DisplayName())");
        }

        return _navigationExpressionsCache.TryGetValue(navigation, out var expression)
            ? expression
            : null;
    }

    public virtual EntityProjectionExpression Clone()
    {
        var readExpressionMap = new Dictionary<IProperty, MethodCallExpression>(_readExpressionMap);
        var entityProjectionExpression = new EntityProjectionExpression(EntityType, readExpressionMap);
        foreach (var (navigation, entityShaperExpression) in _navigationExpressionsCache)
        {
            entityProjectionExpression._navigationExpressionsCache[navigation] = new StructuralTypeShaperExpression(
                entityShaperExpression.StructuralType,
                ((EntityProjectionExpression)entityShaperExpression.ValueBufferExpression).Clone(),
                entityShaperExpression.IsNullable);
        }

        return entityProjectionExpression;
    }

    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine(nameof(EntityProjectionExpression) + ":");
        using (expressionPrinter.Indent())
        {
            foreach (var (property, methodCallExpression) in _readExpressionMap)
            {
                expressionPrinter.Append(property + " -> ");
                expressionPrinter.Visit(methodCallExpression);
                expressionPrinter.AppendLine();
            }
        }
    }
}