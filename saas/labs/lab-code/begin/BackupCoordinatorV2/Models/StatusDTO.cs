using Common.DTO.V2;

namespace BackupCoordinatorV2.Models
{
    public class StatusDTO
    {
        public long activeThreads { get; set; }
        public List<FileDTO> AgentFileDTOs = new List<FileDTO>();
        internal List<FileDTO> StagingFileDTOs = new List<FileDTO>();
    }
}
