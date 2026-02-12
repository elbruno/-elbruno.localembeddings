# API Reference — elbruno.LocalEmbeddings

## LocalEmbeddingGenerator

The main class for generating embeddings. Implements `IEmbeddingGenerator<string, Embedding<float>>`.

```csharp
public sealed class LocalEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    // Constructor
    public LocalEmbeddingGenerator(LocalEmbeddingsOptions options);

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
}
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ModelName` | `string` | `"sentence-transformers/all-MiniLM-L6-v2"` | HuggingFace model identifier |
| `ModelPath` | `string?` | `null` | Path to a local model directory (bypasses download) |
| `CacheDirectory` | `string?` | `null` | Custom directory for model cache |
| `MaxSequenceLength` | `int` | `512` | Maximum token sequence length |
| `EnsureModelDownloaded` | `bool` | `true` | Download model on startup if not cached |

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
