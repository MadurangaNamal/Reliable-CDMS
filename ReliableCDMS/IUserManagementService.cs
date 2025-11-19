using ReliableCDMS.Models;
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
}
