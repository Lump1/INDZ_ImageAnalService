namespace WebApplication1.Models
{
    public class ImageModelView
    {
        public string Url { get; set; }
        public string Description { get; set; }

        public ImageModelView(string uriUrl = null, string Description = null) 
        {
            Url = uriUrl;
            this.Description = Description;
        }
    }
}
