namespace BackupCoordinatorV2.Models
{
    public class ReportDTO
    {
        public long lastDateEnteredTimestamp { get; set; }

        public long totalRestores { get; set; }

        public  List<ReportItemDTO> backupItemsFileReportItems { get; set; }        

        public List<ReportItemDTO> offSiteFileReportItems { get; set; }
        public List<ReportItemDTO> dirListFileReportItems { get; set; }
    }
}
