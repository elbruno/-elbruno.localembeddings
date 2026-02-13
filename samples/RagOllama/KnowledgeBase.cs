namespace RagOllama;

internal static class KnowledgeBase
{
    public static IReadOnlyList<string> Documents { get; } =
    [
        ".NET local embeddings can run offline using ONNX Runtime and HuggingFace sentence transformer models.",
        "LocalEmbeddingGenerator implements IEmbeddingGenerator<string, Embedding<float>> from Microsoft.Extensions.AI.",
        "The default model in LocalEmbeddings is sentence-transformers/all-MiniLM-L6-v2 with 384 dimensions.",
        "EmbeddingExtensions.CosineSimilarity compares vectors by angle, which is useful for semantic search.",
        "EmbeddingExtensions.FindClosest returns the top K items sorted by descending cosine similarity score.",
        "Set LocalEmbeddingsOptions.ModelPath to load a local ONNX model directory instead of downloading.",
        "Set LocalEmbeddingsOptions.CacheDirectory to override where LocalEmbeddings stores downloaded models.",
        "Set LocalEmbeddingsOptions.NormalizeEmbeddings to true to output unit-length vectors.",
        "Use AddLocalEmbeddings in dependency injection to register IEmbeddingGenerator for application services.",
        "For retrieval-augmented generation, embed docs once, embed queries at runtime, and inject top matches as context."
    ];
}
