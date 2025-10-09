using ReliableCDMS.DAL;
using ReliableCDMS.Helpers;
using System;

namespace ReliableCDMS
{
    public class UserManagementService : IUserManagementService
    {
        private UserDAL userDAL = new UserDAL();

        /// <summary>
        /// Authenticate user for SOAP service
        /// </summary>
        private bool AuthenticateServiceUser(string username, string password, out int userId, out string userRole)
        {
            userId = 0;
            userRole = "";

            try
            {
                string passwordHash = SecurityHelper.HashPassword(password);
                var user = userDAL.AuthenticateUser(username, passwordHash);

                if (user != null)
                {
                    userId = user.UserId;
                    userRole = user.Role;
                    return true;
                }
            }
            catch
            {
                // Authentication failed
            }

            return false;
        }

        /// <summary>
        /// Create new user
        /// </summary>
        public ServiceResponse CreateUser(string username, string password, string role, string department, string authUsername, string authPassword)
        {
            try
            {
                // Authenticate
                int authUserId;
                string authUserRole;
                if (!AuthenticateServiceUser(authUsername, authPassword, out authUserId, out authUserRole))
                {
                    return new ServiceResponse { Success = false, Message = "Authentication failed" };
                }

                // Check if user is admin
                if (authUserRole != "Admin")
                {
                    return new ServiceResponse { Success = false, Message = "Unauthorized. Admin role required." };
                }

                // Create user
                string passwordHash = SecurityHelper.HashPassword(password);
                int userId = userDAL.CreateUser(username, passwordHash, role, department);

                if (userId > 0)
                {
                    // Log action
                    AuditHelper.LogAction(authUserId, "SOAP Create User",
                        $"Created user via SOAP: {username}, Role: {role}", "");

                    return new ServiceResponse
                    {
                        Success = true,
                        Message = "User created successfully",
                        RecordId = userId
                    };
                }
                else
                {
                    return new ServiceResponse { Success = false, Message = "Failed to create user. Username may already exist." };
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse { Success = false, Message = "Error: " + ex.Message };
            }
        }

        /// <summary>
        /// Update user
        /// </summary>
        public ServiceResponse UpdateUser(int userId, string department, string role, string authUsername, string authPassword)
        {
            try
            {
                // Authenticate
                int authUserId;
                string authUserRole;
                if (!AuthenticateServiceUser(authUsername, authPassword, out authUserId, out authUserRole))
                {
                    return new ServiceResponse { Success = false, Message = "Authentication failed" };
                }

                // Check if user is admin
                if (authUserRole != "Admin")
                {
                    return new ServiceResponse { Success = false, Message = "Unauthorized. Admin role required." };
                }

                // Update user
                bool success = userDAL.UpdateUser(userId, department, role);

                if (success)
                {
                    // Log action
                    AuditHelper.LogAction(authUserId, "SOAP Update User",
                        $"Updated user via SOAP: User ID: {userId}", "");

                    return new ServiceResponse { Success = true, Message = "User updated successfully" };
                }
                else
                {
                    return new ServiceResponse { Success = false, Message = "User not found or update failed" };
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse { Success = false, Message = "Error: " + ex.Message };
            }
        }

        /// <summary>
        /// Delete user (soft delete)
        /// </summary>
        public ServiceResponse DeleteUser(int userId, string authUsername, string authPassword)
        {
            try
            {
                // Authenticate
                int authUserId;
                string authUserRole;

                if (!AuthenticateServiceUser(authUsername, authPassword, out authUserId, out authUserRole))
                {
                    return new ServiceResponse { Success = false, Message = "Authentication failed" };
                }

                // Check if user is admin
                if (authUserRole != "Admin")
                {
                    return new ServiceResponse { Success = false, Message = "Unauthorized. Admin role required." };
                }

                // Prevent self-deletion
                if (userId == authUserId)
                {
                    return new ServiceResponse { Success = false, Message = "Cannot delete your own account" };
                }

                // Delete user
                bool success = userDAL.DeleteUser(userId);

                if (success)
                {
                    // Log action
                    AuditHelper.LogAction(authUserId, "SOAP Delete User",
                        $"Deleted user via SOAP: User ID: {userId}", "");

                    return new ServiceResponse { Success = true, Message = "User deleted successfully" };
                }
                else
                {
                    return new ServiceResponse { Success = false, Message = "User not found or delete failed" };
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse { Success = false, Message = "Error: " + ex.Message };
            }
        }

        /// <summary>
        /// Activate user
        /// </summary>
        public ServiceResponse ActivateUser(int userId, string authUsername, string authPassword)
        {
            try
            {
                // Authenticate service user
                int authUserId;
                string authUserRole;

                if (!AuthenticateServiceUser(authUsername, authPassword, out authUserId, out authUserRole))
                {
                    return new ServiceResponse { Success = false, Message = "Authentication failed" };
                }

                // Only Admin can activate users
                if (authUserRole != "Admin")
                {
                    return new ServiceResponse { Success = false, Message = "Unauthorized. Admin role required." };
                }

                // Activate user
                bool success = userDAL.ActivateUser(userId);

                if (success)
                {
                    // Log the action
                    AuditHelper.LogAction(authUserId, "SOAP Activate User",
                        $"Activated user via SOAP: User ID: {userId}", "");

                    return new ServiceResponse { Success = true, Message = "User activated successfully" };
                }
                else
                {
                    return new ServiceResponse { Success = false, Message = "User not found or activation failed" };
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse { Success = false, Message = "Error: " + ex.Message };
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public UserInfo GetUser(int userId, string authUsername, string authPassword)
        {
            try
            {
                // Authenticate
                int authUserId;
                string authUserRole;
                if (!AuthenticateServiceUser(authUsername, authPassword, out authUserId, out authUserRole))
                {
                    return null;
                }

                // Get user
                var user = userDAL.GetUserById(userId);

                if (user != null)
                {
                    return new UserInfo
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        Role = user.Role,
                        Department = user.Department,
                        IsActive = user.IsActive
                    };
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get all users
        /// </summary>
        public UserInfo[] GetAllUsers(string authUsername, string authPassword)
        {
            try
            {
                // Authenticate
                int authUserId;
                string authUserRole;
                if (!AuthenticateServiceUser(authUsername, authPassword, out authUserId, out authUserRole))
                {
                    return new UserInfo[0];
                }

                // Check if user is admin
                if (authUserRole != "Admin")
                {
                    return new UserInfo[0];
                }

                // Get all users
                var usersTable = userDAL.GetAllUsers();
                var usersList = new System.Collections.Generic.List<UserInfo>();

                foreach (System.Data.DataRow row in usersTable.Rows)
                {
                    usersList.Add(new UserInfo
                    {
                        UserId = Convert.ToInt32(row["UserId"]),
                        Username = row["Username"].ToString(),
                        Role = row["Role"].ToString(),
                        Department = row["Department"].ToString(),
                        IsActive = Convert.ToBoolean(row["IsActive"])
                    });
                }

                return usersList.ToArray();
            }
            catch
            {
                return new UserInfo[0];
            }
        }
    }
}
