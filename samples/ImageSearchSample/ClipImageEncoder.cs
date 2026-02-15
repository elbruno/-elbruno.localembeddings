using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics.Tensors;

namespace ImageSearchSample;

/// <summary>
/// CLIP image encoder using ONNX Runtime and ImageSharp.
/// </summary>
public sealed class ClipImageEncoder : IDisposable
{
    private readonly InferenceSession _session;
    private readonly string _inputName;
    private readonly string _outputName;

    // CLIP normalization parameters
    private static readonly float[] Mean = { 0.48145466f, 0.4578275f, 0.40821073f };
    private static readonly float[] Std = { 0.26862954f, 0.26130258f, 0.27577711f };
    private const int ImageSize = 224;

    public ClipImageEncoder(string modelPath)
    {
        _session = new InferenceSession(modelPath);
        _inputName = _session.InputMetadata.Keys.First();
        _outputName = _session.OutputMetadata.Keys.First();
    }

    /// <summary>
    /// Encodes an image file to an L2-normalized embedding vector.
    /// </summary>
    public float[] Encode(string imagePath)
    {
        // Load and preprocess image
        using var image = Image.Load<Rgb24>(imagePath);
        
        // Resize to 224x224
        image.Mutate(x => x.Resize(ImageSize, ImageSize));

        // Create NCHW tensor [1, 3, 224, 224]
        var tensor = new DenseTensor<float>(new[] { 1, 3, ImageSize, ImageSize });

        // Convert to normalized float tensor
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < ImageSize; y++)
            {
                var pixelRow = accessor.GetRowSpan(y);
                for (int x = 0; x < ImageSize; x++)
                {
                    var pixel = pixelRow[x];
                    
                    // Normalize: (pixel / 255 - mean) / std
                    tensor[0, 0, y, x] = (pixel.R / 255.0f - Mean[0]) / Std[0];
                    tensor[0, 1, y, x] = (pixel.G / 255.0f - Mean[1]) / Std[1];
                    tensor[0, 2, y, x] = (pixel.B / 255.0f - Mean[2]) / Std[2];
                }
            }
        });

        // Run inference
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(_inputName, tensor)
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
