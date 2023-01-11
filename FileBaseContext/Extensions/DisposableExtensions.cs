namespace kDg.FileBaseContext.Extensions;

internal static class DisposableExtensions
{
    public static ValueTask DisposeAsyncIfAvailable(this IDisposable disposable)
    {
        if (disposable != null)
        {
            if (disposable is IAsyncDisposable asyncDisposable)
            {
                return asyncDisposable.DisposeAsync();
            }

            disposable.Dispose();
        }

        return default;
    }
}