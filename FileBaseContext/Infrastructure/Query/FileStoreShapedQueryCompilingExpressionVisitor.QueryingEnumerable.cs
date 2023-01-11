using kDg.FileBaseContext.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections;

namespace kDg.FileBaseContext.Infrastructure.Query;

public partial class FileBaseContextShapedQueryCompilingExpressionVisitor
{
    private sealed class QueryingEnumerable<T> : IAsyncEnumerable<T>, IEnumerable<T>, IQueryingEnumerable
    {
        private readonly QueryContext _queryContext;
        private readonly IEnumerable<ValueBuffer> _innerEnumerable;
        private readonly Func<QueryContext, ValueBuffer, T> _shaper;
        private readonly Type _contextType;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
        private readonly bool _standAloneStateManager;
        private readonly bool _threadSafetyChecksEnabled;

        public QueryingEnumerable(
            QueryContext queryContext,
            IEnumerable<ValueBuffer> innerEnumerable,
            Func<QueryContext, ValueBuffer, T> shaper,
            Type contextType,
            bool standAloneStateManager,
            bool threadSafetyChecksEnabled)
        {
            _queryContext = queryContext;
            _innerEnumerable = innerEnumerable;
            _shaper = shaper;
            _contextType = contextType;
            _queryLogger = queryContext.QueryLogger;
            _standAloneStateManager = standAloneStateManager;
            _threadSafetyChecksEnabled = threadSafetyChecksEnabled;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new Enumerator(this, cancellationToken);

        public IEnumerator<T> GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public string ToQueryString()
            => "InMemoryStrings.NoQueryStrings";

        private sealed class Enumerator : IEnumerator<T>, IAsyncEnumerator<T>
        {
            private readonly QueryContext _queryContext;
            private readonly IEnumerable<ValueBuffer> _innerEnumerable;
            private readonly Func<QueryContext, ValueBuffer, T> _shaper;
            private readonly Type _contextType;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
            private readonly bool _standAloneStateManager;
            private readonly CancellationToken _cancellationToken;
            private readonly IConcurrencyDetector _concurrencyDetector;
            private readonly IExceptionDetector _exceptionDetector;

            private IEnumerator<ValueBuffer> _enumerator;

            public Enumerator(QueryingEnumerable<T> queryingEnumerable, CancellationToken cancellationToken = default)
            {
                _queryContext = queryingEnumerable._queryContext;
                _innerEnumerable = queryingEnumerable._innerEnumerable;
                _shaper = queryingEnumerable._shaper;
                _contextType = queryingEnumerable._contextType;
                _queryLogger = queryingEnumerable._queryLogger;
                _standAloneStateManager = queryingEnumerable._standAloneStateManager;
                _cancellationToken = cancellationToken;
                _exceptionDetector = _queryContext.ExceptionDetector;
                Current = default!;

                _concurrencyDetector = queryingEnumerable._threadSafetyChecksEnabled
                    ? _queryContext.ConcurrencyDetector
                    : null;
            }

            public T Current { get; private set; }

            object IEnumerator.Current
                => Current!;

            public bool MoveNext()
            {
                try
                {
                    _concurrencyDetector?.EnterCriticalSection();

                    try
                    {
                        return MoveNextHelper();
                    }
                    finally
                    {
                        _concurrencyDetector?.ExitCriticalSection();
                    }
                }
                catch (Exception exception)
                {
                    if (_exceptionDetector.IsCancellation(exception))
                    {
                        _queryLogger.QueryCanceled(_contextType);
                    }
                    else
                    {
                        _queryLogger.QueryIterationFailed(_contextType, exception);
                    }

                    throw;
                }
            }

            public ValueTask<bool> MoveNextAsync()
            {
                try
                {
                    _concurrencyDetector?.EnterCriticalSection();

                    try
                    {
                        _cancellationToken.ThrowIfCancellationRequested();

                        return new ValueTask<bool>(MoveNextHelper());
                    }
                    finally
                    {
                        _concurrencyDetector?.ExitCriticalSection();
                    }
                }
                catch (Exception exception)
                {
                    if (_exceptionDetector.IsCancellation(exception, _cancellationToken))
                    {
                        _queryLogger.QueryCanceled(_contextType);
                    }
                    else
                    {
                        _queryLogger.QueryIterationFailed(_contextType, exception);
                    }

                    throw;
                }
            }

            private bool MoveNextHelper()
            {
                if (_enumerator == null)
                {
                    EntityFrameworkEventSource.Log.QueryExecuting();

                    _enumerator = _innerEnumerable.GetEnumerator();
                    _queryContext.InitializeStateManager(_standAloneStateManager);
                }

                var hasNext = _enumerator.MoveNext();

                Current = hasNext
                    ? _shaper(_queryContext, _enumerator.Current)
                    : default!;

                return hasNext;
            }

            public void Dispose()
            {
                _enumerator?.Dispose();
                _enumerator = null;
            }

            public ValueTask DisposeAsync()
            {
                var enumerator = _enumerator;
                _enumerator = null;

                return enumerator.DisposeAsyncIfAvailable();
            }

            public void Reset()
                => throw new NotSupportedException(CoreStrings.EnumerableResetNotSupported);
        }
    }
}