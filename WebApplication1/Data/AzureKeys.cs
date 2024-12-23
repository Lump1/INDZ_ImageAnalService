namespace WebApplication1.Data
{
    public class AzureKeys
    {
        public string COMPUTER_VISION_KEY { get; set; }
        public string BLOB_SERVICE_KEY { get; set; }


        public AzureKeys() 
        {
            COMPUTER_VISION_KEY = string.Empty;
            BLOB_SERVICE_KEY = string.Empty;
        }
    }
}
