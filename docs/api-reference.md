# API Reference — elbruno.LocalEmbeddings

## LocalEmbeddingGenerator

The main class for generating embeddings. Implements `IEmbeddingGenerator<string, Embedding<float>>`.

```csharp
public sealed class LocalEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    // Constructor
    public LocalEmbeddingGenerator(LocalEmbeddingsOptions options);

    // Async factory method
    public static Task<LocalEmbeddingGenerator> CreateAsync(
        LocalEmbeddingsOptions options,
        CancellationToken cancellationToken = default);

    // Properties
    public EmbeddingGeneratorMetadata Metadata { get; }

    // Methods
    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default);

    public TService? GetService<TService>(object? key = null) where TService : class;

    public void Dispose();
}
```

## LocalEmbeddingsOptions

Configuration options for the embedding generator.

```csharp
public sealed class LocalEmbeddingsOptions
{
    public string ModelName { get; set; }
    public string? ModelPath { get; set; }
    public string? CacheDirectory { get; set; }
    public int MaxSequenceLength { get; set; }
    public bool EnsureModelDownloaded { get; set; }
    public bool NormalizeEmbeddings { get; set; }
}
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ModelName` | `string` | `"sentence-transformers/all-MiniLM-L6-v2"` | HuggingFace model identifier |
| `ModelPath` | `string?` | `null` | Path to a local model directory (bypasses download) |
| `CacheDirectory` | `string?` | `null` | Custom directory for model cache |
| `MaxSequenceLength` | `int` | `512` | Maximum token sequence length |
| `EnsureModelDownloaded` | `bool` | `true` | Download model on startup if not cached |
| `NormalizeEmbeddings` | `bool` | `false` | Normalize vectors to unit length |

## EmbeddingExtensions

Utility methods for embedding comparison and retrieval.

```csharp
public static class EmbeddingExtensions
{
    public static float CosineSimilarity(this ReadOnlyMemory<float> a, ReadOnlyMemory<float> b);

    public static float CosineSimilarity(this Embedding<float> a, Embedding<float> b);

    public static List<(T Item, float Score)> FindClosest<T>(
        this IEnumerable<(T Item, Embedding<float> Embedding)> items,
        Embedding<float> query,
        int topK = 5,
        float minScore = 0.0f);
}
```

## ServiceCollectionExtensions

Extension methods for DI registration in `LocalEmbeddings.Extensions` namespace.

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLocalEmbeddings(
        this IServiceCollection services,
        Action<LocalEmbeddingsOptions>? configure = null);

    public static IServiceCollection AddLocalEmbeddings(
        this IServiceCollection services,
        LocalEmbeddingsOptions options);

    public static IServiceCollection AddLocalEmbeddings(
        this IServiceCollection services,
        string modelName);

    public static IServiceCollection AddLocalEmbeddings(
        this IServiceCollection services,
        IConfiguration configuration);
}
```
