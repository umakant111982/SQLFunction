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
using System.Collections.Generic;

namespace SQLFunction
{
    public static class GetProduct
    {
        private static SqlConnection GetConnection()
        {
            string connectionString = "Server=tcp:projectappserver.database.windows.net,1433;Initial Catalog=appdb;Persist Security Info=False;User ID=sqladmin;Password=Pa55word;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            return new SqlConnection(connectionString);
        }

        [FunctionName("GetProducts")]
        public static async Task<IActionResult> RunProducts(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            List<Product> _products_lst = new List<Product>();
            string _statement = "SELECT ProductID,ProductName,Quantity from Products";
            SqlConnection _sqlConnection = GetConnection();
            _sqlConnection.Open();

            SqlCommand _sqlCommand = new SqlCommand(_statement, _sqlConnection);

            using (SqlDataReader _reader = _sqlCommand.ExecuteReader())
            {
                while (_reader.Read())
                {
                    Product _product = new Product()
                    {
                        ProductID = _reader.GetInt32(0),
                        ProductName = _reader.GetString(1),
                        Quantity = _reader.GetInt32(2)
                    };
                    _products_lst.Add(_product);
                }

            }
            return new OkObjectResult(JsonConvert.SerializeObject(_products_lst));
        }

        [FunctionName("GetProduct")]
        public static async Task<IActionResult> RunProduct(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {            
            int _productID = int.Parse(req.Query["id"]);

            string _statement = String.Format("SELECT ProductID,ProductName,Quantity from Products where ProductID={0}", _productID);
            SqlConnection _sqlConnection = GetConnection();
            _sqlConnection.Open();

            SqlCommand _sqlCommand = new SqlCommand(_statement, _sqlConnection);
            Product _product = new Product();

            try
            {
                using (SqlDataReader _reader = _sqlCommand.ExecuteReader())
                {
                    _reader.Read();
                    _product.ProductID = _reader.GetInt32(0);
                    _product.ProductName = _reader.GetString(1);
                    _product.Quantity = _reader.GetInt32(2);
                    var response = _product;
                    return new OkObjectResult(JsonConvert.SerializeObject(response));
                }
            }
            catch (Exception ex)
            {
                var response = "No Records found" + ex;
                _sqlConnection.Close();
                return new OkObjectResult(response);
            }            
        }
    }
}
