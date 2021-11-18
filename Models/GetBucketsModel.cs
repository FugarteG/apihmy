using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hmyapi.Models
{
    public class GetBucketsModel
    {
        public int bucketsPerPage { get; set; }
        public string startAt { get; set; }
    }
}
