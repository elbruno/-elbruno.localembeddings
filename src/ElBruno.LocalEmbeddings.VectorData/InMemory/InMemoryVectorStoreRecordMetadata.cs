using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace ElBruno.LocalEmbeddings.VectorData.InMemory;

internal sealed class InMemoryVectorStoreRecordMetadata<TKey, TRecord>
    where TKey : notnull
    where TRecord : class
{
    private static readonly ConcurrentDictionary<Type, InMemoryVectorStoreRecordMetadata<TKey, TRecord>> Cache = new();

    private readonly PropertyInfo _keyProperty;
    private readonly PropertyInfo _vectorProperty;
    private readonly IReadOnlyDictionary<string, PropertyInfo> _propertyMap;

    private InMemoryVectorStoreRecordMetadata(
        PropertyInfo keyProperty,
        PropertyInfo vectorProperty,
        IReadOnlyDictionary<string, PropertyInfo> propertyMap)
    {
        _keyProperty = keyProperty;
        _vectorProperty = vectorProperty;
        _propertyMap = propertyMap;
    }

    public static InMemoryVectorStoreRecordMetadata<TKey, TRecord> GetOrCreate(VectorStoreCollectionDefinition? definition)
    {
        _ = definition;
        return Cache.GetOrAdd(typeof(TRecord), static _ => Create());
    }

    public TKey GetKey(TRecord record)
    {
        var value = _keyProperty.GetValue(record);
        if (value is not TKey key)
        {
            throw new InvalidOperationException(
                $"Key property '{_keyProperty.Name}' on '{typeof(TRecord).FullName}' must be assignable to '{typeof(TKey).FullName}'.");
        }

        return key;
    }

    public ReadOnlyMemory<float> GetVector(TRecord record)
    {
        var value = _vectorProperty.GetValue(record);

        return value switch
        {
            ReadOnlyMemory<float> vector => vector,
            float[] vectorArray => vectorArray,
            Embedding<float> embedding => embedding.Vector,
            null => throw new InvalidOperationException(
                $"Vector property '{_vectorProperty.Name}' on '{typeof(TRecord).FullName}' cannot be null."),
            _ => throw new InvalidOperationException(
                $"Vector property '{_vectorProperty.Name}' on '{typeof(TRecord).FullName}' must be ReadOnlyMemory<float>, float[], or Embedding<float>.")
        };
    }

    public bool TryGetPropertyValue(TRecord record, string propertyName, out object? value)
    {
        if (_propertyMap.TryGetValue(propertyName, out var propertyInfo))
        {
            value = propertyInfo.GetValue(record);
            return true;
        }

        value = null;
        return false;
    }

    private static InMemoryVectorStoreRecordMetadata<TKey, TRecord> Create()
    {
        var properties = typeof(TRecord).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var keyProperties = properties
            .Where(property => property.GetCustomAttribute<VectorStoreKeyAttribute>() is not null)
            .ToList();

        if (keyProperties.Count == 0)
        {
            throw new InvalidOperationException(
                $"Record type '{typeof(TRecord).FullName}' must contain exactly one [VectorStoreKey] property.");
        }

        if (keyProperties.Count > 1)
        {
            throw new InvalidOperationException(
                $"Record type '{typeof(TRecord).FullName}' contains multiple [VectorStoreKey] properties; only one is supported.");
        }

        var vectorProperties = properties
            .Where(property => property.GetCustomAttribute<VectorStoreVectorAttribute>() is not null)
            .ToList();

        if (vectorProperties.Count == 0)
        {
            throw new InvalidOperationException(
                $"Record type '{typeof(TRecord).FullName}' must contain exactly one [VectorStoreVector] property.");
        }

        if (vectorProperties.Count > 1)
        {
            throw new InvalidOperationException(
                $"Record type '{typeof(TRecord).FullName}' contains multiple [VectorStoreVector] properties; only one is supported.");
        }

        var keyProperty = keyProperties[0];
        if (!typeof(TKey).IsAssignableFrom(keyProperty.PropertyType))
        {
            throw new InvalidOperationException(
                $"[VectorStoreKey] property '{keyProperty.Name}' on '{typeof(TRecord).FullName}' must be assignable to key type '{typeof(TKey).FullName}'.");
        }

        var propertyMap = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in properties)
        {
            propertyMap[property.Name] = property;

            if (property.GetCustomAttribute<VectorStoreKeyAttribute>() is VectorStoreKeyAttribute keyAttribute &&
                !string.IsNullOrWhiteSpace(keyAttribute.StorageName))
            {
                propertyMap[keyAttribute.StorageName] = property;
            }

            if (property.GetCustomAttribute<VectorStoreDataAttribute>() is VectorStoreDataAttribute dataAttribute &&
                !string.IsNullOrWhiteSpace(dataAttribute.StorageName))
            {
                propertyMap[dataAttribute.StorageName] = property;
            }

            if (property.GetCustomAttribute<VectorStoreVectorAttribute>() is VectorStoreVectorAttribute vectorAttribute &&
                !string.IsNullOrWhiteSpace(vectorAttribute.StorageName))
            {
                propertyMap[vectorAttribute.StorageName] = property;
            }
        }

        return new InMemoryVectorStoreRecordMetadata<TKey, TRecord>(
            keyProperty,
            vectorProperties[0],
            propertyMap);
    }
}
