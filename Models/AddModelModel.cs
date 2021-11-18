using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hmyapi.Models
{
    public class AddModelModel
    {
            public string bucketKey { get; set; }
            public IFormFile fileToUpload { get; set; }

    }
}
