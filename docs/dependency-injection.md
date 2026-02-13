# Dependency Injection — elbruno.LocalEmbeddings

`AddLocalEmbeddings()` provides four overloads for flexible registration of `IEmbeddingGenerator<string, Embedding<float>>`.

## 1) Basic registration

```csharp
using LocalEmbeddings.Extensions;

services.AddLocalEmbeddings();
```

## 2) Configure with delegate

```csharp
services.AddLocalEmbeddings(options =>
{
    options.ModelName = "sentence-transformers/all-MiniLM-L6-v2";
    options.MaxSequenceLength = 256;
    options.NormalizeEmbeddings = true;
});
```

## 3) Register with pre-built options

```csharp
var options = new LocalEmbeddingsOptions
{
    ModelName = "sentence-transformers/all-MiniLM-L6-v2",
    CacheDirectory = "/models/cache"
};

services.AddLocalEmbeddings(options);
```

## 4) Register with model name only

```csharp
services.AddLocalEmbeddings("sentence-transformers/all-MiniLM-L6-v2");
```

## 5) IConfiguration binding

```json
{
  "LocalEmbeddings": {
    "ModelName": "sentence-transformers/all-MiniLM-L6-v2",
    "MaxSequenceLength": 256,
    "NormalizeEmbeddings": true,
    "CacheDirectory": "/path/to/cache"
  }
}
```

```csharp
services.AddLocalEmbeddings(configuration.GetSection("LocalEmbeddings"));
```

## Injecting the generator

```csharp
public sealed class MyService(
    IEmbeddingGenerator<string, Embedding<float>> embeddings)
{
    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var result = await embeddings.GenerateAsync([text]);
        return result[0].Vector.ToArray();
    }
}
```
