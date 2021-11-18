namespace hmyapi.Models
{
    public class ObjectInfo
    {
            public string urn { get; set; }
            public string source {get; set;}
            public string objectName { get; set; }
            public string thumbnail { get; set; }
            public string bucketKey { get; set; }
            public ObjectInfo(string objectName, string urn, string source, string bucketKey, string thumbnail = "")
            {
                this.objectName = objectName;
                if (thumbnail != "")
                {
                    this.thumbnail = thumbnail;
                }
                this.urn = urn;
                this.source = source;
                this.bucketKey = bucketKey;                
            }
    }
}