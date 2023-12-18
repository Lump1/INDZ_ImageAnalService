using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Data
{
    public static class ImagesMethods
    {
        public static async Task<Uri> CreateServiceSASBlob(
            BlobClient blobClient,
            string storedPolicyName = null)
        {
            // Check if BlobContainerClient object has been authorized with Shared Key
            if (blobClient.CanGenerateSasUri)
            {
                // Create a SAS token that's valid for one day
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                    BlobName = blobClient.Name,
                    Resource = "b"
                };

                if (storedPolicyName == null)
                {
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddDays(1);
                    sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);
                }
                else
                {
                    sasBuilder.Identifier = storedPolicyName;
                }

                Uri sasURI = blobClient.GenerateSasUri(sasBuilder);

                return sasURI;
            }
            else
            {
                // Client object is not authorized via Shared Key
                return null;
            }
        }

        //public static async Task<IEnumerable<Data.CosmosDb.Image>> Search(
        //    string inputedRes, 
        //    int page, 
        //    CosmosDb.CosmosDbContext _context)
        //{
        //    var inputedResArr = inputedRes.Split(' ');
        //    var imgList = await _context.Images.ToListAsync();
        //    var imgEnumerator = imgList.Where(image => {
        //        for (int i = 0; i < inputedResArr.Length; i++)
        //            if (image.Tags.Contains(inputedRes[i]))
        //                return true;

        //        return false;
        //    });


        //    return imgEnumerator;
        //}
    }
}
