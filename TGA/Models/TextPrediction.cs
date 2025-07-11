using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGA.Models
{
    internal class TextPrediction
    {
        [ColumnName("PredictedLabel")]
        public string? Category { get; set; }
    }
}
