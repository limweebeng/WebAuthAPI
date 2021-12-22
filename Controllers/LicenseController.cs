using Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OA.WebAuthLibrary;
using OA.WebAuthLibrary.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LicenseController : ControllerBase
    {
        private IWebHostEnvironment _hostingEnvironment;
        private ILogger<LicenseController> _logger;

        #region Constructor
        public LicenseController(IWebHostEnvironment environment,
            ILogger<LicenseController> logger)
        {
            _hostingEnvironment = environment;
            _logger = logger;
        }
        #endregion

        //Get api/license
        [HttpGet()]
        public ActionResult Get()
        {
            return Ok("Testing");
        }

        //Post api/license/register
        [HttpPost("register/{dbID}")]
        public ActionResult Register(string dbID, [FromBody] LicenseModel license)
        {
            string errorCode = "";
            string errorLog = "";
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
                    string json = JsonConvert.SerializeObject(pm);
                    Startup.ProjectModelDic.Add(stKey, json);
                }

                if (pm != null)
                {
                    DataRow userRow = WebAuthAction.RegisterLicense(license, pm, out errorCode);
                }
                else
                {
                    errorCode = "err_server";
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
            {
                string msgDisplay = null;
                if (errorCode == "")
                {
                    msgDisplay = "'" + license.license_fullname + "', email : " + license.license_email;
                    errorCode = "success_reglicense";
                }
                else if (errorCode == "err_license_exist")
                {
                    msgDisplay = "'" + license.license_fullname + "', email : " + license.license_email;
                }
                ErrorModel em = new ErrorModel()
                {
                    errorCode = errorCode,
                    objVariable = msgDisplay
                };
                return Ok(em);
            }
            else
            {
                return BadRequest(errorLog);
            }
        }

        [HttpPost("login/{dbID}")]
        public ActionResult Login(string dbID, [FromBody] LoginCredentials creds)
        {
            string errorLog = "";
            string errorCode = "";
            string jsonTemp = "";
            Dictionary<string, string> dicOut = new Dictionary<string, string>();
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
                    string json = JsonConvert.SerializeObject(pm);
                    Startup.ProjectModelDic.Add(stKey, json);
                }

                if (pm != null)
                {
                    DataRow userRow = WebAuthAction.Login(creds, pm, out errorCode);
                    if (userRow != null)
                    {
                        DataTable dt = DataHelper.ConvertDT(userRow);
                        string json1 = DataHelper.GetJsonTable(dt);
                        dicOut.Add("userRow", json1);
                        ClaimsPrincipal principal = this.getPrincipal(userRow, dbID, Startup.JWTAuthScheme);
                        string token = this.generateJSON(principal);
                        dicOut.Add("token", token);
                    }
                }
                else
                {
                    errorCode = "err_server";
                }
            }
            catch (Exception ex)
            {
                errorLog = ex.ToString();
            }
            finally
            {
                dicOut.Add("errorCode", errorCode);
                jsonTemp = JsonConvert.SerializeObject(dicOut);
            }
            if (errorLog == "")
            {
                return Ok(jsonTemp);
            }
            else
            {
                return BadRequest(errorLog);
            }
        }

        [HttpPost("password/{dbID}")]
        public ActionResult ForgotPassword(string dbID, [FromBody] LicenseModel license)
        {
            string errorLog = "";
            string errorCode = "";
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
                    string json = JsonConvert.SerializeObject(pm);
                    Startup.ProjectModelDic.Add(stKey, json);
                }

                if (pm != null)
                {
                    WebAuthAction.ForgotPassword(license, pm, out errorCode);
                    if (errorCode == "")
                        errorCode = "success_forgotpw";
                }
                else
                    errorCode = "err_server";
            }
            catch (Exception ex)
            {
                errorLog = ex.ToString();
            }
            finally
            {

            }
            if (errorLog == "")
            {
                return Ok(errorCode);
            }
            else
            {
                return BadRequest(errorLog);
            }
        }

        [HttpPost("edit/{dbID}")]
        [Authorize]
        public ActionResult EditProfile(string dbID, [FromBody] LicenseModel license)
        {
            string errorCode = "";
            string errorLog = "";
            Dictionary<string, string> dicOut = new Dictionary<string, string>();
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
                    string json = JsonConvert.SerializeObject(pm);
                    Startup.ProjectModelDic.Add(stKey, json);
                }

                if (pm != null)
                {
                    DataRow userRow = WebAuthAction.EditProfile(license, pm, out errorCode);
                    if (userRow != null)
                    {
                        DataTable dt = DataHelper.ConvertDT(userRow);
                        string json1 = DataHelper.GetJsonTable(dt);
                        dicOut.Add("userRow", json1);
                    }
                }
                else
                {
                    errorCode = "err_server";
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
            {
                if (errorCode == "")
                {
                    errorCode = "success_save";
                }
                else
                {
                    errorCode = "err_save";
                }

                dicOut.Add("errorCode", errorCode);
                string jsonTemp = JsonConvert.SerializeObject(dicOut);
                return Ok(jsonTemp);
            }
            else
            {
                return BadRequest(errorLog);
            }
        }

        [HttpPost("editPassword/{dbID}")]
        [Authorize]
        public ActionResult EditPassword(string dbID, [FromBody] LoginCredentialsEx creds)
        {
            string errorCode = "";
            string errorLog = "";
            Dictionary<string, string> dicOut = new Dictionary<string, string>();
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
                    string json = JsonConvert.SerializeObject(pm);
                    Startup.ProjectModelDic.Add(stKey, json);
                }

                if (pm != null)
                {
                    DataRow userRow = WebAuthAction.EditPassword(creds, pm, out errorCode);
                    if (userRow != null)
                    {
                        DataTable dt = DataHelper.ConvertDT(userRow);
                        string json1 = DataHelper.GetJsonTable(dt);
                        dicOut.Add("userRow", json1);
                    }
                }
                else
                {
                    errorCode = "err_server";
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
            {
                if (errorCode == "")
                {
                    errorCode = "success_changepw";
                }
                else
                {
                    errorCode = "err_changepw";
                }

                dicOut.Add("errorCode", errorCode);
                string jsonTemp = JsonConvert.SerializeObject(dicOut);
                return Ok(jsonTemp);
            }
            else
            {
                return BadRequest(errorLog);
            }
        }

        private ClaimsPrincipal getPrincipal(DataRow userInfo, string dbID, string authScheme)
        {
            var claims = new[] {
                new Claim(ClaimTypes.Name, userInfo.Field<string>("license_email")),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(JwtRegisteredClaimNames.Email, userInfo.Field<string>("license_email")),
                new Claim("dbID", dbID)
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, authScheme));
        }

        private string generateJSON(ClaimsPrincipal principal)
        {
            var securityKey = Startup.SecurityKey;
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(DataShared.Properties.Resources.auth_security_key,
                DataShared.Properties.Resources.auth_security_key,
                principal.Claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
