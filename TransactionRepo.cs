using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MovieShop.Models;

namespace MovieShop.Repositories
{
    internal class TransactionRepo
    {
        private string connString = "Server = S24B\\SQLEXPRESS; Database = MovieShopDB; Trusted_Connection = True; TrustServerCertificate = True";

        public void LogTransaction(Transaction transaction)
        {
            string query = @"INSERT INTO Transactions(BuyerID, SellerID, EquipmentID, MovieID, EventID, Amount, Type, Status, Timestamp, ShippingAddress)
                            VALUES (@buyerID, @sellerID, @equipID, @movieID, @eventID, @amount, @type, @status, @timestamp, @address)";

            // 'using' automatically opens and closes the data conn

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@buyerID", transaction.BuyerID.ID);
                    cmd.Parameters.AddWithValue("@sellerID", (object)transaction.SellerID?.ID ?? DBNull.Value); // Seller might be null for store buys
                    cmd.Parameters.AddWithValue("@equipID", (object)transaction.EquipmentID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@movieID", (object)transaction.MovieID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@eventID", (object)transaction.EventID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@amount", transaction.Amount);
                    cmd.Parameters.AddWithValue("@type", transaction.Type);
                    cmd.Parameters.AddWithValue("@status", transaction.Status);
                    cmd.Parameters.AddWithValue("@timestamp", transaction.Timestamp);
                    cmd.Parameters.AddWithValue("@address", (object)transaction.ShippingAddress ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }

            }
        }

        public List<Transaction> GetTransactionsByUserId(int userID)
        {
            var transactions = new List<Transaction>();
            string query = "SELECT * FROM Transactions WHERE BuyerID = @userID OR SellerID = @userID ORDER BY Timestamp DESC";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userID", userID);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transactions.Add(new Transaction
                            {
                                ID = reader.GetInt32(reader.GetOrdinal("TransactionID")),
                                BuyerID = new User { ID = reader.GetInt32(reader.GetOrdinal("BuyerID")) },
                                SellerID = reader.IsDBNull(reader.GetOrdinal("SellerID")) ? null : new User { ID = reader.GetInt32(reader.GetOrdinal("SellerID")) },
                                MovieID = reader.IsDBNull(reader.GetOrdinal("MovieID")) ? null : reader.GetInt32(reader.GetOrdinal("MovieID")),
                                EquipmentID = reader.IsDBNull(reader.GetOrdinal("EquipmentID")) ? null : reader.GetInt32(reader.GetOrdinal("EquipmentID")),
                                EventID = reader.IsDBNull(reader.GetOrdinal("EventID")) ? null : reader.GetInt32(reader.GetOrdinal("EventID")),
                                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                Type = reader.GetString(reader.GetOrdinal("Type")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp")),
                                ShippingAddress = reader.IsDBNull(reader.GetOrdinal("ShippingAddress")) ? null : reader.GetString(reader.GetOrdinal("ShippingAddress"))
                            });
                        }
                    }
                }
            }
            return transactions;
        }
    }
}