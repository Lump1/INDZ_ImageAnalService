using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class ImageModelView
    {
        public string? Url { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, double>? TagsAndConfidence { get; set; }

        public ImageModelView(string uriUrl = null, string Description = null, Dictionary<string, double> TagsAndConfidence = null) 
        {
            Url = uriUrl;
            this.Description = Description;
            this.TagsAndConfidence = TagsAndConfidence;
        }
    }
}
