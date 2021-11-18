using System.Collections.Generic;
using System.Linq;

namespace hmyapi.Models
{
    public class BucketInfo
    {
         private string bucketKey { get; set; }
            public string bucketName { get; set; }
            public long createdDate { get; set; }
            public string policyKey { get; set; }
            public BucketInfo(string bk, long cd, string pk)
            {
                this.bucketKey = bk;
            if (bk.Substring(0, 3) == "hmy")
            {
                this.bucketName = bk.Substring(3);
            }
            else
            {
                this.bucketName = bk;
            }
                
                this.createdDate = cd;
                this.policyKey = pk;
            }

            public static List<BucketInfo> GetHmyBuckets(List<BucketInfo> buckets)
            {
                var lista = buckets.Where(x => x.bucketKey.StartsWith("hmy")).ToList();
                return lista;
            }

            public static string GetBucketKey(List<BucketInfo> buckets, string code)
            {
                var bucket = buckets.FirstOrDefault(x => x.bucketName == code);
                if (bucket != null)
                {
                    return bucket.bucketKey;
                }
                else
                {
                    return null;
                }
            }
    }
}