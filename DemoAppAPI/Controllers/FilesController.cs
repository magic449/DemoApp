using DemoAppAPI.DAL;
using DemoAppAPI.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic.FileIO;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Xml.Linq;

namespace DemoAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private IConfiguration configuration;
        public FilesController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpPost("uploadFile")]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<IActionResult> PostFileAsync([FromForm] UploadRequestDto request)
        {
            try
            {
                //var fileDetails = new FileDetails()
                //{
                //    ID = 0,
                //    FileName = fileData.FileName,
                //    FileType = fileType,
                //};
                var claimsPrincipal = HttpContext.User;
                string userName = "";

                if (claimsPrincipal.Identity.IsAuthenticated)
                {
                    var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Often the user ID
                     userName = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value; // Often the username
                   
                }
                byte[] res;
                using (var stream = new MemoryStream())
                {
                    request.File.CopyTo(stream);
                     res = stream.ToArray();
                }

                var uploadedFile = new UploadedFile
                {
                    FileName = request.File.FileName, // Or generate a safe name if needed
                    ContentType = request.File.ContentType,
                    FileSize = request.File.Length,
                    FileData = res,
                    UploadDate = DateTime.UtcNow,
                    userName = userName
                };

                //var result = dbContextClass.FileDetails.Add(fileDetails);
                //await dbContextClass.SaveChangesAsync();
                FileDAL fileDAL = new FileDAL(configuration);
                fileDAL.addFile(uploadedFile);
                return Ok();

            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("download-file-ado")] // Endpoint for downloading
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize]
        public async Task<IActionResult> DownloadFileFromDatabaseAdo(string nameFile)
        {
            byte[] fileBytes = null;
            string fileName = string.Empty;
            string contentType = string.Empty;

            try {

                string userName = "";
                var claimsPrincipal = HttpContext.User;
                if (claimsPrincipal.Identity.IsAuthenticated)
                {
                    var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Often the user ID
                    userName = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value; // Often the username

                }

                string sql_1 = "SELECT FileId FROM [DemoDB].[dbo].[UploadedFiles] WHERE userId = @userName";
            bool res = checkUserFile(userName);
            if (res == true)
            {
                return BadRequest("User does not have access to file");
            }

            
                using (SqlConnection connection = new SqlConnection(configuration.GetValue<string>("ConnectionStrings:DefaultConnection")))
                {
                    await connection.OpenAsync();

                    // Select the necessary columns: FileName, ContentType, and the binary FileData
                    string sql = "SELECT top 1 [FileName], [ContentType], [FileData] FROM [dbo].[UploadedFiles] WHERE [FileName] = @FileId order by UploadDate desc";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@FileId", fileName); // Use parameterized query for safety

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync()) // Check if a record was found
                            {
                                fileName = reader.GetString(0);      // Column 0: FileName
                                contentType = reader.GetString(1);   // Column 1: ContentType
                                fileBytes = (byte[])reader.GetValue(2); // Column 2: FileData (VARBINARY(MAX))
                            }
                        }
                    }
                }

                if (fileBytes == null || fileBytes.Length == 0)
                {
                   // _logger.LogWarning($"File with ID {fileId} not found or is empty.");
                    return NotFound("File not found or is empty.");
                }

                // Return the file using FileContentResult
                // This will set the appropriate Content-Type header and trigger a download in the browser.
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, $"Error downloading file with ID {fileId} using ADO.NET.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while downloading the file.");
            }
        }

        [HttpGet("DownloadFileVersion")] // Endpoint for downloading
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize]
        public async Task<IActionResult> DownloadFileVersion(string fileId, string version)
        {
            byte[] fileBytes = null;
            string fileName = string.Empty;
            string contentType = string.Empty;
            var claimsPrincipal = HttpContext.User;
            string userName = "";

            if (claimsPrincipal.Identity.IsAuthenticated)
            {
                var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Often the user ID
                userName = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value; // Often the username

            }

            try
            {
                using (SqlConnection connection = new SqlConnection(configuration.GetValue<string>("ConnectionStrings:DefaultConnection")))
                {
                    await connection.OpenAsync();




                    string sql_1 = "SELECT FileId FROM [DemoDB].[dbo].[UploadedFiles] WHERE userId = @userName";
                    bool res = checkUserFile(userName);
                    if(res == true)
                    {
                        return BadRequest("User does not have access to file");
                    }
                    using (SqlCommand command = new SqlCommand(sql_1, connection))
                    {
                       
                        command.Parameters.AddWithValue("@userName", userName);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync()) // Check if a record was found
                            {
                                fileName = reader.GetString(0);      // Column 0: FileName
                                contentType = reader.GetString(1);   // Column 1: ContentType
                                fileBytes = (byte[])reader.GetValue(2); // Column 2: FileData (VARBINARY(MAX))
                            }
                        }
                    }

                    // Select the necessary columns: FileName, ContentType, and the binary FileData
                    string sql = "SELECT [FileName], [ContentType], [FileData] FROM [dbo].[UploadedFiles] " +
                        " WHERE [FileName] = @FileId order by UploadDate desc " +
                        "offset @version rows fetch next 1 rows only";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@FileId", fileId); // Use parameterized query for safety
                        command.Parameters.AddWithValue("@version", Convert.ToInt32(version) - 1);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync()) // Check if a record was found
                            {
                                fileName = reader.GetString(0);      // Column 0: FileName
                                contentType = reader.GetString(1);   // Column 1: ContentType
                                fileBytes = (byte[])reader.GetValue(2); // Column 2: FileData (VARBINARY(MAX))
                            }
                        }
                    }
                }

                if (fileBytes == null || fileBytes.Length == 0)
                {
                    // _logger.LogWarning($"File with ID {fileId} not found or is empty.");
                    return NotFound("File not found or is empty.");
                }

                // Return the file using FileContentResult
                // This will set the appropriate Content-Type header and trigger a download in the browser.
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, $"Error downloading file with ID {fileId} using ADO.NET.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while downloading the file.");
            }
        }

        private bool checkUserFile(string userName)
        {
            string s = configuration.GetValue<string>("ConnectionStrings:DefaultConnection");
            string sql = "SELECT FileId FROM [DemoDB].[dbo].[UploadedFiles]  WHERE userId =@userName ";
            using (SqlConnection connection = new SqlConnection(s))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    //command.CommandType = CommandType.;

                    // Add input parameter
                    // Always use AddWithValue or SqlParameter constructor to prevent SQL Injection
                    command.Parameters.AddWithValue("@userName", userName);
                   
                    DataSet dataSet = new DataSet();

                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            try
                            {
                                adapter.Fill(dataSet); // Fills all result sets into separate DataTables within the DataSet
                                
                                string status = "";
                                if (dataSet.Tables.Count > 0)
                                {
                                  if(dataSet.Tables[0].Rows.Count > 0){
                                        return true;
                                    }
                                }
                                
                            }
                            catch (Exception ex)
                            {

                                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                       
                    }
                    return false;

                }
            }
        }

    }
}
