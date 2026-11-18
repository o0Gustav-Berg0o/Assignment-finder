namespace AssignmentFinder.Worker.Notifications;

public class EmailOptions
{
    public const string SectionName = "Email";

    public required string Host { get; set; }
    public int Port { get; set; } = 587;
    public required string FromAddress { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool UseSsl { get; set; } = true;
}
