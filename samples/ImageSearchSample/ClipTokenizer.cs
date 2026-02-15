using System.Text.Json;

namespace ImageSearchSample;

/// <summary>
/// Minimal tokenizer for CLIP text encoding using BPE.
/// </summary>
public sealed class ClipTokenizer
{
    private readonly Dictionary<string, int> _vocabulary;
    private const int SOT = 49406;  // Start of text token
    private const int EOT = 49407;  // End of text token
    private const int ContextLength = 77;

    public ClipTokenizer(string vocabJsonPath, string mergesTxtPath)
    {
        // Load vocab
        var json = File.ReadAllText(vocabJsonPath);
        _vocabulary = JsonSerializer.Deserialize<Dictionary<string, int>>(json) 
            ?? throw new Exception("Failed to parse vocabulary");

        // Note: merges.txt is required by CLIP but not used in this simplified implementation
        // A full BPE tokenizer would use merge rules for subword tokenization
        if (!File.Exists(mergesTxtPath))
        {
            throw new FileNotFoundException($"Merge rules file not found: {mergesTxtPath}");
        }
    }

    public (int[] InputIds, int[] AttentionMask) Encode(string text)
    {
        // This is a simplified tokenizer implementation for demonstration purposes.
        // Note: CLIP's BPE vocabulary is case-sensitive. This implementation converts to lowercase
        // which may reduce search quality for proper nouns and capitalized words.
        // For production use, consider using a full BPE tokenizer implementation.
        
        var tokens = new List<int> { SOT };

        // Simple whitespace tokenization + vocab lookup
        var words = text.ToLowerInvariant().Split(
            new[] { ' ', '\t', '\n', '\r', '.', ',', '!', '?', ';', ':', '-', '(', ')' }, 
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            // Try to find token in vocabulary
            if (_vocabulary.TryGetValue(word, out int tokenId))
            {
                tokens.Add(tokenId);
            }
            else
            {
                // Try character-level fallback
                foreach (char c in word)
                {
                    var charStr = c.ToString();
                    if (_vocabulary.TryGetValue(charStr, out int charId))
                    {
                        tokens.Add(charId);
                    }
                }
            }
        }

        tokens.Add(EOT);

        // Pad or truncate to context length
        var inputIds = new int[ContextLength];
        var attentionMask = new int[ContextLength];

        int copyLen = Math.Min(tokens.Count, ContextLength);
        for (int i = 0; i < copyLen; i++)
        {
            inputIds[i] = tokens[i];
            attentionMask[i] = 1;
        }

        return (inputIds, attentionMask);
    }
}
