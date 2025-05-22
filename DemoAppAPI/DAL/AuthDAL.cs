using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DemoAppAPI.DAL
{
    public class AuthDAL
    {
        IConfiguration _configuration;
        public AuthDAL(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string register(string userName,string password)
        {
            
            string spName = "dbo.Register";

            string s = _configuration.GetValue<string>("ConnectionStrings:DefaultConnection");
            using (SqlConnection connection = new SqlConnection(s))
            {
                using (SqlCommand command = new SqlCommand(spName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add input parameter
                    // Always use AddWithValue or SqlParameter constructor to prevent SQL Injection
                    command.Parameters.AddWithValue("@userName", userName);
                    command.Parameters.AddWithValue("@password", password);
                    DataSet dataSet = new DataSet();

                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            try
                            {
                                adapter.Fill(dataSet); // Fills all result sets into separate DataTables within the DataSet
                                Console.WriteLine($"Filled DataSet with {dataSet.Tables.Count} table(s) from '{spName}'.");
                            }
                            catch (Exception ex)
                            {
                                return ex.Message;
                                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                            }
                        }
                    }                    
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error executing {spName}: {ex.Message}");
                    }
                    return "success";
                    
                }
            }
           
        }

        public bool login(string uname,string password)
        {
            string spName = "dbo.sp_Login";

            string s = _configuration.GetValue<string>("ConnectionStrings:DefaultConnection");
            using (SqlConnection connection = new SqlConnection(s))
            {
                using (SqlCommand command = new SqlCommand(spName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add input parameter
                    // Always use AddWithValue or SqlParameter constructor to prevent SQL Injection
                    command.Parameters.AddWithValue("@userName", uname);
                    command.Parameters.AddWithValue("@PlainPassword", password);
                    DataSet dataSet = new DataSet();

                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            try
                            {
                                adapter.Fill(dataSet); // Fills all result sets into separate DataTables within the DataSet
                                Console.WriteLine($"Filled DataSet with {dataSet.Tables.Count} table(s) from '{spName}'.");
                                string status = "";
                                if(dataSet.Tables.Count > 0)
                                {
                                   status = Convert.ToString(dataSet.Tables[0].Rows[0]["LoginStatus"]);
                                }
                                if (status.ToLower().Contains("success"))
                                {
                                    return true;
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
                        Console.WriteLine($"Error executing {spName}: {ex.Message}");
                    }
                    return false;

                }
            }

        }
        public string? generateToken(string userName, string password)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userName),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, "User") // Example role
                // Add more claims as needed (e.g., user ID, specific permissions)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30), // Token expires in 30 minutes
                signingCredentials: creds
            );

            string? jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;

        }
    }
}
