using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J_SETUP_SQL.Controllers.Setup
{
    [Route("api/[controller]")]
    public class SiteSetController : Controller
    {
        private IConfiguration _config;
        private ILogger<SiteSetController> _logger; 

        public SiteSetController(IConfiguration config
            , ILogger<SiteSetController> logger )
        {
            _config = config;
            _logger = logger; 
        }


        /// <summary>
        /// 신규 사이트 생성
        /// http://localhost:5000/api/tgboard/Set
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        [Consumes("application/json")]
        public IActionResult Set([FromBody]SiteSet model)
        {
            StringBuilder line = new StringBuilder(); string temp = "";
            if (model == null) model = new SiteSet();
            string ConnectionString = string.Format("data source={0};initial catalog={1};user id={2};password={3};MultipleActiveResultSets=true;"
                , model.Database + "", model.Databasename + "", model.Databaseid + "", model.Databasepassword + "");
            //_logger.LogError($"ConnectionString1 : {Newtonsoft.Json.JsonConvert.SerializeObject(model).ToString()}");
            //_logger.LogError($"ConnectionString2 : {ConnectionString}");
            model.Databasename = ""; model.Databasepassword = ""; model.Userpassword = ""; model.Database = "";

            #region [icon]
            Boolean icon = false;
            string ServerVersion = "";
            using (var l_oConnection = new SqlConnection(ConnectionString + ";Connection Timeout=3000"))
            {
                try
                {
                    l_oConnection.Open();
                    //_logger.LogError("ServerVersion {0}", l_oConnection.ServerVersion);
                    //_logger.LogError("State {0}", l_oConnection.State);
                    icon = true;
                    ServerVersion = l_oConnection.ServerVersion;
                    ServerVersion = ServerVersion.IndexOf('.') > 0 ? ServerVersion.Substring(0, ServerVersion.IndexOf('.')) : ServerVersion;
                }
                catch (SqlException err1)
                {
                    _logger.LogError("입력 에러1: " + err1.ToString());
                    icon = false;
                }
                finally
                {
                    l_oConnection.Close();
                    l_oConnection.Dispose();
                }
            }
            if (!icon)
            {
                model.Database = "FALSE";
                model.Databasename = "Failed to connect to SQL Server.";
                return Ok(model);
            }
            //_logger.LogError("ServerVersion: {0}", ServerVersion);


            if (icon)
            {
                if (Util.Cint(ServerVersion) < 10)
                {
                    model.Database = "FALSE";
                    model.Databasename = "The version of SQL Server is not supported.";
                    icon = false;
                }
                if (Util.Cint(ServerVersion) > 14)
                {
                    ServerVersion = "14";
                }
            }

            //10.0    2008    SQL Server 2008 Katmai  661
            //10.25   2010    Azure SQL database(initial release)    Cloud database or CloudDB
            //10.50   2010    SQL Server 2008 R2 Kilimanjaro(aka KJ)    665
            //11.0    2012    SQL Server 2012 Denali  706
            //12.0    2014    SQL Server 2014 Hekaton 782
            //13.0    2016    SQL Server 2016 - 852
            //14.0    2017    SQL Server 2017 Helsinki    869


            if (icon && _config.GetSection("ConnectionStrings:DefaultConnection").Value.ToString().Equals(""))
            {
                _config["ConnectionStrings:DefaultConnection"] = ConnectionString;

                string contentRoot = _config["contentRoot"];
                //appsettings.json
                //appsettings.Development.json

                //_logger.LogError($"There was an error: {_config["ConnectionStrings:DefaultConnection"]}"); 
                //_logger.LogError($"json : {JsonConvert.SerializeObject(_config)}");
                //_logger.LogError($"json : {JsonConvert.SerializeObject(_app)}");

                model.Database = "SUC";
                if (Directory.Exists($"{contentRoot}/setup/{ServerVersion}"))
                {
                    DirectoryInfo di = new DirectoryInfo($"{contentRoot}/setup/{ServerVersion}");
                    foreach (FileInfo fi in di.GetFiles("*.sql"))
                    { 
                        line = new StringBuilder();
                        using (StreamReader sr = fi.OpenText())
                        {
                            while (sr.Peek() >= 0)
                            {
                                temp = sr.ReadLine();
                                if (temp.ToUpperInvariant().Trim() == "GO")
                                {
                                    if (line.ToString().ToUpperInvariant().Trim().Equals("SET ANSI_NULLS ON")
                                        || line.ToString().ToUpperInvariant().Trim().Equals("SET QUOTED_IDENTIFIER ON"))
                                    {
                                        //_logger.LogError($"not __ : {line.ToString()} ");
                                    }
                                    else if (line.ToString().Length > 10)
                                    {
                                        //_logger.LogError($"proc __ : {line.ToString()} ");
                                        using (SqlConnection connection = new SqlConnection(ConnectionString))
                                        {
                                            try
                                            {
                                                SqlCommand command = new SqlCommand(
                                                    line.ToString()
                                                    .Replace("@{Sitename}", model.Sitename)
                                                    .Replace("@{Userid}", model.Userid)
                                                    .Replace("@{Userpassword}", model.Userpassword)
                                                    .Replace("@{Useremail}", model.Useremail)
                                                    , connection);
                                                command.Connection.Open();
                                                command.ExecuteNonQuery();
                                            }
                                            catch (SqlException err1)
                                            {
                                                _logger.LogError("Sql Execute: " + err1.ToString());
                                            }
                                            finally
                                            {
                                                connection.Close();
                                                connection.Dispose();
                                            }
                                        }
                                    }
                                    line.Clear();
                                    line = new StringBuilder();
                                }
                                else
                                {
                                    line.Append(temp + "\n");
                                }
                            }
                        } 
                    }
                    model.Database = "SUC";
                    model.Databasename = "Site creation completed successfully."; 
                }
                else
                {
                    model.Database = "FALSE";
                    model.Databasename = "The version of SQL Server is not supported.";
                    icon = false;
                }
                #region [make connection info]
                if (icon)
                {
                    try
                    {
                        line.Clear();
                        line = new StringBuilder(); 
                        using (StreamReader sr = new StreamReader(contentRoot + "/appsettings.json"))
                        {
                            while (sr.Peek() >= 0)
                            {
                                line.Append(sr.ReadLine() + "\n");
                            }
                        } 
                        if (line.ToString() != "")
                        {
                            AppsettingsRead pop = JsonConvert.DeserializeObject<AppsettingsRead>(line.ToString());
                            pop.ConnectionStrings.DefaultConnection = ConnectionString;  

                            using (System.IO.StreamWriter file =
                                new System.IO.StreamWriter(contentRoot + "/appsettings.json", false))
                            {
                                file.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(pop));
                                file.Close();
                                file.Dispose();
                            }
                            using (System.IO.StreamWriter file =
                                new System.IO.StreamWriter(contentRoot + "/appsettings.Development.json", false))
                            {
                                file.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(pop));
                                file.Close();
                                file.Dispose();
                            }
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("입력 에러3: " + ex.ToString());
                    }
                }
                #endregion
            }
            else
            {
                model.Database = "SUC2";
                model.Databasename = "The server settings have already been completed.";
            };
            #endregion 
            return Ok(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        [Consumes("application/json")]
        public IActionResult Set2()
        {
            SiteSet model = new SiteSet();

            return Ok(model);
        }
    }
}
