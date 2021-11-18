using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using hmyapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace hmyapi.Controllers
{
    [ApiController]
    public class HmyAuthController : ControllerBase
    {
        //usar dos pantallas con un portatil
        private readonly IConfiguration _configuration;
        private static TokenModel PublicToken { get; set; }
        private static TokenModel InternalToken { get; set; }
        
        public HmyAuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Get an access token by providing your app’s client ID and secret.
        /// </summary>
        /// <param name="login">The first name to search for</param>
        /// <response code="200">Successful request; access token returned.</response>
        /// <response code="401">The client_id and client_secret combination is not valid.</response>
        /// <response code="500">Generic internal server error.</response>
        [AllowAnonymous]    
        [HttpPost] 
        [Route("api/hmy/oauth/token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Login([FromBody]LoginModel login)
        {
            IActionResult response = Unauthorized();
            if (login != null && login.clientId == "shapphmy" && login.clientSecret == "RlR6t7KwgqKLfrjH")
            {
                if (PublicToken == null || PublicToken.expiresAt < DateTime.UtcNow)
                {
                    DateTime expiresAt;
                    TokenResponseModel bodyResponse;
                    Regeneratetoken(login, out expiresAt, out bodyResponse);
                    PublicToken = new TokenModel(bodyResponse, expiresAt);

                    response = Ok(bodyResponse);
                }
                else
                {
                    response = Ok(PublicToken.access_token);
                }
            }            

            return response;
        }

        private void Regeneratetoken(LoginModel login, out DateTime expiresAt, out TokenResponseModel bodyResponse)
        {

            // Leemos el secret_key desde nuestro appseting
            var secretKey = _configuration.GetValue<string>("SecretKey");
            var key = Encoding.ASCII.GetBytes(secretKey);

            // Creamos los claims (pertenencias, características) del usuario
            var test = new List<Claim> {
                        new Claim(ClaimTypes.NameIdentifier, login.clientId),
                        new Claim(ClaimTypes.SerialNumber, login.clientSecret)
                    };
            var claims = new System.Security.Claims.ClaimsIdentity(test);

            expiresAt = DateTime.UtcNow.AddSeconds(3600);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                // Nuestro token va a durar una hora
                Expires = expiresAt,
                // Credenciales para generar el token usando nuestro secretykey y el algoritmo hash 256
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var createdToken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(createdToken);
            bodyResponse = new TokenResponseModel(token, 3600);
        }

        [Authorize]
        [HttpGet]
        [Route("api/hmy/oauth/check")]
        public bool Check()
        {
            return true;
        }
        public static dynamic GetInternalAsync()
        {
            if (InternalToken == null|| InternalToken.expiresAt < DateTime.UtcNow)
            {
                InternalToken = PublicToken;
            }
        //else para regenerar token

            return InternalToken;
        }
        private string GenerateToken(LoginModel login)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));    
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);    
    
            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],    
              _configuration["Jwt:Issuer"],    
              null,    
              expires: DateTime.Now.AddSeconds(3600), 
              signingCredentials: credentials);    
    
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string GetAppSetting(string settingKey)
        {
            return Environment.GetEnvironmentVariable(settingKey).Trim();
        }
    }
    
}