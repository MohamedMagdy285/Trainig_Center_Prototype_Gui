namespace ECMS;

/// <summary>
/// The in-memory database for the desktop demo.
/// Every page reads and writes these lists through the services.
/// </summary>
public static class AppData
{
    public static List<Student> Students { get; } = new();
    public static List<Instructor> Instructors { get; } = new();
    public static List<Course> Courses { get; } = new();
    public static List<Batch> Batches { get; } = new();
    public static List<Enrollment> Enrollments { get; } = new();
    public static List<Payment> Payments { get; } = new();
    public static List<Refund> Refunds { get; } = new();
    public static List<Credit> Credits { get; } = new();
    public static List<ExamAttempt> Exams { get; } = new();
    public static List<Certificate> Certificates { get; } = new();
    public static List<Review> Reviews { get; } = new();
    public static List<DeleteRequest> DeleteRequests { get; } = new();
    public static List<LessonCompletion> Completions { get; } = new();

    public static string AdminName { get; set; } = "Menna Karim";
    public static string AdminEmail { get; set; } = "admin@edusphere.com";
    public static string AdminPassword { get; set; } = "admin123";

    public static int NextId<T>(IEnumerable<T> items, Func<T, int> id)
        => items.Any() ? items.Max(id) + 1 : 1;

    static AppData() => Seed();

    private static void Seed()
    {
        // Students
        Students.Add(NewStudent("nour", "Nour Adel", "nour@mail.com", "+20 100 234 5678", new DateTime(2026, 9, 12)));
        Students.Add(NewStudent("youssef", "Youssef Hassan", "youssef@mail.com", "+20 101 884 1209", new DateTime(2026, 9, 10)));
        Students.Add(NewStudent("salma", "Salma Tarek", "salma@mail.com", "+20 102 551 7783", new DateTime(2026, 9, 8)));
        Students.Add(NewStudent("omar.s", "Omar Khaled", "omar.s@mail.com", "+20 106 770 2140", new DateTime(2026, 9, 5)));
        Students.Add(NewStudent("farida", "Farida Mahmoud", "farida@mail.com", "+20 109 342 8761", new DateTime(2026, 9, 2)));
        Students.Add(NewStudent("ziad", "Ziad Mostafa", "ziad@mail.com", "+20 111 450 9928", new DateTime(2026, 8, 30)));
        Students.Add(NewStudent("habiba", "Habiba Ali", "habiba@mail.com", "+20 112 806 3354", new DateTime(2026, 8, 28)));
        Students.Add(NewStudent("karim", "Karim Nabil", "karim@mail.com", "+20 115 239 4408", new DateTime(2026, 8, 25)));
        Students.Add(NewStudent("mariam", "Mariam Fouad", "mariam@mail.com", "+20 128 441 9902", new DateTime(2026, 7, 18)));

        // Instructors
        Instructors.Add(NewInstructor("Dr. Omar Khaled", "omar@edusphere.com", "+20 100 111 2201",
            "Full-Stack Development", "omar-khaled-cv.pdf", TeachingPreference.Both, AccountStatus.Active));
        Instructors.Add(NewInstructor("Eng. Nada Samir", "nada@edusphere.com", "+20 100 111 2202",
            "UI/UX & Graphic Design", "nada-samir-cv.pdf", TeachingPreference.Online, AccountStatus.Active));
        Instructors.Add(NewInstructor("Dr. Ahmed Fathy", "ahmed@edusphere.com", "+20 100 111 2203",
            "Data Science", "ahmed-fathy-cv.pdf", TeachingPreference.Both, AccountStatus.Active));
        Instructors.Add(NewInstructor("Eng. Mona Adel", "mona@edusphere.com", "+20 100 111 2204",
            "Business English", "mona-adel-cv.pdf", TeachingPreference.Offline, AccountStatus.Pending));
        Instructors.Add(NewInstructor("Eng. Tamer Hosny", "tamer@edusphere.com", "+20 100 111 2205",
            "Digital Marketing", "tamer-hosny-cv.pdf", TeachingPreference.Both, AccountStatus.Active));
        Instructors.Add(NewInstructor("Eng. Hala Youssef", "hala@edusphere.com", "+20 100 111 2206",
            "Product Management", "hala-youssef-cv.pdf", TeachingPreference.Online, AccountStatus.Deactivated));

        // Courses and their learning content
        var fullStack = NewCourse("Full-Stack Web Development",
            "Build complete web applications from the database to the browser.",
            CourseKind.Online, 1, 3500, true, true);
        AddContent(fullStack,
            ("Getting started", true, new[] { "Course roadmap", "Setting up the tools", "Your first web page" }),
            ("Core concepts", false, new[] { "HTML and semantic structure", "CSS layout and colour", "JavaScript essentials" }),
            ("Building the project", false, new[] { "Connecting the API", "Deploying your app" }));
        AddBonus(fullStack, "Bonus: Interview preparation",
            new[] { "Portfolio review", "Common interview questions" });

        var uiux = NewCourse("UI/UX Design Fundamentals",
            "Learn research, wireframes, prototypes and visual design.",
            CourseKind.Online, 2, 2800, true, true);
        AddContent(uiux,
            ("Design thinking", true, new[] { "What is UX?", "User research basics", "Problem statements" }),
            ("Wireframes and flows", false, new[] { "Sketching screens", "User flows", "Usability testing" }),
            ("Visual design", false, new[] { "Colour and type", "Design systems" }));

        var english = NewCourse("Business English Bootcamp",
            "Practical English for meetings, emails and presentations.",
            CourseKind.Offline, 4, 1800, true, true);
        AddContent(english,
            ("Meetings", true, new[] { "Opening a meeting", "Giving opinions" }),
            ("Writing", false, new[] { "Professional emails", "Reports" }),
            ("Presentations", false, new[] { "Structure and delivery" }));

        var dataScience = NewCourse("Data Science with Python",
            "From Python basics to real data projects.",
            CourseKind.Online, 3, 4200, true, true);
        AddContent(dataScience,
            ("Python foundations", true, new[] { "Variables and types", "Control flow", "Functions" }),
            ("Data analysis", false, new[] { "Pandas basics", "Cleaning data", "Exploratory analysis" }),
            ("Machine learning", false, new[] { "First model", "Evaluating a model", "Next steps" }));

        var marketing = NewCourse("Digital Marketing Essentials",
            "Plan campaigns, create content and measure results.",
            CourseKind.Offline, 5, 2200, true, true);
        AddContent(marketing,
            ("Marketing foundations", true, new[] { "Audience and offer", "The marketing funnel" }),
            ("Channels", false, new[] { "Social media", "Search and content" }),
            ("Measuring results", false, new[] { "Campaign metrics" }));

        var graphic = NewCourse("Graphic Design Masterclass",
            "Colour, typography and layout for digital and print.",
            CourseKind.Online, 2, 3000, true, true);
        AddContent(graphic,
            ("Design basics", true, new[] { "The design process", "Colour theory", "Typography" }),
            ("Layout", false, new[] { "Grids and spacing", "Composition", "Print vs screen" }),
            ("Portfolio project", false, new[] { "Brief and moodboard", "Final artwork" }));

        var product = NewCourse("Product Management Essentials",
            "Discover, prioritise and deliver useful products.",
            CourseKind.Online, 6, 2600, true, false);
        AddContent(product,
            ("Product thinking", true, new[] { "What is a product?", "Customer problems" }),
            ("Delivery", false, new[] { "Roadmaps", "Working with teams", "Measuring outcomes" }));

        var advancedEnglish = NewCourse("Business English Advanced",
            "Advanced communication for leaders and managers.",
            CourseKind.Offline, 4, 2000, true, true);
        AddContent(advancedEnglish,
            ("Leadership communication", true, new[] { "Difficult conversations", "Giving feedback" }),
            ("Negotiation", false, new[] { "Negotiation language", "Closing a deal" }),
            ("Executive presence", false, new[] { "Storytelling" }));

        // Batches
        NewBatch(english, 4, "Business English - November", new DateTime(2026, 11, 15), new DateTime(2026, 12, 20), 8, 1800, "Sat & Mon, 6:00 - 8:00 PM");
        NewBatch(english, 4, "Business English - January", new DateTime(2027, 1, 10), new DateTime(2027, 2, 14), 20, 1800, "Sun & Tue, 6:00 - 8:00 PM");
        NewBatch(marketing, 5, "Digital Marketing - November", new DateTime(2026, 11, 20), new DateTime(2026, 12, 30), 30, 2200, "Fri & Sat, 5:00 - 7:00 PM");
        NewBatch(advancedEnglish, 4, "Advanced English - December", new DateTime(2026, 12, 5), new DateTime(2027, 1, 16), 20, 2000, "Mon & Wed, 7:00 - 9:00 PM");
        NewBatch(english, 4, "Business English - Summer", new DateTime(2026, 9, 1), new DateTime(2026, 10, 15), 20, 1800, "Sat & Mon, 6:00 - 8:00 PM");
        NewBatch(marketing, 5, "Digital Marketing - Fast track", new DateTime(2026, 11, 10), new DateTime(2026, 12, 10), 2, 2200, "Daily, 4:00 - 6:00 PM");

        // Payments (kept forever, even after refunds)
        var p1 = NewPayment(1, 1, null, 3500, PaymentMethod.CreditCard, PaymentStatus.Successful, new DateTime(2026, 9, 12), "GW-1001");
        var p2 = NewPayment(2, 2, null, 2800, PaymentMethod.Fawry, PaymentStatus.Successful, new DateTime(2026, 9, 10), "GW-1002");
        var p3 = NewPayment(3, 3, 1, 1800, PaymentMethod.VodafoneCash, PaymentStatus.Pending, new DateTime(2026, 11, 2), "GW-1003");
        var p4 = NewPayment(9, 4, null, 4200, PaymentMethod.DebitCard, PaymentStatus.Refunded, new DateTime(2026, 7, 20), "GW-1004");
        var p5 = NewPayment(4, 5, 3, 2200, PaymentMethod.CreditCard, PaymentStatus.Successful, new DateTime(2026, 9, 5), "GW-1005");
        var p6 = NewPayment(7, 6, null, 3000, PaymentMethod.Fawry, PaymentStatus.Successful, new DateTime(2026, 8, 28), "GW-1006");
        var p7 = NewPayment(6, 2, null, 2800, PaymentMethod.CreditCard, PaymentStatus.Failed, new DateTime(2026, 8, 30), "GW-1007");
        var p8 = NewPayment(1, 3, 5, 1800, PaymentMethod.CreditCard, PaymentStatus.Successful, new DateTime(2026, 8, 20), "GW-1008");
        var p9 = NewPayment(5, 8, 4, 2000, PaymentMethod.DebitCard, PaymentStatus.Successful, new DateTime(2026, 10, 1), "GW-1009");
        var p10 = NewPayment(7, 6, null, 3000, PaymentMethod.CreditCard, PaymentStatus.Successful, new DateTime(2026, 8, 29), "GW-1010");
        p10.IsDuplicate = true;
        var p11 = NewPayment(6, 5, 6, 2200, PaymentMethod.CreditCard, PaymentStatus.Successful, new DateTime(2026, 9, 20), "GW-1011");
        var p12 = NewPayment(8, 5, 6, 2200, PaymentMethod.Fawry, PaymentStatus.Successful, new DateTime(2026, 9, 21), "GW-1012");

        // Enrollments are only created after a successful payment
        NewEnrollment(1, 1, null, p1, new DateTime(2026, 9, 12));
        NewEnrollment(2, 2, null, p2, new DateTime(2026, 9, 10));
        NewEnrollment(9, 4, null, p4, new DateTime(2026, 7, 20), EnrollmentStatus.Refunded);
        NewEnrollment(4, 5, 3, p5, new DateTime(2026, 9, 5));
        NewEnrollment(7, 6, null, p6, new DateTime(2026, 8, 28));
        NewEnrollment(1, 3, 5, p8, new DateTime(2026, 8, 20));
        NewEnrollment(5, 8, 4, p9, new DateTime(2026, 10, 1));
        NewEnrollment(6, 5, 6, p11, new DateTime(2026, 9, 20));
        NewEnrollment(8, 5, 6, p12, new DateTime(2026, 9, 21));

        // Refunds and credit
        NewRefund(p4, 9, 4200, new DateTime(2026, 7, 25), RefundKind.Refund, "Academy cancelled the cohort");
        NewRefund(p10, 7, 3000, new DateTime(2026, 8, 30), RefundKind.Credit, "Duplicate successful payment");
        Credits.Add(new Credit
        {
            Id = 1, StudentId = 7, SourcePaymentId = p10.Id, Amount = 3000, Remaining = 3000,
            CreatedOn = new DateTime(2026, 8, 30)
        });

        // Lesson progress
        Complete(1, 1, 4);
        Complete(2, 2, 7);
        Complete(7, 6, 3);

        // Exams and certificates
        Exams.Add(new ExamAttempt
        {
            Id = 1, StudentId = 1, CourseId = 1, Type = ExamType.OnSite,
            ScheduledOn = new DateTime(2026, 11, 20), Fee = 300, Result = ExamResult.Pending
        });
        Exams.Add(new ExamAttempt
        {
            Id = 2, StudentId = 2, CourseId = 2, Type = ExamType.OnSite,
            ScheduledOn = new DateTime(2026, 10, 10), Fee = 300, Score = 95,
            Result = ExamResult.Passed, TakenOn = new DateTime(2026, 10, 10), CertificateId = 1
        });
        Exams.Add(new ExamAttempt
        {
            Id = 3, StudentId = 7, CourseId = 6, Type = ExamType.Remote,
            ScheduledOn = new DateTime(2026, 11, 1), Fee = 300, Score = 72,
            Result = ExamResult.Failed, TakenOn = new DateTime(2026, 11, 1)
        });
        Exams.Add(new ExamAttempt
        {
            Id = 4, StudentId = 9, CourseId = 4, Type = ExamType.Remote,
            ScheduledOn = new DateTime(2026, 10, 18), Fee = 300, Score = 92,
            Result = ExamResult.Passed, TakenOn = new DateTime(2026, 10, 18), CertificateId = 2
        });
        Exams.Add(new ExamAttempt
        {
            Id = 5, StudentId = 4, CourseId = 5, Type = ExamType.OnSite,
            ScheduledOn = new DateTime(2026, 10, 20), Fee = 300, Score = 88,
            Result = ExamResult.Failed, TakenOn = new DateTime(2026, 10, 20)
        });

        Certificates.Add(new Certificate
        {
            Id = 1, StudentId = 2, CourseId = 2, ExamAttemptId = 2, Type = ExamType.OnSite,
            Code = "EDS-2026-0001", IssuedOn = new DateTime(2026, 10, 12)
        });
        Certificates.Add(new Certificate
        {
            Id = 2, StudentId = 9, CourseId = 4, ExamAttemptId = 4, Type = ExamType.Remote,
            Code = "EDS-2026-0002", IssuedOn = new DateTime(2026, 10, 20)
        });

        // Reviews
        Reviews.Add(NewReview(1, 1, 1, null, "A very clear introduction. The setup video saved me a lot of time."));
        Reviews.Add(NewReview(2, 2, 2, null, "The wireframe exercises made the design process easy to follow."));
        Reviews.Add(NewReview(7, 6, 1, null, "Loved the colour theory section. I used it in my own project."));
        Reviews.Add(NewReview(1, 3, null, 5, "The instructor gave useful feedback after every lecture."));
        Reviews.Add(NewReview(4, 5, null, 3, "The campaign examples were practical and up to date."));

        // Delete requests
        DeleteRequests.Add(new DeleteRequest
        {
            Id = 1, InstructorId = 1, CourseId = 1, TargetName = "Lesson: Old setup video",
            Reason = "The recording is outdated after the 2026 tool update.", Status = DeleteRequestStatus.Pending,
            CreatedOn = new DateTime(2026, 10, 28)
        });
        DeleteRequests.Add(new DeleteRequest
        {
            Id = 2, InstructorId = 2, CourseId = 2, TargetName = "Section: Legacy tools",
            Reason = "The section is no longer part of the course.", Status = DeleteRequestStatus.Pending,
            CreatedOn = new DateTime(2026, 10, 30)
        });
        DeleteRequests.Add(new DeleteRequest
        {
            Id = 3, InstructorId = 1, CourseId = 1, TargetName = "Lesson: Duplicate welcome clip",
            Reason = "A duplicate welcome clip was uploaded by mistake.", Status = DeleteRequestStatus.Approved,
            CreatedOn = new DateTime(2026, 10, 1), ReviewedOn = new DateTime(2026, 10, 2)
        });
    }

    // ---------- small factory helpers ----------

    private static Student NewStudent(string username, string name, string email, string phone, DateTime joined)
        => new()
        {
            Id = Students.Count + 1, Username = username, FullName = name, Email = email,
            Phone = phone, Password = "123456", JoinedOn = joined
        };

    private static Instructor NewInstructor(string name, string email, string phone, string specialization,
        string cv, TeachingPreference preference, AccountStatus status)
        => new()
        {
            Id = Instructors.Count + 1, FullName = name, Email = email, Phone = phone,
            Password = "123456", Specialization = specialization, CvFileName = cv,
            Preference = preference, Status = status, AppliedOn = new DateTime(2026, 8, 1)
        };

    private static Course NewCourse(string title, string description, CourseKind kind, int instructorId,
        decimal price, bool published, bool acceptsNew)
        => new Course
        {
            Id = Courses.Count + 1, Title = title, Description = description, Kind = kind,
            InstructorId = instructorId, Price = price, IsPublished = published,
            AcceptsNewEnrollments = acceptsNew, CreatedOn = new DateTime(2026, 8, 1)
        }.Also(c => Courses.Add(c));

    private static void AddContent(Course course, params (string Title, bool Preview, string[] Lessons)[] sections)
    {
        var order = 1;
        foreach (var item in sections)
        {
            var section = new Section
            {
                Id = AppData.NextId(AppData.Courses.SelectMany(c => c.Sections), s => s.Id),
                CourseId = course.Id, Title = item.Title, Order = order++, IsPreview = item.Preview
            };
            var lessonOrder = 1;
            var nextLessonId = AppData.NextId(
                AppData.Courses.SelectMany(c => c.Sections).SelectMany(s => s.Lessons), l => l.Id);
            foreach (var lesson in item.Lessons)
            {
                section.Lessons.Add(new Lesson
                {
                    Id = nextLessonId++,
                    SectionId = section.Id, Title = lesson, Order = lessonOrder++, Minutes = 12
                });
            }
            course.Sections.Add(section);
        }
    }

    private static void AddBonus(Course course, string title, string[] lessons)
    {
        var section = new Section
        {
            Id = AppData.NextId(AppData.Courses.SelectMany(c => c.Sections), s => s.Id),
            CourseId = course.Id, Title = title, Order = course.Sections.Count + 1, IsBonus = true
        };
        var order = 1;
        var nextLessonId = AppData.NextId(
            AppData.Courses.SelectMany(c => c.Sections).SelectMany(s => s.Lessons), l => l.Id);
        foreach (var lesson in lessons)
        {
            section.Lessons.Add(new Lesson
            {
                Id = nextLessonId++,
                SectionId = section.Id, Title = lesson, Order = order++, Minutes = 14
            });
        }
        course.Sections.Add(section);
    }

    private static void NewBatch(Course course, int instructorId, string title, DateTime start, DateTime end,
        int capacity, decimal price, string schedule)
    {
        Batches.Add(new Batch
        {
            Id = Batches.Count + 1, CourseId = course.Id, InstructorId = instructorId, Title = title,
            StartDate = start, EndDate = end, Capacity = capacity, Price = price, Schedule = schedule,
            Status = end < DateTime.Today ? BatchStatus.Completed : BatchStatus.Scheduled
        });
    }

    private static Payment NewPayment(int studentId, int courseId, int? batchId, decimal amount,
        PaymentMethod method, PaymentStatus status, DateTime created, string reference)
    {
        var payment = new Payment
        {
            Id = Payments.Count + 1, StudentId = studentId, CourseId = courseId, BatchId = batchId,
            Amount = amount, Method = method, Status = status, CreatedOn = created,
            ConfirmedOn = status == PaymentStatus.Successful || status == PaymentStatus.Refunded
                ? created : null,
            Reference = reference
        };
        Payments.Add(payment);
        return payment;
    }

    private static void NewEnrollment(int studentId, int courseId, int? batchId, Payment payment,
        DateTime enrolledOn, EnrollmentStatus status = EnrollmentStatus.Confirmed)
    {
        Enrollments.Add(new Enrollment
        {
            Id = Enrollments.Count + 1, StudentId = studentId, CourseId = courseId, BatchId = batchId,
            PaymentId = payment.Id, EnrolledOn = enrolledOn, Status = status
        });
    }

    private static void NewRefund(Payment payment, int studentId, decimal amount, DateTime executed,
        RefundKind kind, string reason)
    {
        Refunds.Add(new Refund
        {
            Id = Refunds.Count + 1, PaymentId = payment.Id, StudentId = studentId, Amount = amount,
            ExecutedOn = executed, Kind = kind, Reason = reason
        });
    }

    private static void Complete(int studentId, int courseId, int lessonCount)
    {
        var lessons = Courses.First(c => c.Id == courseId).Sections
            .OrderBy(s => s.Order).SelectMany(s => s.Lessons.OrderBy(l => l.Order)).Take(lessonCount);
        foreach (var lesson in lessons)
        {
            Completions.Add(new LessonCompletion
            {
                Id = Completions.Count + 1, StudentId = studentId, LessonId = lesson.Id,
                CompletedOn = DateTime.Today.AddDays(-3)
            });
        }
    }

    private static Review NewReview(int studentId, int courseId, int? sectionId, int? batchId, string text)
        => new()
        {
            Id = Reviews.Count + 1, StudentId = studentId, CourseId = courseId, SectionId = sectionId,
            BatchId = batchId, Text = text, CreatedOn = DateTime.Today.AddDays(-5)
        };
}

internal static class ObjectExtensions
{
    public static T Also<T>(this T value, Action<T> action)
    {
        action(value);
        return value;
    }
}
