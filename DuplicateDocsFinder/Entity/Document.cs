namespace DuplicateDocsFinder.Entity
{
    public class Document
    {
        public Guid Id { get; set; }

        public int UserId { get; set; }

        public string? FileName { get; set; }

        public string? FilePath { get; set; }

        public string? FileHash { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
