using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace TGA.Models
{
    class Classifier
    {
        private static ITransformer _loadedModel;
        private static MLContext _mlContext;
        private static PredictionEngine<TextData, TextPrediction> _predEngine;

        public static void LoadModel(string modelPath)
        {
            _mlContext = new MLContext(seed: 0);
            _loadedModel = _mlContext.Model.Load(modelPath, out var modelInputSchema);
        }

        public static string PredictCategory(string text)
        {
            TextData singleIssue = new TextData() { Text = text };

            _predEngine = _mlContext.Model.CreatePredictionEngine<TextData, TextPrediction>(_loadedModel);

            var prediction = _predEngine.Predict(singleIssue);

            return prediction.Category;
        }
    }
}
