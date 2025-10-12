using ReliableCDMS.Models;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ReliableCDMS.DAL
{
    /// <summary>
    /// Data Access Layer for User operations
    /// </summary>
    public class UserDAL
    {
        private readonly string connString = ConfigurationManager.ConnectionStrings["ReliableCDMSDB"].ConnectionString;

        /// <summary>
        /// Authenticate user
        /// </summary>
        public User AuthenticateUser(string username, string passwordHash)
        {
            User user = null;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"SELECT UserId, Username, PasswordHash, Role, Department, IsActive, CreatedDate 
                               FROM Users 
                               WHERE Username = @Username AND PasswordHash = @PasswordHash AND IsActive = 1";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                UserId = (int)reader["UserId"],
                                Username = reader["Username"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                Role = reader["Role"].ToString(),
                                Department = reader["Department"].ToString(),
                                IsActive = (bool)reader["IsActive"],
                                CreatedDate = (DateTime)reader["CreatedDate"]
                            };
                        }
                    }
                }
            }

            return user;
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public User GetUserById(int userId)
        {
            User user = null;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"SELECT UserId, Username, PasswordHash, Role, Department, IsActive, CreatedDate 
                               FROM Users 
                               WHERE UserId = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                UserId = (int)reader["UserId"],
                                Username = reader["Username"].ToString(),
                                Role = reader["Role"].ToString(),
                                Department = reader["Department"].ToString(),
                                IsActive = (bool)reader["IsActive"],
                                CreatedDate = (DateTime)reader["CreatedDate"]
                            };
                        }
                    }
                }
            }

            return user;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        public DataTable GetAllUsers()
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"SELECT UserId, Username, Role, Department, IsActive, CreatedDate 
                               FROM Users 
                               ORDER BY CreatedDate DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// Create new user
        /// </summary>
        public int CreateUser(string username, string passwordHash, string role, string department)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"INSERT INTO Users (Username, PasswordHash, Role, Department, IsActive, CreatedDate) 
                               VALUES (@Username, @PasswordHash, @Role, @Department, 1, GETDATE());
                               SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    cmd.Parameters.AddWithValue("@Role", role);
                    cmd.Parameters.AddWithValue("@Department", department ?? "");

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Update user
        /// </summary>
        public bool UpdateUser(int userId, string department, string role)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"UPDATE Users 
                               SET Department = @Department, Role = @Role 
                               WHERE UserId = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Department", department ?? "");
                    cmd.Parameters.AddWithValue("@Role", role);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Delete user (soft delete by setting IsActive = false)
        /// </summary>
        public bool DeleteUser(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"UPDATE Users SET IsActive = 0 WHERE UserId = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Activate user
        /// </summary>
        public bool ActivateUser(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"UPDATE Users SET IsActive = 1 WHERE UserId = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}