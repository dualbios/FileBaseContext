using Microsoft.EntityFrameworkCore.Query;

namespace kDg.FileBaseContext.Infrastructure.Query;

public class FileBaseContextShapedQueryCompilingExpressionVisitorFactory : IShapedQueryCompilingExpressionVisitorFactory
{
    public FileBaseContextShapedQueryCompilingExpressionVisitorFactory(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    protected virtual ShapedQueryCompilingExpressionVisitorDependencies Dependencies { get; }

    public virtual ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
        => new FileBaseContextShapedQueryCompilingExpressionVisitor(Dependencies, queryCompilationContext);
}