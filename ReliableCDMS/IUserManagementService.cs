using System.Runtime.Serialization;
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
    [DataContract]
    public class ServiceResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public int RecordId { get; set; }
    }

    /// <summary>
    /// User information object
    /// </summary>
    [DataContract]
    public class UserInfo
    {
        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Role { get; set; }

        [DataMember]
        public string Department { get; set; }

        [DataMember]
        public bool IsActive { get; set; }
    }
}
