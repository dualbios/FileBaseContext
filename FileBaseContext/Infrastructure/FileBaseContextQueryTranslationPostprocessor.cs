using Microsoft.EntityFrameworkCore.Query;

namespace kDg.FileBaseContext.Infrastructure;

public class FileBaseContextQueryTranslationPostprocessor : QueryTranslationPostprocessor
{
    public FileBaseContextQueryTranslationPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext)
    {
    }
}