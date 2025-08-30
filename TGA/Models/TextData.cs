using Microsoft.ML.Data;

namespace TGA.Models
{
    internal class TextData
    {
        [LoadColumn(0)]
        public string? Text { get; set; }

        [LoadColumn(1)]
        public string? Category { get; set; }
    }
}
