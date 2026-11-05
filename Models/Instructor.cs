namespace ECMS;

/// <summary>An instructor application or active instructor profile.</summary>
public class Instructor
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Password { get; set; } = "";
    public string Specialization { get; set; } = "";
    public string CvFileName { get; set; } = "";
    public TeachingPreference Preference { get; set; } = TeachingPreference.Both;
    public AccountStatus Status { get; set; } = AccountStatus.Pending;
    public DateTime AppliedOn { get; set; } = DateTime.Today;
    public string AdminNote { get; set; } = "";

    public bool CanAccessPlatform => Status == AccountStatus.Active;
    public string StatusText => Status.ToString();
}
