namespace ECMS;

/// <summary>A learner who registered on the platform.</summary>
public class Student
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Password { get; set; } = "";
    public DateTime JoinedOn { get; set; } = DateTime.Today;
    public bool IsActive { get; set; } = true;

    public string DisplayName => string.IsNullOrWhiteSpace(FullName) ? Username : FullName;
}
