using DemoAppAPI.Model;
using System.Data.SqlClient;

namespace DemoAppAPI.DAL
{
    public class FileDAL
    {
        private IConfiguration _configuration;
        public FileDAL(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void addFile(UploadedFile file)
        {
            try
            {
                Guid newFileId = Guid.NewGuid();
                using (SqlConnection connection = new SqlConnection(_configuration.GetValue<string>("ConnectionStrings:DefaultConnection")))
                {
                    connection.Open();

                    string sql = @"
                        INSERT INTO [dbo].[UploadedFiles]
                        ([FileId], [FileName], [ContentType], [FileSize], [FileData], [UploadDate], [userId])
                        VALUES
                        (@FileId, @FileName, @ContentType, @FileSize, @FileData, @UploadDate, @userId)";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@FileId", newFileId);
                        command.Parameters.AddWithValue("@FileName", file.FileName);
                        command.Parameters.AddWithValue("@ContentType", file.ContentType);
                        command.Parameters.AddWithValue("@FileSize", file.FileSize);
                        command.Parameters.AddWithValue("@FileData", file.FileData); // Pass the byte array
                        command.Parameters.AddWithValue("@UploadDate", DateTime.UtcNow);
                        command.Parameters.AddWithValue("@userId", file.userName);

                         command.ExecuteNonQuery();
                    }
                }

             

                //return Ok(new
                //{
                //    message = "PDF uploaded to database successfully using ADO.NET!",
                //    fileId = newFileId,
                //    fileName = file.FileName,
                //    fileSize = file.Length
                //});
            }
            catch (Exception ex)
            {
                //_logger.LogError($"Error uploading file to database using ADO.NET: {ex.Message}");
                //return StatusCode(StatusCodes.Status500InternalServerError, $"Error uploading file: {ex.Message}");
            }
        }
    }
}
