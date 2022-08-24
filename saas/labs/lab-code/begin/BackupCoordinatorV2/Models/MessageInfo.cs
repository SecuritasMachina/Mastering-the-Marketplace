namespace BackupCoordinatorV2.Models
{
    public class MessageInfo
    {
        public DateTime DateCreated = DateTime.UtcNow;
        public Guid CorrelationId = Guid.NewGuid();
    }
}
