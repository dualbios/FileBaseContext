using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace kDg.FileBaseContext.Infrastructure.Query;

public class CollectionResultShaperExpression : Expression, IPrintableExpression
{
    public CollectionResultShaperExpression(Expression projection, Expression innerShaper, INavigationBase navigation, Type elementType)
    {
        Projection = projection;
        InnerShaper = innerShaper;
        Navigation = navigation;
        ElementType = elementType;
    }

    public virtual Type ElementType { get; }
    public virtual Expression InnerShaper { get; }
    public virtual INavigationBase Navigation { get; }
    public override sealed ExpressionType NodeType => ExpressionType.Extension;
    public virtual Expression Projection { get; }
    public override Type Type => Navigation?.ClrType ?? typeof(List<>).MakeGenericType(ElementType);

    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine("CollectionResultShaperExpression:");
        using (expressionPrinter.Indent())
        {
            expressionPrinter.Append("(");
            expressionPrinter.Visit(Projection);
            expressionPrinter.Append(", ");
            expressionPrinter.Visit(InnerShaper);
            expressionPrinter.AppendLine($", {Navigation?.Name}, {ElementType.ShortDisplayName()})");
        }
    }

    public virtual CollectionResultShaperExpression Update(
        Expression projection,
        Expression innerShaper)
    {
        return projection != Projection || innerShaper != InnerShaper
            ? new CollectionResultShaperExpression(projection, innerShaper, Navigation, ElementType)
            : this;
    }

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var projection = visitor.Visit(Projection);
        var innerShaper = visitor.Visit(InnerShaper);

        return Update(projection, innerShaper);
    }
}