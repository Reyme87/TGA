using Microsoft.ML.Data;

namespace TGA.Models
{
    internal class TextPrediction
    {
        [ColumnName("PredictedLabel")]
        public string? Category { get; set; }
    }
}
