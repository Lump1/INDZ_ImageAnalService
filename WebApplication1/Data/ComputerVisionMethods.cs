using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace WebApplication1.Data
{
    public static class ComputerVisionMethods
    {
        public static ComputerVisionClient Authenticate(string key, string endpoint)
        {
            ComputerVisionClient visionClient = new ComputerVisionClient(
            new ApiKeyServiceClientCredentials(key))
            { Endpoint = endpoint };
            return visionClient;
        }

        public static async Task<ImageAnalysis?> AnalyzeImageAsync(ComputerVisionClient visionClient, string url)
        {
            List<VisualFeatureTypes?> features =
            Enum.GetValues(typeof(VisualFeatureTypes)).OfType<VisualFeatureTypes?>().ToList();

            if(features == null || features.Count == 0)
            {
                return null;
            }

            ImageAnalysis analysis = await visionClient.AnalyzeImageAsync(url, features);

            return analysis;
        }

    }   
}
