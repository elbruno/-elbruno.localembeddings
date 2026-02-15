namespace ImageSearchSample;

/// <summary>
/// Image search engine using CLIP embeddings and cosine similarity.
/// </summary>
public sealed class ImageSearchEngine
{
    private readonly ClipImageEncoder _imageEncoder;
    private readonly ClipTextEncoder _textEncoder;
    private readonly List<(string Path, float[] Embedding)> _imageIndex;

    public ImageSearchEngine(ClipImageEncoder imageEncoder, ClipTextEncoder textEncoder)
    {
        _imageEncoder = imageEncoder;
        _textEncoder = textEncoder;
        _imageIndex = new List<(string, float[])>();
    }

    /// <summary>
    /// Indexes all images in the specified directory.
    /// </summary>
    public void IndexImages(string imageDirectory)
    {
        var extensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
        var imageFiles = Directory.GetFiles(imageDirectory)
            .Where(f => extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .ToList();

        Console.WriteLine($"Indexing {imageFiles.Count} images...");

        for (int i = 0; i < imageFiles.Count; i++)
        {
            var imagePath = imageFiles[i];
            try
            {
                var embedding = _imageEncoder.Encode(imagePath);
                _imageIndex.Add((imagePath, embedding));
                Console.WriteLine($"  [{i + 1}/{imageFiles.Count}] Indexed: {Path.GetFileName(imagePath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [!] Failed to index {Path.GetFileName(imagePath)}: {ex.Message}");
            }
        }

        Console.WriteLine($"Indexing complete. {_imageIndex.Count} images ready for search.\n");
    }

    /// <summary>
    /// Searches for images matching the text query.
    /// </summary>
    public List<(string ImagePath, float Score)> Search(string query, int topK = 5)
    {
        if (_imageIndex.Count == 0)
        {
            return new List<(string, float)>();
        }

        // Encode query text
        var queryEmbedding = _textEncoder.Encode(query);

        // Compute cosine similarity with all images
        var results = new List<(string Path, float Score)>();
        foreach (var (path, imageEmbedding) in _imageIndex)
        {
            float similarity = CosineSimilarity(queryEmbedding, imageEmbedding);
            results.Add((path, similarity));
        }

        // Return top-K results sorted by score
        return results
            .OrderByDescending(r => r.Score)
            .Take(topK)
            .ToList();
    }

    /// <summary>
    /// Computes cosine similarity between two normalized vectors (dot product).
    /// </summary>
    private static float CosineSimilarity(float[] a, float[] b)
    {
        float dotProduct = 0f;
        for (int i = 0; i < a.Length && i < b.Length; i++)
        {
            dotProduct += a[i] * b[i];
        }
        return dotProduct;
    }

    public int ImageCount => _imageIndex.Count;
}
