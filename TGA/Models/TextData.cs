using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
