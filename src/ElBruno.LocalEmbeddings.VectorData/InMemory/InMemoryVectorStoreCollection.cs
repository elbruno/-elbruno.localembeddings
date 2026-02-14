using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using ElBruno.LocalEmbeddings.Extensions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace ElBruno.LocalEmbeddings.VectorData.InMemory;

internal sealed class InMemoryVectorStoreCollection<TKey, TRecord> : VectorStoreCollection<TKey, TRecord>
    where TKey : notnull
    where TRecord : class
{
    private readonly ConcurrentDictionary<TKey, TRecord> _records = new();
    private readonly InMemoryVectorStoreRecordMetadata<TKey, TRecord> _metadata;

    public InMemoryVectorStoreCollection(string name, VectorStoreCollectionDefinition? definition)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Collection name cannot be null or whitespace.", nameof(name));
        }

        Name = name;
        _metadata = InMemoryVectorStoreRecordMetadata<TKey, TRecord>.GetOrCreate(definition);
    }

    public override string Name { get; }

    public override Task<bool> CollectionExistsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(true);
    }

    public override Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public override Task EnsureCollectionDeletedAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _records.Clear();
        return Task.CompletedTask;
    }

    public override Task<TRecord?> GetAsync(
        TKey key,
        RecordRetrievalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = options;
        _records.TryGetValue(key, out var record);
        return Task.FromResult(record);
    }

    public override async IAsyncEnumerable<TRecord> GetAsync(
        System.Linq.Expressions.Expression<Func<TRecord, bool>> filter,
        int top,
        FilteredRecordRetrievalOptions<TRecord>? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (top < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(top), top, "top must be greater than zero.");
        }

        var predicate = filter.Compile();
        var skip = options?.Skip ?? 0;

        foreach (var item in _records.Values.Where(predicate).Skip(skip).Take(top))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
            await Task.Yield();
        }
    }

    public override Task DeleteAsync(TKey key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _records.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public override Task UpsertAsync(TRecord record, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(record);

        var key = _metadata.GetKey(record);
        _records[key] = record;
        return Task.CompletedTask;
    }

    public override Task UpsertAsync(IEnumerable<TRecord> records, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(records);

        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var key = _metadata.GetKey(record);
            _records[key] = record;
        }

        return Task.CompletedTask;
    }

    public override async IAsyncEnumerable<VectorSearchResult<TRecord>> SearchAsync<TInput>(
        TInput searchValue,
        int top,
        VectorSearchOptions<TRecord>? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (top < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(top), top, "top must be greater than zero.");
        }

        var queryVector = ConvertSearchValue(searchValue);
        var skip = options?.Skip ?? 0;
        var predicate = options?.Filter?.Compile();

        var ranked = _records.Values
            .Where(record => predicate is null || predicate(record))
            .Select(record =>
            {
                var score = queryVector.CosineSimilarity(_metadata.GetVector(record));
                return new
                {
                    Record = record,
                    Score = score,
                    Key = _metadata.GetKey(record)
                };
            })
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Key)
            .Skip(skip)
            .Take(top)
            .ToList();

        foreach (var item in ranked)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new VectorSearchResult<TRecord>(item.Record, item.Score);
            await Task.Yield();
        }
    }

    public override object? GetService(Type serviceType, object? serviceKey = null)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        return serviceType == typeof(VectorStoreCollection<TKey, TRecord>) || serviceType == GetType() ? this : null;
    }

    private static ReadOnlyMemory<float> ConvertSearchValue<TInput>(TInput searchValue)
    {
        return searchValue switch
        {
            Embedding<float> embedding => embedding.Vector,
            ReadOnlyMemory<float> vector => vector,
            float[] vectorArray => vectorArray,
            _ => throw new NotSupportedException(
                $"Search value type '{typeof(TInput).FullName}' is not supported by InMemoryVectorStoreCollection. " +
                "Supported types are Embedding<float>, ReadOnlyMemory<float>, and float[].")
        };
    }
}
