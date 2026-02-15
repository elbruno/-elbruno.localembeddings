using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Numerics.Tensors;

namespace ImageSearchSample;

/// <summary>
/// CLIP text encoder using ONNX Runtime.
/// </summary>
public sealed class ClipTextEncoder : IDisposable
{
    private readonly InferenceSession _session;
    private readonly ClipTokenizer _tokenizer;
    private readonly string _inputIdsName;
    private readonly string _attentionMaskName;
    private readonly string _outputName;

    public ClipTextEncoder(string modelPath, string vocabPath, string mergesPath)
    {
        _session = new InferenceSession(modelPath);
        _tokenizer = new ClipTokenizer(vocabPath, mergesPath);

        // Get input/output names from model metadata
        _inputIdsName = _session.InputMetadata.Keys.FirstOrDefault(k => k.Contains("input_ids") || k.Contains("input")) 
            ?? _session.InputMetadata.Keys.First();
        _attentionMaskName = _session.InputMetadata.Keys.FirstOrDefault(k => k.Contains("attention_mask") || k.Contains("mask")) 
            ?? _session.InputMetadata.Keys.Skip(1).First();
        _outputName = _session.OutputMetadata.Keys.First();
    }

    /// <summary>
    /// Encodes text to an L2-normalized embedding vector.
    /// </summary>
    public float[] Encode(string text)
    {
        // Tokenize
        var (inputIds, attentionMask) = _tokenizer.Encode(text);

        // Create input tensors with shape [1, 77]
        var inputIdsTensor = new DenseTensor<long>(new[] { 1, 77 });
        var attentionMaskTensor = new DenseTensor<long>(new[] { 1, 77 });

        for (int i = 0; i < 77; i++)
        {
            inputIdsTensor[0, i] = inputIds[i];
            attentionMaskTensor[0, i] = attentionMask[i];
        }

        // Run inference
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(_inputIdsName, inputIdsTensor),
            NamedOnnxValue.CreateFromTensor(_attentionMaskName, attentionMaskTensor)
        };

        using var results = _session.Run(inputs);
        var output = results.First().AsEnumerable<float>().ToArray();

        // L2 normalize
        Normalize(output);

        return output;
    }

    private static void Normalize(float[] vector)
    {
        var span = vector.AsSpan();
        float norm = MathF.Sqrt(TensorPrimitives.SumOfSquares(span));
        if (norm > 0)
        {
            TensorPrimitives.Divide(span, norm, span);
        }
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}
