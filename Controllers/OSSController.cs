using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace hmyapi.Controllers
{
    [ApiController]
    public class OSSController : ControllerBase
    {
        private IWebHostEnvironment _env;
        public OSSController(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Base64 enconde a string
        /// </summary>
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string encodedText)
        {
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedText));
        }
    }
}