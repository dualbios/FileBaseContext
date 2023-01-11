using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace kDg.FileBaseContext.Infrastructure.Query;

public class SingleResultShaperExpression : Expression, IPrintableExpression
{
    public SingleResultShaperExpression(
        Expression projection,
        Expression innerShaper)
    {
        Projection = projection;
        InnerShaper = innerShaper;
        Type = innerShaper.Type;
    }

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var projection = visitor.Visit(Projection);
        var innerShaper = visitor.Visit(InnerShaper);

        return Update(projection, innerShaper);
    }

    public virtual SingleResultShaperExpression Update(Expression projection, Expression innerShaper)
        => projection != Projection || innerShaper != InnerShaper
            ? new SingleResultShaperExpression(projection, innerShaper)
            : this;

    public override sealed ExpressionType NodeType
        => ExpressionType.Extension;

    public override Type Type { get; }

    public virtual Expression Projection { get; }

    public virtual Expression InnerShaper { get; }

    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine($"{nameof(SingleResultShaperExpression)}:");
        using (expressionPrinter.Indent())
        {
            expressionPrinter.Append("(");
            expressionPrinter.Visit(Projection);
            expressionPrinter.Append(", ");
            expressionPrinter.Visit(InnerShaper);
            expressionPrinter.AppendLine(")");
        }
    }
}