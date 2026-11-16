using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace ProductCatalogWpf
{
    public class DatabaseService
    {
        private readonly string _connectionString = "Data Source=products.db";

        public void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Price REAL NOT NULL
                );";
            cmd.ExecuteNonQuery();
        }

        public List<Product> GetProducts(string searchTerm = "")
        {
            var products = new List<Product>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.CommandText = "SELECT Id, Name, Price FROM Products ORDER BY Id;";
            }
            else
            {
                cmd.CommandText = "SELECT Id, Name, Price FROM Products WHERE Name LIKE $term ORDER BY Id;";
                cmd.Parameters.AddWithValue("$term", $"%{searchTerm}%");
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDecimal(2)
                });
            }

            return products;
        }

        public void InsertProduct(string name, decimal price)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Products (Name, Price) VALUES ($name, $price);";
            cmd.Parameters.AddWithValue("$name", name);
            cmd.Parameters.AddWithValue("$price", price);
            cmd.ExecuteNonQuery();
        }

        public void UpdateProduct(int id, string name, decimal price)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE Products SET Name = $name, Price = $price WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$name", name);
            cmd.Parameters.AddWithValue("$price", price);
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        public void DeleteProduct(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Products WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }
    }
}