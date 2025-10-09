using System;

namespace ReliableCDMS.Models
{
    public class Document
    {
        public int DocumentId { get; set; }
        public string FileName { get; set; }
        public string Category { get; set; }
        public int UploadedBy { get; set; }
        public DateTime UploadDate { get; set; }
        public int CurrentVersion { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public bool IsDeleted { get; set; }
    }
}