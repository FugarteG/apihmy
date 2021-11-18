using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Autodesk.Forge;
using Autodesk.Forge.Model;
using hmyapi.Models;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using hmyapi.Services;
using Newtonsoft.Json;

namespace hmyapi.Controllers
{
    [ApiController]
   // [Authorize]
    public class HmyApiController : ControllerBase
    {
        private readonly IEmailSender emailService;
        public HmyApiController(IEmailSender emailSender)
        {
            emailService = emailSender;
        }

        /// <summary>
        /// Get the all projects that resides in the application.
        /// </summary>
        /// <response code="200">Projects successfully returned.</response>
        /// <response code="401">The supplied Authorization header was not valid. Verify Authentication and try again.</response>
        /// <response code="500">Generic internal server error.</response>
        [HttpGet]
        [Route("api/hmy/cwa/projects")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<List<BucketInfo>> GetBulkBuckets()
        {
            dynamic oauth = await OAuthController.GetInternalAsync();
            var bucketsApi = new BucketsApi();
            bucketsApi.Configuration.AccessToken = oauth.access_token;
            var lista = new List<BucketInfo>();
            dynamic last = null;
            int contador = 0;
            do
            {
                dynamic buckets = await bucketsApi.GetBucketsAsync("EMEA", 10,last);
                contador = 0;
                foreach (KeyValuePair<string, dynamic> bucket in new DynamicDictionaryItems(buckets.items))
                {
                    lista.Add(new BucketInfo(bucket.Value.bucketKey, bucket.Value.createdDate, bucket.Value.policyKey));
                    ++contador;
                    if(contador == 10)
                    {
                        last = bucket.Value.bucketKey;
                    }
                }
                
            } while (contador == 10);
            return BucketInfo.GetHmyBuckets(lista);

        }
        /// <summary>
        /// Get the projects that resides in the application by specifying the number of buckets per page and the starting bucket.
        /// </summary>
        /// <response code="200">Projects successfully returned.</response>
        /// <response code="401">The supplied Authorization header was not valid. Verify Authentication and try again.</response>
        /// <response code="500">Generic internal server error.</response>
        [HttpGet]
        [Route("api/hmy/cwa/projects/paginated/{bucketsPerPage}/{startAt}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<List<BucketInfo>> GetBuckets(int bucketsPerPage , string startAt )
        {

            dynamic oauth = await OAuthController.GetInternalAsync();
            var bucketsApi = new BucketsApi();
            bucketsApi.Configuration.AccessToken = oauth.access_token;
            bool completed = false;
            var lista = new List<BucketInfo>();
            dynamic buckets = await bucketsApi.GetBucketsAsync("EMEA", 1, $"hmy{startAt}");
            do
            {
                if (buckets.items.Count >0)
                {
                    var bucket = buckets.items[0];
                    lista.Add(new BucketInfo(bucket.bucketKey, bucket.createdDate, bucket.policyKey));
                    if (BucketInfo.GetHmyBuckets(lista).Count == bucketsPerPage || buckets == null)
                    {
                        completed = true;
                    }
                    else
                    {
                        startAt = bucket.bucketKey;
                    };
                }
                else
                {
                    completed = true;
                }
                buckets = await bucketsApi.GetBucketsAsync("EMEA", 1, startAt);
            } while (!completed );
            return BucketInfo.GetHmyBuckets(lista);
        }
        /// <summary>
        /// Get the projects that resides in the application by specifying the number of buckets per page and starting with the first one.
        /// </summary>
        /// <response code="200">Projects successfully returned.</response>
        /// <response code="401">The supplied Authorization header was not valid. Verify Authentication and try again.</response>
        /// <response code="500">Generic internal server error.</response>
        [HttpGet]
        [Route("api/hmy/cwa/projects/paginated/{bucketsPerPage}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<List<BucketInfo>> GetBucketsWithBucketsPerPage(int bucketsPerPage = 10)
        {
            string startAt = null;

            dynamic oauth = await OAuthController.GetInternalAsync();
            var bucketsApi = new BucketsApi();
            bucketsApi.Configuration.AccessToken = oauth.access_token;
            bool completed = false;
            var lista = new List<BucketInfo>();
            do
            {
                dynamic buckets = await bucketsApi.GetBucketsAsync("EMEA", 1, startAt);
                if (buckets.items.Count > 0)
                {
                    var bucket = buckets.items[0];

                    lista.Add(new BucketInfo(bucket.bucketKey, bucket.createdDate, bucket.policyKey));
                    var test = buckets.items.Count;
                    var test2 = bucket;
                    if (BucketInfo.GetHmyBuckets(lista).Count == bucketsPerPage || buckets == null)
                    {
                        completed = true;
                    }
                    else
                    {
                        startAt = bucket.bucketKey;
                    };
                }
                else
                {
                    completed = true;
                }

            } while (!completed);
            return BucketInfo.GetHmyBuckets(lista);
        }
        /// <summary>
        /// Creates a project. Projects are spaces that are created to store models for later retrieval.
        /// </summary>
        /// <response code="200">Project successfully created.</response>
        /// <response code="401">The supplied Authorization header was not valid. Verify Authentication and try again.</response>
        /// <response code="500">Generic internal server error.</response>
        [HttpPost]
        [Route("api/hmy/cwa/projects")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<dynamic> CreateBucket(CreateBucketModel model)
        {
            dynamic oauth = await OAuthController.GetInternalAsync();
            var bucketsApi = new BucketsApi();
            bucketsApi.Configuration.AccessToken = oauth.access_token;
            var bucketPayLoad = new PostBucketsPayload($"hmy{model.bucket}", null, PostBucketsPayload.PolicyKeyEnum.Persistent);
            var createdBucket = await bucketsApi.CreateBucketAsync(bucketPayLoad, "EMEA");
            return createdBucket;
        }

        /// <summary>
        /// Deletes a Project. Note that the Project name will not be immediately available for reuse.
        /// </summary>
        /// <response code="200">Project successfully deleted.</response>
        /// <response code="401">The supplied Authorization header was not valid. Verify Authentication and try again.</response>
        /// <response code="404">The specified Project does not exist.</response>
        /// <response code="500">Generic internal server error.</response>
        [HttpDelete]
        [Route("api/hmy/cwa/projects/{code}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteBucket(string code)
        {
            var buckets = await GetBulkBuckets();
            var bucket = buckets.Find(x => x.bucketName == code);
            if (bucket != null)
            {
                dynamic oauth = await OAuthController.GetInternalAsync();
                var bucketsApi = new BucketsApi();
                bucketsApi.Configuration.AccessToken = oauth.access_token;
                var bucketKey = $"hmy{code}";
                await bucketsApi.DeleteBucketAsync(bucketKey);
                return Ok(new ResponseModel() { Response= $"Project {code} successfully deleted." });
            }
            else
            {
                return NotFound(new ResponseModel() { Response = $"Project {code} does not exists." });
            }            
        }
        /// <summary>
        /// Uploads a model to the project and starts translating the model.
        /// </summary>
        /// <response code="200">Successfuly uploaded the model and started translation</response>
        /// <response code="401">The supplied Authorization header was not valid. Verify Authentication and try again.</response>
        /// <response code="500">Generic internal server error.</response>
        [HttpPost, DisableRequestSizeLimit]
        [Route("api/hmy/cwa/projects/model")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddModel([FromForm] AddModelModel model)
        {
            dynamic oauth = await OAuthController.GetInternalAsync();
            // save the file on the server
            var fileSavePath = Path.Combine("", Path.GetFileName(model.fileToUpload.FileName));
            using (var stream = new FileStream(fileSavePath, FileMode.Create))
                await model.fileToUpload.CopyToAsync(stream);


            // get the bucket...
            ObjectsApi objects = new ObjectsApi();
            objects.Configuration.AccessToken = oauth.access_token;

            // upload the file/object, which will create a new object
            dynamic uploadedObj;
           using (StreamReader streamReader = new StreamReader(fileSavePath))
            {
                uploadedObj = await objects.UploadObjectAsync("hmy"+model.bucketKey,
                       Path.GetFileName(model.fileToUpload.FileName), (int)streamReader.BaseStream.Length, streamReader.BaseStream,
                       "application/octet-stream");
            }

            // prepare the payload
            List<JobPayloadItem> outputs = new List<JobPayloadItem>()
            {
            new JobPayloadItem(
                JobPayloadItem.TypeEnum.Svf,
                new List<JobPayloadItem.ViewsEnum>()
                {
                JobPayloadItem.ViewsEnum._2d,
                JobPayloadItem.ViewsEnum._3d
                })
            };
            JobPayload job;
            byte[] bytes  =Encoding.ASCII.GetBytes(uploadedObj.Dictionary["objectId"])  ;
            var urn = Convert.ToBase64String(bytes);
            job = new JobPayload(new JobPayloadInput(urn), new JobPayloadOutput(outputs)); 

            // start the translation
            DerivativesApi derivative = new DerivativesApi();
            derivative.Configuration.AccessToken = oauth.access_token;
            dynamic jobPosted = await derivative.TranslateAsync(job);
            // cleanup
            System.IO.File.Delete(fileSavePath);

            return Ok(jobPosted);
        
    }
        /// <summary>
        /// Deletes a model from a bucket.
        /// </summary>
        /// <response code="200">Successfuly deleted the model.</response>
        /// <response code="400">Bad request, missing body parameters.</response>
        /// <response code="401">The supplied Authorization header was not valid. Verify Authentication and try again.</response>
        /// <response code="500">Generic internal server error.</response>
        [HttpDelete]
        [Route("api/hmy/cwa/projects/{bucketKey}/model/{modelKey}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteModel(string bucketKey, string modelKey)
        {

            dynamic oauth = await OAuthController.GetInternalAsync();
            ObjectsApi objectsApi = new ObjectsApi();
            objectsApi.Configuration.AccessToken = oauth.access_token;
            if(bucketKey == "" || modelKey == "")
            {
                return BadRequest(new ResponseModel() { Response = "ModelKey or BucketKey cant be null." });
            }
            
                await objectsApi.DeleteObjectAsync("hmy" + bucketKey,  modelKey);
                return Ok(new ResponseModel() { Response = "Successfully deleted " });
            
            
        }
        /// <summary>
        /// Return project details.
        /// </summary>
        /// <response code="200">Get project details was successful.</response>
        /// <response code="401">The supplied Authorization header was not valid. Verify Authentication and try again.</response>
        /// <response code="404">The size requested is not allowed.</response>
        /// <response code="500">Generic internal server error.</response>
        [HttpGet]
        [Route("api/hmy/cwa/projects/{code}/{size}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBucketInfo(string code, int size)
        {
            if (size == 100 || size == 200 || size == 400)
            {
                var buckets = await GetBulkBuckets();
                var bucketKey = BucketInfo.GetBucketKey(buckets, code);
                if (bucketKey != null)
                {
                    dynamic oauth = await OAuthController.GetInternalAsync();
                    var objectsApi = new ObjectsApi();
                    objectsApi.Configuration.AccessToken = oauth.access_token;
                    dynamic objects = await objectsApi.GetObjectsAsync(bucketKey);
                    var lista = new List<ObjectInfo>();
                    foreach(KeyValuePair<string, dynamic> ossObject in new DynamicDictionaryItems(objects.items))
                    {
                        var objectId = (string)ossObject.Value.objectId;
                        var urn = OSSController.Base64Encode(objectId);
                        var thumbnail = await GetThumbnail(urn, size);
                        var source = HmyAuthController.GetAppSetting("CWA_URLDOMAIN") + urn;
                        lista.Add(new ObjectInfo(ossObject.Value.objectKey, urn, source, bucketKey.Substring(3), thumbnail));
                    }
                    if (lista.Count == 0)
                    {
                        return NotFound(new ResponseModel() { Response = $"El proyecto {code} no tiene modelos." });
                    }
                    else
                    {
                        return Ok(lista);
                    }
                    

                }
                else
                {
                    return NotFound(new ResponseModel() { Response = $"El proyecto {code} no existe." });
                }
            }
            else {
                return NotFound(new ResponseModel() { Response = $"El tamaño {size} no está disponible. Utiliza 100, 200 o 400." });
            }
        }
        /// <summary>
        /// Return if project exists.
        /// </summary>
        /// <response code="200">Project exists.</response>
        /// <response code="401">The supplied Authorization header was not valid. Verify Authentication and try again.</response>
        /// <response code="404">The size requested is not allowed.</response>
        /// <response code="500">Generic internal server error.</response>
        [HttpGet]
        [Route("api/hmy/cwa/projects/{code}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BucketExists(string code)
        {
            if (code == "") 
            {
                return BadRequest(new ResponseModel() { Response = "El nombre no puede ser nulo" });
            }
            else
            {
                var buckets = await GetBulkBuckets();
                var bucketKey = BucketInfo.GetBucketKey(buckets, code);
                if (bucketKey != null)
                {

                    dynamic oauth = await OAuthController.GetInternalAsync();
                    var objectsApi = new ObjectsApi();
                    objectsApi.Configuration.AccessToken = oauth.access_token;
                    dynamic objects = await objectsApi.GetObjectsAsync(bucketKey);
                    var lista = new List<ObjectInfo>();
                    foreach(KeyValuePair<string, dynamic> ossObject in new DynamicDictionaryItems(objects.items))
                    {
                        var objectId = (string)ossObject.Value.objectId;
                        var urn = OSSController.Base64Encode(objectId);
                        var token = HmyAuthController.GetInternalAsync();
                        var source = HmyAuthController.GetAppSetting("CWA_URLDOMAIN") + urn + "&token=" + token.access_token;
                        //var source = "https://localhost:4200/external-viewer?urn=" + urn + "&token=" + token.access_token;
                        lista.Add(new ObjectInfo(ossObject.Value.objectKey, urn, source, bucketKey.Substring(3)));
                    }
                    if (lista.Count == 0)
                    {
                        return NotFound(new ResponseModel() { Response = $"El proyecto {code} no tiene modelos." });
                    }
                    else
                    {
                        return Ok(lista);
                    }
                }
                else
                {
                    return NotFound(new ResponseModel() { Response = $"El proyecto {code} no existe." });
                }
            }
        }

        private async Task<string> GetThumbnail(string urn, int size)
        {
            dynamic oauth = await OAuthController.GetInternalAsync();
            var derivativesApi = new DerivativesApi();
            derivativesApi.Configuration.AccessToken = oauth.access_token;
            MemoryStream thumbnail = await derivativesApi.GetThumbnailAsync(urn, size, size);
            var image = System.Convert.ToBase64String(thumbnail.ToArray());
            return image;
        }
        /// <summary>
        /// Gives the current percentage of the translation.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/hmy/cwa/projects/model-percentage")]
        
        public  async Task<IActionResult> GetTranslationPercentage(UrnModel urn)
        {
            var apiInstance = new DerivativesApi();
            
            dynamic oauth = await OAuthController.GetInternalAsync();
            apiInstance.Configuration.AccessToken = oauth.access_token;
            try
            {
                dynamic result = apiInstance.GetManifest(urn.urn);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest("ERROR" + e);
            }
        }


        /// <summary>
        /// Model for TranslateObject method
        /// </summary>
        public class TranslateObjectModel
        {
            public string bucketKey { get; set; }
            public string objectName { get; set; }
        }

        [HttpGet]
        [Route("api/hmy/cwa/colors")]
        public async Task<IActionResult> GetColors()
        {
            var lista = HmyColor.GetHmyColors();
            return Ok(lista);
        }
        [HttpPost]
        [Route("api/hmy/cwa/colors")]
        public async Task<IActionResult> PostColors(dynamic colores)
        {
            await emailService.SendEmailAsync("adrian@atbim.es", "HMY OTC Color Configurator", "Se adjunta la configuración de colores. ",Convert.ToString(colores));
            return Ok(Convert.ToString(colores));
        }
    }
}
