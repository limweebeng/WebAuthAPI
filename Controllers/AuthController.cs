using Helper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OA.AuthLibrary;
using OA.WebAuthLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;

namespace WebAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private string _passCodeLicenser = DataShared.Properties.Resources.licenser;
        private IWebHostEnvironment _hostingEnvironment;
        private ILogger<AuthController> _logger;

        #region Constructor
        public AuthController(IWebHostEnvironment environment,
            ILogger<AuthController> logger)
        {
            _hostingEnvironment = environment;
            _logger = logger;
        }
        #endregion

        [HttpGet()]
        public ActionResult GetAuthWeb()
        {
            System.IO.FileStream fs = new System.IO.FileStream(WebAuthHelper.AuthWebFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            fs.CopyTo(ms);
            fs.Close();
            var contentType = "APPLICATION/octet-stream";
            var fileName = Path.GetFileName(WebAuthHelper.AuthWebFilePath);
            FileStreamResult fsTemp = File(ms, contentType, fileName);
            fsTemp.FileStream.Position = 0;
            return fsTemp;
        }

        [HttpGet("size")]
        public ActionResult GetAuthWebSize()
        {
            FileInfo fi = new FileInfo(WebAuthHelper.AuthWebFilePath);
            return Ok(fi.Length.ToString());
        }

        [HttpPut("{dbID}")]
        public ActionResult GetDBWeb(string dbID)
        {
            string filePath = Path.Combine(WebAuthHelper.MainFolder, dbID);
            filePath = Path.Combine(filePath, WebAuthHelper.DatabaseWeb);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                fs.CopyTo(ms);
                fs.Close();
                var contentType = "APPLICATION/octet-stream";
                var fileName = Path.GetFileName(filePath);
                FileStreamResult fsTemp = File(ms, contentType, fileName);
                fsTemp.FileStream.Position = 0;
                return fsTemp;
            }
            return BadRequest();
        }

        [HttpGet("size/{dbID}")]
        public ActionResult GetDBWebSize(string dbID)
        {
            string filePath = Path.Combine(WebAuthHelper.MainFolder, dbID);
            filePath = Path.Combine(filePath, WebAuthHelper.DatabaseWeb);
            FileInfo fi = new FileInfo(filePath);
            return Ok(fi.Length.ToString());
        }

        [HttpPost("web")]
        [DisableRequestSizeLimit]
        public async System.Threading.Tasks.Task<ActionResult> UploadAuthWebEx(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Ok("No file is selected.");

            if (!TextHelper.IsMultipartContentType(HttpContext.Request.ContentType))
                return StatusCode(415);

            string errorLog = "";
            try
            {
                Startup.ProgressAuth = 0;

                long totalBytes = file.Length;
                ContentDispositionHeaderValue contentDispositionHeaderValue =
                        ContentDispositionHeaderValue.Parse(file.ContentDisposition);
                string filename = contentDispositionHeaderValue.FileName.Trim('"');
                byte[] buffer = new byte[16 * 1024];

                using (FileStream output = System.IO.File.Create(WebAuthHelper.AuthWebFilePath))
                {
                    using (Stream input = file.OpenReadStream())
                    {
                        long totalReadBytes = 0;
                        int readBytes;

                        while ((readBytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            await output.WriteAsync(buffer, 0, readBytes);
                            totalReadBytes += readBytes;
                            int progress = (int)((float)totalReadBytes / (float)totalBytes * 100.0);
                            if (Startup.ProgressAuth != progress && progress > Startup.ProgressAuth)
                            {
                                Startup.ProgressAuth = progress;
                                await System.Threading.Tasks.Task.Delay(100);
                            }
                        }
                    }
                }
                Startup.ProgressAuth = 0;
            }
            catch (Exception ex)
            {
                errorLog = ex.ToString();
            }
            finally
            {

            }

            if (errorLog == "")
                return Ok();
            else
                return BadRequest(errorLog);
        }

        [HttpGet("progress")]
        public ActionResult Progress()
        {
            int progress = Startup.ProgressAuth;

            return Content(progress.ToString());
        }


        [HttpPost("web/db/{dbID}")]
        [DisableRequestSizeLimit]
        public async System.Threading.Tasks.Task<ActionResult> UploadDBWebEx(string dbID, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Ok("No file is selected.");

            if (!TextHelper.IsMultipartContentType(HttpContext.Request.ContentType))
                return StatusCode(415);

            string errorLog = "";
            try
            {
                if (!Startup.ProgressDBDic.ContainsKey(dbID))
                    Startup.ProgressDBDic.Add(dbID, 0);
                Startup.ProgressDBDic[dbID] = 0;
                
                long totalBytes = file.Length;
                ContentDispositionHeaderValue contentDispositionHeaderValue =
                        ContentDispositionHeaderValue.Parse(file.ContentDisposition);
                string filename = contentDispositionHeaderValue.FileName.Trim('"');
                byte[] buffer = new byte[16 * 1024];
                string filePath = Path.Combine(WebAuthHelper.MainFolder, dbID);
                filePath = Path.Combine(filePath, WebAuthHelper.DatabaseWeb);
                using (FileStream output = System.IO.File.Create(filePath))
                {
                    using (Stream input = file.OpenReadStream())
                    {
                        long totalReadBytes = 0;
                        int readBytes;

                        while ((readBytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            await output.WriteAsync(buffer, 0, readBytes);
                            totalReadBytes += readBytes;
                            int progress = (int)((float)totalReadBytes * 100 / (float)totalBytes);
                            if (Startup.ProgressDBDic[dbID] != progress && progress > Startup.ProgressDBDic[dbID])
                            {
                                Startup.ProgressDBDic[dbID] = progress;
                                await System.Threading.Tasks.Task.Delay(100);
                            }
                        }
                    }
                }
                Startup.ProgressDBDic.Remove(dbID);
            }
            catch (Exception ex)
            {
                errorLog = ex.ToString();
            }
            finally
            {

            }

            if (errorLog == "")
                return Ok();
            else
                return BadRequest(errorLog);
        }

        [HttpGet("dbProgress/{dbID}")]
        public ActionResult ProgressDB(string dbID)
        {
            int progress = 0;
            if (Startup.ProgressDBDic.ContainsKey(dbID))
                progress = Startup.ProgressDBDic[dbID];

            return Content(progress.ToString());
        }

        [HttpPut()]
        public ActionResult UpdateAuth()
        {
            string errorLog = "";
            try
            {
                WebAuthHelper.UpdateErrorMessage(WebAuthHelper.AuthWebFilePath);
                AuthEngine.UpdateLocation(WebAuthHelper.AuthWebFilePath, _passCodeLicenser);
                WebAuthHelper.UpdateLanguage(WebAuthHelper.AuthWebFilePath);
            }
            catch (Exception ex)
            {
                errorLog = ex.ToString();
            }
            finally
            {

            }
            if (errorLog == "")
                return Ok();
            else
                return BadRequest(errorLog);
        }

        [HttpGet("language")]
        public ActionResult GetLanguage()
        {
            string json = "";
            string errorLog = "";
            try
            {
                json = DataHelper.GetJsonTable(Utilities.LanguageTranslationUtil.LanguageDT);
            }
            catch (Exception ex)
            {
                errorLog = ex.ToString();
            }
            finally
            {

            }
            if (errorLog == "")
                return Ok(json);
            else
                return BadRequest(errorLog);
        }

        [HttpGet("message")]
        public ActionResult GetMessage()
        {
            string json = "";
            string errorLog = "";
            try
            {
                json = DataHelper.GetJsonTable(Utilities.MessageBoxUtil.MessageDT);
            }
            catch (Exception ex)
            {
                errorLog = ex.ToString();
            }
            finally
            {

            }
            if (errorLog == "")
                return Ok(json);
            else
                return BadRequest(errorLog);
        }

        [HttpGet("region")]
        public ActionResult GetRegions()
        {
            string json = "";
            string errorLog = "";
            try
            {
                Dictionary<string, string> dicOutput = WebAuthHelper.GetRegionDic();
                json = JsonConvert.SerializeObject(dicOutput);
            }
            catch (Exception ex)
            {
                errorLog = ex.ToString();
            }
            finally
            {

            }
            if (errorLog == "")
                return Ok(json);
            else
                return BadRequest(errorLog);
        }

        [HttpGet("country/{regionCode}")]
        public ActionResult GetCountry(string regionCode)
        {
            string json = "";
            string errorLog = "";
            try
            {
                Dictionary<string, string> dicOutput = WebAuthHelper.GetCountryDic(regionCode);
                json = JsonConvert.SerializeObject(dicOutput);
            }
            catch (Exception ex)
            {
                errorLog = ex.ToString();
            }
            finally
            {

            }
            if (errorLog == "")
                return Ok(json);
            else
                return BadRequest(errorLog);
        }

        [HttpGet("city/{countryCode}")]
        public ActionResult GetCity(string countryCode)
        {
            string json = "";
            string errorLog = "";
            try
            {
                Dictionary<string, string> dicOutput = WebAuthHelper.GetCityDic(countryCode);
                json = JsonConvert.SerializeObject(dicOutput);
            }
            catch (Exception ex)
            {
                errorLog = ex.ToString();
            }
            finally
            {

            }
            if (errorLog == "")
                return Ok(json);
            else
                return BadRequest(errorLog);
        }
    }
}
