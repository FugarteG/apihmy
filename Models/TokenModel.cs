using System;
using System.Collections.Generic;
using System.Linq;

namespace hmyapi.Models
{
    class TokenModel
    {
        public TokenResponseModel access_token { get; set; }
        public DateTime expiresAt { get; set; }

        public TokenModel(TokenResponseModel access_token, DateTime expiresAt)
        {
            this.access_token = access_token;
            this.expiresAt = expiresAt;
        }
    }
}