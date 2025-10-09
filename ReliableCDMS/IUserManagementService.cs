using System.ServiceModel;

namespace ReliableCDMS
{
    [ServiceContract]
    public interface IUserManagementService
    {
        [OperationContract]
        ServiceResponse CreateUser(string username, string password, string role, string department, string authUsername, string authPassword);

        [OperationContract]
        ServiceResponse UpdateUser(int userId, string department, string role, string authUsername, string authPassword);

        [OperationContract]
        ServiceResponse DeleteUser(int userId, string authUsername, string authPassword);

        [OperationContract]
        ServiceResponse ActivateUser(int userId, string authUsername, string authPassword);

        [OperationContract]
        UserInfo GetUser(int userId, string authUsername, string authPassword);

        [OperationContract]
        UserInfo[] GetAllUsers(string authUsername, string authPassword);
    }

    /// <summary>
    /// Service response object
    /// </summary>
    [System.Runtime.Serialization.DataContract]
    public class ServiceResponse
    {
        [System.Runtime.Serialization.DataMember]
        public bool Success { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string Message { get; set; }

        [System.Runtime.Serialization.DataMember]
        public int RecordId { get; set; }
    }

    /// <summary>
    /// User information object
    /// </summary>
    [System.Runtime.Serialization.DataContract]
    public class UserInfo
    {
        [System.Runtime.Serialization.DataMember]
        public int UserId { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string Username { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string Role { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string Department { get; set; }

        [System.Runtime.Serialization.DataMember]
        public bool IsActive { get; set; }
    }
}
