using System.Collections.Generic;
using System.Linq;

namespace hmyapi.Models
{
    public class TokenResponseModel
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set;}

        public TokenResponseModel(string at, int ei)
        {
            this.access_token = at;
            this.expires_in = ei;
            this.token_type = "Bearer";
        }
    }
}