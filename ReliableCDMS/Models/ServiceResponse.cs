using System.Runtime.Serialization;

namespace ReliableCDMS.Models
{

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
}