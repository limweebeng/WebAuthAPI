using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OA.WebAuthLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace WebAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        private IWebHostEnvironment _hostingEnvironment;
        private ILogger<DatabaseController> _logger;

        #region Constructor
        public DatabaseController(IWebHostEnvironment environment,
            ILogger<DatabaseController> logger)
        {
            _hostingEnvironment = environment;
            _logger = logger;
        }
        #endregion

        //Get api/database/announcement
        [HttpGet("announcement/{dbID}/{width}/{height}")]
        public ActionResult GetAnnouncement(string dbID, string width, string height)
        {
            string json = "";
            string errorLog = "";
            int width1 = 1024;
            int height1 = 480;
            if (int.TryParse(width, out int width2))
            {
                if (width1 == width2)
                    width1 = 0;
                else
                    width1 = width2;
            }
            if (int.TryParse(height, out int height2))
            {
                if (height1 == height2)
                    height1 = 0;
                else
                    height1 = height2;

            }
            try
            {
                List<string> list = WebDatabaseHelper.GetImageBase64List(dbID, width1, height1);
                ArrayList aList = new ArrayList(list);
                json = JsonConvert.SerializeObject(aList);
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
