using Azure;


namespace WebApplication1.Data.CosmosDb
{
    public class Image
    {
        private static int idCounter = 0;
        public int Id { get; set; }
        public string Name { get; set; }
        public string Tags { get; set; }

        public Image(string Name, string Tags)
        {
            this.Id = System.Threading.Interlocked.Increment(ref idCounter);
            this.Name = Name;
            this.Tags = Tags;
        }
        public Image()
        {
            this.Id = System.Threading.Interlocked.Increment(ref idCounter);
        }

        public static void _init(int lastId)
        {
            idCounter = lastId;
        }
    }
}
