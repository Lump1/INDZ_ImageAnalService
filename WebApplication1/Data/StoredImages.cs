    namespace WebApplication1.Data
{
    public class StoredImages
    {
        List<byte[]> imagesData;


        public byte[] imageGetByID(int id)
        {
            return imagesData[id];
        }

        public StoredImages()
        {
            imagesData = new List<byte[]>();
        }
        public StoredImages(List<byte[]> imagesPrototypeList)
        {
            imagesData = imagesPrototypeList;
        }
    }
}
