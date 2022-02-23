using DataShared;
using Helper;
using Ionic.Zip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OA.WebAuthLibrary;
using OA.WebAuthLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace WebAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DrawingController : ControllerBase
    {
        private string _passCodeLicenser = DataShared.Properties.Resources.licenser;
        private IWebHostEnvironment _hostingEnvironment;
        private ILogger<DrawingController> _logger;

        #region Constructor
        public DrawingController(IWebHostEnvironment environment,
            ILogger<DrawingController> logger)
        {
            _hostingEnvironment = environment;
            _logger = logger;
        }
        #endregion

        [HttpPost("fileList")]
        [Authorize]
        public ActionResult GetFileList([FromBody] Dictionary<string, string> dicInput)
        {
            string json = "";
            string errorLog = "";

            try
            {
                string dbID = null;
                string ID = null;
                string extension = null;

                if (dicInput.ContainsKey("dbID"))
                {
                    dbID = dicInput["dbID"];
                    dicInput.Remove("dbID");
                }
                if (dicInput.ContainsKey("ID"))
                {
                    ID = dicInput["ID"];
                    dicInput.Remove("ID");
                }
                if (dicInput.ContainsKey("extension"))
                {
                    extension = dicInput["extension"];
                    dicInput.Remove("extension");
                }

                if (dbID != null && ID != null && extension != null)
                {
                    string directory = Path.Combine(WebAuthHelper.MainFolder, dbID);
                    directory = Path.Combine(directory, ID);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                    List<string> stList = Directory.GetFiles(directory, "*." + extension).Select(obj => Path.GetFileName(obj)).ToList();
                    Dictionary<string, string> dicOut = new Dictionary<string, string>();
                    string stFileList = JsonConvert.SerializeObject(stList);
                    dicOut.Add("fileList", stFileList);
                    json = JsonConvert.SerializeObject(dicOut);
                }
                
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

        [HttpPost("fileDetails")]
        [Authorize]
        public ActionResult GetFileDetails([FromBody] Dictionary<string, string> dicInput)
        {
            string json = "";
            string errorLog = "";
            string filePath1 = null;
            try
            {
                string dbID = null;
                string ID = null;
                string fileName = null;

                if (dicInput.ContainsKey("dbID"))
                {
                    dbID = dicInput["dbID"];
                    dicInput.Remove("dbID");
                }
                if (dicInput.ContainsKey("ID"))
                {
                    ID = dicInput["ID"];
                    dicInput.Remove("ID");
                }
                if (dicInput.ContainsKey("fileName"))
                {
                    fileName = dicInput["fileName"];
                    dicInput.Remove("fileName");
                }
                string stKey = dbID;
                ProjectModel pm = null;
                string errorCode = "";
                Dictionary<string, string> dicOut = new Dictionary<string, string>();

                if (Startup.ProjectModelDic.ContainsKey(stKey))
                {
                    string stValue = Startup.ProjectModelDic[stKey];
                    pm = JsonConvert.DeserializeObject<ProjectModel>(stValue);
                }
                else
                {
                    pm = WebAuthAction.GetProjectModel(stKey);
                    string json1 = JsonConvert.SerializeObject(pm);
                    Startup.ProjectModelDic.Add(stKey, json1);
                }
                if (pm != null)
                {
                    string zipCode = null;
                    if (pm.infoDic.ContainsKey("zipCode"))
                        zipCode = pm.infoDic["zipCode"];

                    if (dbID != null && ID != null && fileName != null
                          && zipCode != null)
                    {
                        string fileDirectory = Path.Combine(WebAuthHelper.MainFolder, dbID);
                        fileDirectory = Path.Combine(fileDirectory, ID);
                        if (!Directory.Exists(fileDirectory))
                            Directory.CreateDirectory(fileDirectory);
                        string filePath = Path.Combine(fileDirectory, fileName);

                        if (System.IO.File.Exists(filePath))
                        {
                            filePath1 = filePath.Remove(filePath.Length - 3, 3);
                            if (!System.IO.File.Exists(filePath1))
                            {
                                string fileName1 = Path.GetFileName(filePath1);
                                filePath1 = ZipHelper.ExtractFile(filePath, fileName1, dbID, fileDirectory);
                            }
                            ProjectHelper.ProjectHelper prjHelper = new ProjectHelper.ProjectHelper(filePath1, zipCode);
                            prjHelper.Initialize(out errorCode);
                            if (errorCode == "")
                            {
                                string stPrjInfo = JsonConvert.SerializeObject(prjHelper.prjInfo);
                                dicOut.Add("prjInfo", stPrjInfo);
                            }
                        }
                        else
                            errorCode = "err_invalidreqres";
                    }
                    else
                    {
                        errorCode = "err_invalidreqres";
                    }
                }
                else
                {
                    errorCode = "err_invalidreqres";
                }
                dicOut.Add("errorCode", errorCode);
                json = JsonConvert.SerializeObject(dicOut);
            }
            catch (Exception ex)
            {
                errorLog = ex.ToString();
            }
            finally
            {
                if (filePath1 != null)
                {
                    if (System.IO.File.Exists(filePath1))
                        System.IO.File.Delete(filePath1);
                }
            }
            if (errorLog == "")
                return Ok(json);
            else
                return BadRequest(errorLog);

        }

        [HttpDelete()]
        [Authorize]
        public ActionResult DeleteFile([FromBody] Dictionary<string, string> dicInput)
        {
            string json = "";
            string errorLog = "";
            try
            {
                string dbID = null;
                string ID = null;
                string fileName = null;
                string errorCode = "";
                Dictionary<string, string> dicOut = new Dictionary<string, string>();
                if (dicInput.ContainsKey("dbID"))
                {
                    dbID = dicInput["dbID"];
                    dicInput.Remove("dbID");
                }
                if (dicInput.ContainsKey("ID"))
                {
                    ID = dicInput["ID"];
                    dicInput.Remove("ID");
                }
                if (dicInput.ContainsKey("fileName"))
                {
                    fileName = dicInput["fileName"];
                    dicInput.Remove("fileName");
                }


                if (dbID != null && ID != null && fileName != null)
                {
                    string filePath = Path.Combine(WebAuthHelper.MainFolder, dbID);
                    filePath = Path.Combine(filePath, ID);
                    if (!Directory.Exists(filePath))
                        Directory.CreateDirectory(filePath);
                    filePath = Path.Combine(filePath, fileName);

                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }
                else
                    errorCode = "err_invalidreqres";

                dicOut.Add("errorCode", errorCode);
                json = JsonConvert.SerializeObject(dicOut);
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

        [HttpPatch()]
        [Authorize]
        public ActionResult IsDrawingExists([FromBody] Dictionary<string, string> dicInput)
        {
            string errorLog = "";
            bool isExist = false;
            try
            {
                string dbID = null;
                string ID = null;
                string fileName = null;
                string orderNumber = null;
                string jobAHUName = null;

                if (dicInput.ContainsKey("dbID"))
                {
                    dbID = dicInput["dbID"];
                    dicInput.Remove("dbID");
                }
                if (dicInput.ContainsKey("ID"))
                {
                    ID = dicInput["ID"];
                    dicInput.Remove("ID");
                }
                if (dicInput.ContainsKey("fileName"))
                {
                    fileName = dicInput["fileName"];
                    dicInput.Remove("fileName");
                }
                if (dicInput.ContainsKey("orderNumber"))
                {
                    orderNumber = dicInput["orderNumber"];
                    dicInput.Remove("orderNumber");
                }
                if (dicInput.ContainsKey("jobAHUName"))
                {
                    jobAHUName = dicInput["jobAHUName"];
                    dicInput.Remove("jobAHUName");
                }
                string ext = Path.GetExtension(fileName);
                if (!ext.EndsWith("MPD"))
                {
                    string ext1 = ext + "MPD";
                    ext1 = ext1.Replace(".", "");
                    fileName = Path.ChangeExtension(fileName, ext1);
                }
                Dictionary<string, string> dicOut = new Dictionary<string, string>();
                if (dbID != null && ID != null && fileName != null
                    && orderNumber != null && jobAHUName != null)
                {
                    string fileDirectory = Path.Combine(WebAuthHelper.MainFolder, dbID);
                    fileDirectory = Path.Combine(fileDirectory, ID);
                    if (!Directory.Exists(fileDirectory))
                        Directory.CreateDirectory(fileDirectory);

                    string filePath = Path.Combine(fileDirectory, fileName);


                    string jobName = orderNumber + "-" + jobAHUName;
                    string zipFolderEntry = Path.Combine("Drawing", jobName);
                    isExist = ZipHelper.FolderExist(zipFolderEntry, filePath) != null;
                }

            }
            catch (Exception ex)
            {
                errorLog = ex.ToString();
            }
            finally
            {

            }

            if (errorLog == "")
                return Ok(isExist);
            else
                return BadRequest(errorLog);
        }

        [HttpPost("upload/{dbID}/{ID}")]
        [DisableRequestSizeLimit]
        [Authorize]
        public async System.Threading.Tasks.Task<ActionResult> UploadFile(string dbID, string ID, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Ok("No file is selected.");

            if (!TextHelper.IsMultipartContentType(HttpContext.Request.ContentType))
                return StatusCode(415);

            string json = "";
            string errorLog = "";
            string filePath = null;

            try
            {
                string stKey = dbID;
                ProjectModel pm = null;

                if (Startup.ProjectModelDic.ContainsKey(stKey))
                {
                    string stValue = Startup.ProjectModelDic[stKey];
                    pm = JsonConvert.DeserializeObject<ProjectModel>(stValue);
                }
                else
                {
                    pm = WebAuthAction.GetProjectModel(stKey);
                    string json1 = JsonConvert.SerializeObject(pm);
                    Startup.ProjectModelDic.Add(stKey, json1);
                }
                if (pm != null)
                {
                    if (!Startup.ProgressFileDic.ContainsKey(dbID))
                        Startup.ProgressFileDic.Add(dbID, 0);
                    Startup.ProgressFileDic[dbID] = 0;

                    long totalBytes = file.Length;
                    ContentDispositionHeaderValue contentDispositionHeaderValue =
                            ContentDispositionHeaderValue.Parse(file.ContentDisposition);
                    string filename = contentDispositionHeaderValue.FileName.Trim('"');
                    byte[] buffer = new byte[16 * 1024];
                    filePath = Path.Combine(WebAuthHelper.MainFolder, dbID);
                    filePath = Path.Combine(filePath, ID);
                    string directory = filePath;
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                    filePath = Path.Combine(directory, filename);

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
                                int progress = (int)((float)totalReadBytes / (float)totalBytes * 100.0);
                                if (Startup.ProgressFileDic[dbID] != progress && progress > Startup.ProgressFileDic[dbID])
                                {
                                    Startup.ProgressFileDic[dbID] = progress;
                                    await System.Threading.Tasks.Task.Delay(100);
                                }
                            }
                        }
                    }
                    Startup.ProgressFileDic[dbID] = 0;

                    string zipCode = null;
                    if (pm.infoDic.ContainsKey("zipCode"))
                        zipCode = pm.infoDic["zipCode"];

                    string errorCode = "";
                    Dictionary<string, string> dicOut = new Dictionary<string, string>();
                    if (zipCode != null)
                    {
                        if (System.IO.File.Exists(filePath))
                        {
                            string ext = Path.GetExtension(filePath);
                            string ext1 = ext + "MPD";
                            ext1 = ext1.Replace(".", "");
                            string newfilePath = System.IO.Path.ChangeExtension(filePath, ext1);
                            if (System.IO.File.Exists(newfilePath))
                                System.IO.File.Delete(newfilePath);

                            DataHelper.CreateZipFile(filePath, newfilePath, dbID);
                            ProjectHelper.ProjectHelper prjHelper = new ProjectHelper.ProjectHelper(filePath, zipCode);
                            prjHelper.Initialize(out errorCode);
                            if (errorCode == "")
                            {
                                string stPrjInfo = JsonConvert.SerializeObject(prjHelper.prjInfo);
                                dicOut.Add("prjInfo", stPrjInfo);
                            }
                            else
                            {
                                if (System.IO.File.Exists(newfilePath))
                                    System.IO.File.Delete(newfilePath);
                            }
                        }
                        else
                            errorCode = "err_invalidreqres";
                    }
                    else
                        errorCode = "err_invalidreqres";

                    dicOut.Add("errorCode", errorCode);
                    json = JsonConvert.SerializeObject(dicOut);
                }

            }
            catch (Exception ex)
            {
                errorLog = ex.ToString();
            }
            finally
            {
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            if (errorLog == "")
                return Ok(json);
            else
                return BadRequest(errorLog);
        }

        [HttpPost("generate")]
        [Authorize]
        public ActionResult GenerateDrawing([FromBody] Dictionary<string, string> dicInput)
        {
            string json = "";
            string errorLog = "";
            string tempFolder = null;
            try
            {
                string dbID = null;
                string ID = null;
                string fileName = null;
                string orderNumber = null;
                string jobAHUName = null;
                string jobAHUCode = null;
                bool listDrawingStatus = false;

                if (dicInput.ContainsKey("dbID"))
                {
                    dbID = dicInput["dbID"];
                    dicInput.Remove("dbID");
                }
                if (dicInput.ContainsKey("ID"))
                {
                    ID = dicInput["ID"];
                    dicInput.Remove("ID");
                }
                if (dicInput.ContainsKey("fileName"))
                {
                    fileName = dicInput["fileName"];
                    dicInput.Remove("fileName");
                }
                if (dicInput.ContainsKey("orderNumber"))
                {
                    orderNumber = dicInput["orderNumber"];
                    dicInput.Remove("orderNumber");
                }
                if (dicInput.ContainsKey("jobAHUName"))
                {
                    jobAHUName = dicInput["jobAHUName"];
                    dicInput.Remove("jobAHUName");
                }
                if (dicInput.ContainsKey("jobAHUCode"))
                {
                    jobAHUCode = dicInput["jobAHUCode"];
                    dicInput.Remove("jobAHUCode");
                }
                if (dicInput.ContainsKey("listDrawing"))
                {
                    listDrawingStatus = dicInput["listDrawing"] == "1";
                    dicInput.Remove("listDrawing");
                }


                string stKey = dbID;
                ProjectModel pm = null;
                if (Startup.ProjectModelDic.ContainsKey(stKey))
                {
                    string stValue = Startup.ProjectModelDic[stKey];
                    pm = JsonConvert.DeserializeObject<ProjectModel>(stValue);
                }
                else
                {
                    pm = WebAuthAction.GetProjectModel(stKey);
                    string json1 = JsonConvert.SerializeObject(pm);
                    Startup.ProjectModelDic.Add(stKey, json1);
                }
                if (pm != null)
                {
                    string zipCode = null;
                    if (pm.infoDic.ContainsKey("zipCode"))
                        zipCode = pm.infoDic["zipCode"];

                    Dictionary<string, string> dicOut = new Dictionary<string, string>();
                    if (dbID != null && ID != null && fileName != null
                        && zipCode != null && orderNumber != null && jobAHUName != null
                        && jobAHUCode != null)
                    {
                        string dbFolder = Path.Combine(WebAuthHelper.MainFolder, dbID);
                        dbFolder = Path.Combine(dbFolder, "database");

                        string fileDirectory = Path.Combine(WebAuthHelper.MainFolder, dbID);
                        fileDirectory = Path.Combine(fileDirectory, ID);
                        if (!Directory.Exists(fileDirectory))
                            Directory.CreateDirectory(fileDirectory);

                        string filePath = Path.Combine(fileDirectory, fileName);
                        string jobName = orderNumber + "-" + jobAHUName;
                        tempFolder = Path.Combine(fileDirectory, jobName);
                        if (!Directory.Exists(tempFolder))
                            Directory.CreateDirectory(tempFolder);

                        string newDirectory = Path.Combine(tempFolder, "database");
                        DataHelper.DirectoryCopy(dbFolder, newDirectory, true);
                        string dbFilePath = Path.Combine(newDirectory, WebAuthHelper.DatabaseWeb);

                        string locationExe = Path.Combine(WebAuthHelper.MainFolder, dbID);
                        locationExe = Path.Combine(locationExe, "dlls");
                        locationExe = Path.Combine(locationExe, "OAMetalPart.exe");

                        string inputFilePath = Path.Combine(tempFolder, "input.txt");
                        using (StreamWriter sw = System.IO.File.CreateText(inputFilePath))
                        {
                            sw.WriteLine("filePath=" + filePath);
                            sw.WriteLine("orderNumber=" + orderNumber);
                            sw.WriteLine("jobAHUName=" + jobAHUName);
                            sw.WriteLine("jobAHUCode=" + jobAHUCode);
                            sw.WriteLine("dbFilePath=" + dbFilePath);
                            sw.WriteLine("dbZipCode=" + dbID);
                            sw.WriteLine("zipCode=" + zipCode);
                        }
                        if (listDrawingStatus)
                            WebDatabaseHelper.RunExternalExe(locationExe, inputFilePath, 2);
                        else
                            WebDatabaseHelper.RunExternalExe(locationExe, inputFilePath, 0);

                        string outputFilePath = Path.Combine(tempFolder, "output.txt");
                        int count = 0;
                        while (!System.IO.File.Exists(outputFilePath))
                        {
                            if (count > 120)
                                break;
                            Thread.Sleep(1000);
                            count++;
                        }
                        if (System.IO.File.Exists(outputFilePath))
                        {
                            Dictionary<string, string> dicOutput = TextHelper.GetInputDictionary(outputFilePath, out string errorMsg);
                            if (errorMsg != "")
                            {
                                dicOut.Add("status", "0");
                                dicOut.Add("errorCode", errorMsg);
                            }
                            else
                            {
                                if (dicOutput.ContainsKey("status"))
                                    dicOut.Add("status", dicOutput["status"]);
                                if (dicOutput.ContainsKey("returnMsg"))
                                    dicOut.Add("errorCode", dicOutput["returnMsg"]);
                                if (dicOutput.ContainsKey("drawingInfoList"))
                                    dicOut.Add("drawingInfoList", dicOutput["drawingInfoList"]);
                            }
                        }
                        else
                        {
                            dicOut.Add("status", "0");
                            dicOut.Add("errorCode", "Time out");
                        }
                        json = JsonConvert.SerializeObject(dicOut);
                    }
                }
            }
            catch (Exception ex)
            {
                errorLog = ex.ToString();
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempFolder))
                {
                    if (Directory.Exists(tempFolder))
                        Directory.Delete(tempFolder, true);
                }
            }

            if (errorLog == "")
                return Ok(json);
            else
                return BadRequest(errorLog);
        }

        [HttpPost("show")]
        [Authorize]
        public ActionResult ShowDrawing([FromBody] Dictionary<string, string> dicInput)
        {
            string json = "";
            string errorLog = "";
            string tempFolder = null;
            try
            {
                string dbID = null;
                string ID = null;
                string fileName = null;
                string orderNumber = null;
                string jobAHUName = null;
                string drawingFolder = null;
                string drawingName = null;
                string drawingExtension = null;

                if (dicInput.ContainsKey("dbID"))
                {
                    dbID = dicInput["dbID"];
                    dicInput.Remove("dbID");
                }
                if (dicInput.ContainsKey("ID"))
                {
                    ID = dicInput["ID"];
                    dicInput.Remove("ID");
                }
                if (dicInput.ContainsKey("fileName"))
                {
                    fileName = dicInput["fileName"];
                    dicInput.Remove("fileName");
                }
                if (dicInput.ContainsKey("orderNumber"))
                {
                    orderNumber = dicInput["orderNumber"];
                    dicInput.Remove("orderNumber");
                }
                if (dicInput.ContainsKey("jobAHUName"))
                {
                    jobAHUName = dicInput["jobAHUName"];
                    dicInput.Remove("jobAHUName");
                }

                if (dicInput.ContainsKey("drawingFolder"))
                {
                    drawingFolder = dicInput["drawingFolder"];
                    dicInput.Remove("drawingFolder");
                }

                if (dicInput.ContainsKey("drawingName"))
                {
                    drawingName = dicInput["drawingName"];
                    dicInput.Remove("drawingName");
                }

                if (dicInput.ContainsKey("drawingExtension"))
                {
                    drawingExtension = dicInput["drawingExtension"];
                    dicInput.Remove("drawingExtension");
                }

                string stKey = dbID;
                ProjectModel pm = null;
                if (Startup.ProjectModelDic.ContainsKey(stKey))
                {
                    string stValue = Startup.ProjectModelDic[stKey];
                    pm = JsonConvert.DeserializeObject<ProjectModel>(stValue);
                }
                else
                {
                    pm = WebAuthAction.GetProjectModel(stKey);
                    string json1 = JsonConvert.SerializeObject(pm);
                    Startup.ProjectModelDic.Add(stKey, json1);
                }
                if (pm != null)
                {
                    string zipCode = null;
                    if (pm.infoDic.ContainsKey("zipCode"))
                        zipCode = pm.infoDic["zipCode"];

                    Dictionary<string, string> dicOut = new Dictionary<string, string>();
                    if (dbID != null && ID != null && fileName != null
                        && zipCode != null && orderNumber != null && jobAHUName != null
                        && drawingFolder != null && drawingName != null && drawingExtension != null)
                    {
                        string fileDirectory = Path.Combine(WebAuthHelper.MainFolder, dbID);
                        fileDirectory = Path.Combine(fileDirectory, ID);
                        if (!Directory.Exists(fileDirectory))
                            Directory.CreateDirectory(fileDirectory);

                        string filePath = Path.Combine(fileDirectory, fileName);
                        string jobName = orderNumber + "-" + jobAHUName;
                        tempFolder = Path.Combine(fileDirectory, jobName);
                        if (!Directory.Exists(tempFolder))
                            Directory.CreateDirectory(tempFolder);

                        string locationExe = Path.Combine(WebAuthHelper.MainFolder, dbID);
                        locationExe = Path.Combine(locationExe, "dlls");
                        locationExe = Path.Combine(locationExe, "OAMetalPart.exe");

                        string inputFilePath = Path.Combine(tempFolder, "input.txt");
                        using (StreamWriter sw = System.IO.File.CreateText(inputFilePath))
                        {
                            sw.WriteLine("filePath=" + filePath);
                            sw.WriteLine("orderNumber=" + orderNumber);
                            sw.WriteLine("jobAHUName=" + jobAHUName);
                            sw.WriteLine("dbZipCode=" + dbID);
                            sw.WriteLine("dirName=" + drawingFolder);
                            sw.WriteLine("fileName=" + drawingName);
                            sw.WriteLine("dwgExt=" + drawingExtension);
                        }

                        WebDatabaseHelper.RunExternalExe(locationExe, inputFilePath, 1);

                        string outputFilePath = Path.Combine(tempFolder, "output.txt");
                        int count = 0;
                        while (!System.IO.File.Exists(outputFilePath))
                        {
                            if (count > 60)
                                break;
                            Thread.Sleep(1000);
                            count++;
                        }
                        if (System.IO.File.Exists(outputFilePath))
                        {
                            Dictionary<string, string> dicOutput = TextHelper.GetInputDictionary(outputFilePath, out string errorMsg);
                            if (errorMsg != "")
                            {
                                dicOut.Add("status", "0");
                                dicOut.Add("errorCode", errorMsg);
                            }
                            else
                            {
                                if (dicOutput.ContainsKey("status"))
                                    dicOut.Add("status", dicOutput["status"]);
                                if (dicOutput.ContainsKey("returnMsg"))
                                    dicOut.Add("errorCode", dicOutput["returnMsg"]);
                            }

                            if (dicOut["status"] == "1")
                            {
                                if (drawingExtension == ".vds")
                                {
                                    if (dicOutput.ContainsKey("dwgFilePath"))
                                    { 
                                        string sts = System.IO.File.ReadAllText(dicOutput["dwgFilePath"]);
                                        dicOut.Add("vds", sts);
                                    }
                                    
                                }
                                else if (drawingExtension == ".dwg" || drawingExtension == ".pdf")
                                {
                                    string filepath = dicOutput["dwgFilePath"];
                                    System.IO.FileStream fs = new System.IO.FileStream(filepath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                                    fs.CopyTo(ms);
                                    fs.Close();
                                    var contentType = "APPLICATION/octet-stream";
                                    var filename = Path.GetFileName(filepath);
                                    FileStreamResult fsTemp = File(ms, contentType, filename);
                                    fsTemp.FileStream.Position = 0;
                                    return fsTemp;
                                }
                            }
                        }
                        else
                        {
                            dicOut.Add("status", "0");
                            dicOut.Add("errorCode", "Time out");
                        }
                        json = JsonConvert.SerializeObject(dicOut);
                    }
                }
            }
            catch (Exception ex)
            {
                errorLog = ex.ToString();
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempFolder))
                {
                    if (Directory.Exists(tempFolder))
                        Directory.Delete(tempFolder, true);
                }
            }

            if (errorLog == "")
                return Ok(json);
            else
                return BadRequest(errorLog);
        }

       
    }
}
