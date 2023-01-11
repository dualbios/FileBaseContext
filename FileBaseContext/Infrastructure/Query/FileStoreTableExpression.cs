using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace kDg.FileBaseContext.Infrastructure.Query;

public class FileBaseContextTableExpression : Expression, IPrintableExpression
{
    public FileBaseContextTableExpression(IEntityType entityType)
    {
        EntityType = entityType;
    }

    public override Type Type
        => typeof(IEnumerable<ValueBuffer>);

    public virtual IEntityType EntityType { get; }

    public override sealed ExpressionType NodeType
        => ExpressionType.Extension;

    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append(nameof(FileBaseContextTableExpression) + ": Entity: " + EntityType.DisplayName());
}