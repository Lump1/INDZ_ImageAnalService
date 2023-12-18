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
            if (blobClient.CanGenerateSasUri)
            {
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
                return null;
            }
        }

        public static async Task BlobUploadAsync(BlobClient client, IFormFile image)
        {
            if (!client.Exists())
                using (var stream = new MemoryStream())
                {
                    await image.CopyToAsync(stream);

                    await client.UploadAsync(new BinaryData(stream.ToArray()));
                }
        }
    }
}
