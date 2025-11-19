using System.Runtime.Serialization;

namespace ReliableCDMS.Models
{
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