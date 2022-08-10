namespace WebListener.Models
{
    public class OffSiteMessageDTO
    {
       // public String customerGuid="";
        public String backupName ="";
        public String msgType = "";
        public String customerGUID = "";
        //public String backupName = "";
        public String status = "";
        public String azureBlobEndpoint = "";
        //public String BlobStorageEndpoint = stuff.BlobStorageEndpoint;
        //string StorageKey = stuff.StorageKey;
        public String BlobContainerName = "archive";
       // public String BlobSASToken = stuff.BlobSASToken;
        public String errorMsg = "";
        public int? RetentionDays =30;
        public string? passPhrase;
    }
}
