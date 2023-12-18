namespace WebApplication1.Models
{
    public class SearchModel
    {
        public IEnumerable<string> Urls { get; set; }

        public SearchModel() {  }
        public SearchModel(IEnumerable<string> urls) { Urls = urls; }
    }
}
