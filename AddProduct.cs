using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace SQLFunction
{
    public static class AddProduct
    {
        private static SqlConnection GetConnection()
        {
            //string connectionString = "Server=tcp:projectappserver.database.windows.net,1433;Initial Catalog=appdb;Persist Security Info=False;User ID=sqladmin;Password=Pa55word;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            string connectionString = Environment.GetEnvironmentVariable("SQLAZURECONNSTR_SQLConnectionString");
            return new SqlConnection(connectionString);
        }

        [FunctionName("AddProduct")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Product product = JsonConvert.DeserializeObject<Product>(requestBody);

            SqlConnection _connection = GetConnection();
            _connection.Open();

            string _statement = "INSERT INTO PRODUCTS (ProductID,ProductName,Quantity) VALUES(@PARAM1,@PARAM2,@PARAM3)";

            using (SqlCommand cmd = new SqlCommand(_statement, _connection))
            {
                cmd.Parameters.Add("@PARAM1", System.Data.SqlDbType.Int).Value = product.ProductID;
                cmd.Parameters.Add("@PARAM2", System.Data.SqlDbType.VarChar).Value = product.ProductName;
                cmd.Parameters.Add("@PARAM3", System.Data.SqlDbType.Int).Value = product.Quantity;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.ExecuteNonQuery();
            }
                return new OkObjectResult("Product has been added");

        }
    }
}
