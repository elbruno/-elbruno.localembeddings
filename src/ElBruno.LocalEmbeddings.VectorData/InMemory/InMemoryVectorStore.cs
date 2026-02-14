using System.Collections.Concurrent;
using Microsoft.Extensions.VectorData;

namespace ElBruno.LocalEmbeddings.VectorData.InMemory;

/// <summary>
/// A lightweight in-memory <see cref="VectorStore"/> implementation for local development and testing.
/// </summary>
public sealed class InMemoryVectorStore : VectorStore
{
    private readonly ConcurrentDictionary<string, object> _collections = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public override VectorStoreCollection<TKey, TRecord> GetCollection<TKey, TRecord>(
        string name,
        VectorStoreCollectionDefinition? definition = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Collection name cannot be null or whitespace.", nameof(name));
        }

        var created = _collections.GetOrAdd(name, _ =>
            new InMemoryVectorStoreCollection<TKey, TRecord>(name, definition));

        if (created is not VectorStoreCollection<TKey, TRecord> typedCollection)
        {
            throw new InvalidOperationException(
                $"Collection '{name}' was already created with a different key or record type.");
        }

        return typedCollection;
    }

    /// <inheritdoc />
    public override VectorStoreCollection<object, Dictionary<string, object?>> GetDynamicCollection(
        string name,
        VectorStoreCollectionDefinition? definition = null) =>
        throw new NotSupportedException("Dynamic collections are not currently supported by InMemoryVectorStore.");

    /// <inheritdoc />
    public override async IAsyncEnumerable<string> ListCollectionNamesAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var name in _collections.Keys.OrderBy(static n => n, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return name;
            await Task.Yield();
        }
    }

    /// <inheritdoc />
    public override Task<bool> CollectionExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_collections.ContainsKey(name));
    }

    /// <inheritdoc />
    public override Task EnsureCollectionDeletedAsync(string name, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_collections.TryRemove(name, out var collection) && collection is IDisposable disposable)
        {
            disposable.Dispose();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override object? GetService(Type serviceType, object? serviceKey = null)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        return serviceType == typeof(VectorStore) || serviceType == GetType() ? this : null;
    }
}
