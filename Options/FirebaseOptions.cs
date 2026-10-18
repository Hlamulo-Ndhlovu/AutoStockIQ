namespace AutoStockIQ.Options;

public class FirebaseOptions
{
    public const string SectionName = "Firebase";

    public string ProjectId { get; set; } = "";

    public string ServiceAccountKeyPath { get; set; } = "";

    public string StorageBucket { get; set; } = "";
}
