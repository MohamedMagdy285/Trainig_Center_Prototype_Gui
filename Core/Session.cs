namespace ECMS;

/// <summary>The signed-in user for the current desktop session.</summary>
public static class Session
{
    public static UserRole Role { get; private set; } = UserRole.Admin;
    public static Student? Student { get; private set; }
    public static Instructor? Instructor { get; private set; }

    public static string Name => Role switch
    {
        UserRole.Student => Student?.DisplayName ?? "Student",
        UserRole.Instructor => Instructor?.FullName ?? "Instructor",
        _ => AppData.AdminName
    };

    public static string RoleText => Role switch
    {
        UserRole.Student => "Student",
        UserRole.Instructor => "Instructor",
        _ => "Administrator"
    };

    public static void SignInAsAdmin()
    {
        Role = UserRole.Admin;
        Student = null;
        Instructor = null;
    }

    public static void SignInStudent(Student student)
    {
        Role = UserRole.Student;
        Student = student;
        Instructor = null;
    }

    public static void SignInInstructor(Instructor instructor)
    {
        Role = UserRole.Instructor;
        Instructor = instructor;
        Student = null;
    }

    public static void SignOut()
    {
        Role = UserRole.Admin;
        Student = null;
        Instructor = null;
    }
}
