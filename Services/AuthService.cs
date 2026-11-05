namespace ECMS;

/// <summary>Login, student self-registration and instructor applications.</summary>
public static class AuthService
{
    public static (bool Ok, string Message) Login(UserRole role, string emailOrUsername, string password)
    {
        emailOrUsername = emailOrUsername.Trim();
        password = password.Trim();

        if (role == UserRole.Admin)
        {
            if (emailOrUsername.Equals(AppData.AdminEmail, StringComparison.OrdinalIgnoreCase)
                && password == AppData.AdminPassword)
            {
                Session.SignInAsAdmin();
                return (true, "Welcome back, " + AppData.AdminName);
            }
            return (false, "The administrator email or password is incorrect.");
        }

        if (role == UserRole.Student)
        {
            var student = AppData.Students.FirstOrDefault(s =>
                s.Email.Equals(emailOrUsername, StringComparison.OrdinalIgnoreCase) ||
                s.Username.Equals(emailOrUsername, StringComparison.OrdinalIgnoreCase));

            if (student is null || student.Password != password)
                return (false, "The student username / email or password is incorrect.");
            if (!student.IsActive)
                return (false, "This student account is inactive. Please contact the academy.");

            Session.SignInStudent(student);
            return (true, "Welcome back, " + student.DisplayName);
        }

        var instructor = AppData.Instructors.FirstOrDefault(i =>
            i.Email.Equals(emailOrUsername, StringComparison.OrdinalIgnoreCase));

        if (instructor is null || instructor.Password != password)
            return (false, "The instructor email or password is incorrect.");

        if (instructor.Status != AccountStatus.Active)
            return (false, instructor.Status switch
            {
                AccountStatus.Pending => "Your application is still pending admin review.",
                AccountStatus.Rejected => "Your application was not approved. Please contact the admin.",
                _ => "This instructor account is deactivated. Please contact the admin."
            });

        Session.SignInInstructor(instructor);
        return (true, "Welcome back, " + instructor.FullName);
    }

    public static (Student? Student, string Error) RegisterStudent(
        string username, string fullName, string email, string phone, string password)
    {
        username = username.Trim();
        fullName = fullName.Trim();
        email = email.Trim();
        phone = phone.Trim();
        password = password.Trim();

        if (username.Length < 3) return (null, "Username must be at least 3 characters.");
        if (fullName.Length < 3) return (null, "Please enter your full name.");
        if (!email.Contains('@') || !email.Contains('.')) return (null, "Please enter a valid email address.");
        if (phone.Length < 8) return (null, "Please enter a valid phone number.");
        if (password.Length < 6) return (null, "Password must be at least 6 characters.");

        if (AppData.Students.Any(s => s.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            return (null, "This username is already taken.");
        if (AppData.Students.Any(s => s.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            return (null, "This email is already registered.");
        if (AppData.Students.Any(s => s.Phone == phone))
            return (null, "This phone number is already registered.");

        var student = new Student
        {
            Id = AppData.NextId(AppData.Students, s => s.Id),
            Username = username, FullName = fullName, Email = email, Phone = phone,
            Password = password, JoinedOn = DateTime.Today
        };
        AppData.Students.Add(student);
        return (student, "");
    }

    public static (Instructor? Instructor, string Error) ApplyInstructor(
        string fullName, string email, string phone, string password, string specialization,
        string cvFileName, TeachingPreference preference)
    {
        fullName = fullName.Trim();
        email = email.Trim();
        phone = phone.Trim();
        specialization = specialization.Trim();
        cvFileName = cvFileName.Trim();

        if (fullName.Length < 3) return (null, "Please enter your full name.");
        if (!email.Contains('@') || !email.Contains('.')) return (null, "Please enter a valid email address.");
        if (phone.Length < 8) return (null, "Please enter a valid phone number.");
        if (password.Length < 6) return (null, "Password must be at least 6 characters.");
        if (specialization.Length < 2) return (null, "Please enter your specialization.");
        if (cvFileName.Length < 2) return (null, "Please attach your CV file name.");

        if (AppData.Instructors.Any(i => i.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            return (null, "An application already exists for this email.");

        var instructor = new Instructor
        {
            Id = AppData.NextId(AppData.Instructors, i => i.Id),
            FullName = fullName, Email = email, Phone = phone, Password = password,
            Specialization = specialization, CvFileName = cvFileName,
            Preference = preference, Status = AccountStatus.Pending, AppliedOn = DateTime.Today
        };
        AppData.Instructors.Add(instructor);
        return (instructor, "");
    }
}
