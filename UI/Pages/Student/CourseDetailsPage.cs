namespace ECMS;

/// <summary>Full course page with curriculum, batches, instructor and reviews.</summary>
public partial class CourseDetailsPage : BasePage
{
    private readonly int _courseId;
    private readonly List<Section> _sections;
    private readonly Course _course;
    private readonly bool _enrolled;

    /// <summary>Parameterless constructor for the Visual Studio designer.</summary>
    public CourseDetailsPage() : base(_ => { })
    {
        InitializeComponent();
        _course = new Course();
        _sections = new List<Section>();
    }

    public CourseDetailsPage(int courseId) : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode) { _course = new Course(); _sections = new List<Section>(); return; }
        _courseId = courseId;
        var course = CatalogService.Course(courseId);
        if (course is null)
        {
            AddBlock(Header("Course not found", "This course is no longer available."), 60);
            _course = new Course();
            _sections = new List<Section>();
            return;
        }

        _course = course;
        _sections = CatalogService.Sections(courseId).ToList();
        var student = Session.Student!;
        _enrolled = CatalogService.IsEnrolled(student.Id, courseId);
        var instructor = CatalogService.Instructor(course.InstructorId);

        var action = _enrolled ? "Continue learning"
            : course.Kind == CourseKind.Online && course.AcceptsNewEnrollments ? "Enroll now"
            : null;
        AddBlock(Header(course.Title,
            (course.Kind == CourseKind.Online ? "Online course" : "Offline course") +
            "  •  with " + CatalogService.InstructorName(course.InstructorId),
            action, "\uE76C", action is null ? null : OpenCourse), 60);

        AddBlock(BuildHeroCard(course, instructor), 170);
        AddBlock(Split(BuildCurriculumCard(), BuildIncludesCard(course), 340, 62f), 340);

        if (instructor is not null)
        {
            var instructorCard = new InstructorCard(instructor);
            instructorCard.OpenRequested += (_, _) => DetailsDialog.Show(this, instructor.FullName,
                "Course instructor", new[]
                {
                    ("Specialization", instructor.Specialization),
                    ("Preference", instructor.Preference.ToString()),
                    ("Courses", CatalogService.CoursesForInstructor(instructor.Id).Count().ToString())
                });
            AddBlock(instructorCard, 190);
        }

        if (course.Kind == CourseKind.Offline)
            BuildBatchTable(course);

        BuildReviewTable(course);
    }

    private void OpenCourse()
    {
        if (_enrolled)
        {
            Navigator.Go("player", _courseId);
            return;
        }
        Navigator.Go("checkout", new CheckoutRequest(_courseId, null));
    }

    private Card BuildHeroCard(Course course, Instructor? instructor)
    {
        var card = new Card { BackColor = Color.White };
        var title = new Label
        {
            Text = course.Title, Font = Theme.H2, ForeColor = Theme.Ink, BackColor = Color.White,
            AutoSize = false, Location = new Point(24, 20), Size = new Size(600, 28)
        };
        var description = new Label
        {
            Text = course.Description, Font = Theme.Body, ForeColor = Theme.Muted, BackColor = Color.White,
            AutoSize = false, Location = new Point(24, 52), Size = new Size(600, 44)
        };
        var meta = new Label
        {
            Text = $"{course.Kind}  •  {course.TotalSections} sections  •  {course.TotalLessons} lessons  •  " +
                   $"{CatalogService.StudentCount(course.Id)} students",
            Font = Theme.Small, ForeColor = Theme.SoftText, BackColor = Color.White,
            AutoSize = false, Location = new Point(24, 108), Size = new Size(600, 20)
        };
        var price = new Label
        {
            Text = Fmt.Money(course.Price), Font = new Font("Segoe UI", 18f, FontStyle.Bold),
            ForeColor = Theme.Primary, BackColor = Color.White, AutoSize = false,
            Size = new Size(210, 34), TextAlign = ContentAlignment.MiddleRight
        };
        var note = new Label
        {
            Text = _enrolled ? "You own this course"
                : course.Kind == CourseKind.Offline ? "Choose a batch below"
                : course.AcceptsNewEnrollments ? "One payment, lifetime access"
                : "New enrollments are closed",
            Font = Theme.Small, ForeColor = _enrolled ? Theme.Green : Theme.Muted, BackColor = Color.White,
            AutoSize = false, Size = new Size(210, 20), TextAlign = ContentAlignment.MiddleRight
        };

        card.Controls.Add(title);
        card.Controls.Add(description);
        card.Controls.Add(meta);
        card.Controls.Add(price);
        card.Controls.Add(note);

        if (!_enrolled && course.Kind == CourseKind.Online && course.AcceptsNewEnrollments)
        {
            var button = new PillButton
            {
                Text = "Enroll now", Filled = true, Accent = Theme.Primary, Size = new Size(180, 40)
            };
            button.Click += (_, _) => OpenCourse();
            card.Controls.Add(button);
            card.Resize += (_, _) => button.Location = new Point(card.Width - 204, 112);
        }

        card.Resize += (_, _) =>
        {
            var right = Math.Max(260, card.Width - 280);
            title.Width = right;
            description.Width = right;
            meta.Width = right;
            price.Left = card.Width - 234;
            note.Left = card.Width - 234;
        };
        return card;
    }

    private Card BuildCurriculumCard()
    {
        var card = new Card { BackColor = Color.White };
        AddCardHeader(card, "Curriculum",
            $"{_sections.Count} sections  •  {_sections.Sum(s => s.Lessons.Count(l => !l.IsDeleted))} lessons");

        var table = MakeTable(
            new[] { "Section", "Type", "Lessons", "Your progress" },
            new[] { 2.6f, 1f, 0.9f, 1.6f },
            pillColumns: new[] { 1 });
        table.Location = new Point(20, 72);
        table.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        table.SetRows(_sections.Select(section =>
        {
            var lessons = section.Lessons.Where(l => !l.IsDeleted).ToList();
            var completed = lessons.Count(l => CatalogService.IsLessonCompleted(Session.Student!.Id, l.Id));
            var progress = _enrolled
                ? $"{completed}/{lessons.Count} complete"
                : section.IsPreview ? "Free preview" : "Locked";
            return new[]
            {
                section.Title,
                section.KindLabel,
                lessons.Count.ToString(),
                progress
            };
        }));
        table.RowClicked += (_, index) => OpenSection(_sections[index]);
        card.Controls.Add(table);
        card.Resize += (_, _) => table.Width = Math.Max(100, card.Width - 40);
        table.Width = 500;
        return card;
    }

    private void OpenSection(Section section)
    {
        if (_enrolled || section.IsPreview)
        {
            Navigator.Go("player", _courseId);
            return;
        }
        Navigator.Toast("Enroll in the course to unlock this section.", false);
    }

    private Card BuildIncludesCard(Course course)
    {
        var card = new Card { BackColor = Color.White };
        AddCardHeader(card, "What you get", "Everything included with this course");

        var items = course.Kind == CourseKind.Online
            ? new[]
            {
                ("\uE774", "Lifetime access to every lesson"),
                ("\uE7BE", "Structured sections with previews"),
                ("\uE8A5", "Final exam and certificate"),
                ("\uE8BD", "Section reviews after completion"),
                ("\uE8C7", "Visa, Mastercard, Vodafone Cash or Fawry")
            }
            : new[]
            {
                ("\uE716", "Live sessions in a small batch"),
                ("\uE787", "A fixed schedule with your instructor"),
                ("\uE8A5", "Final exam and certificate"),
                ("\uE8BD", "Feedback after the course ends"),
                ("\uE8C7", "Visa, Mastercard, Vodafone Cash or Fawry")
            };

        var y = 82;
        foreach (var (glyph, text) in items)
        {
            var icon = new Label
            {
                Text = glyph, Font = Theme.Icon, ForeColor = Theme.Accent, BackColor = Color.White,
                AutoSize = false, Location = new Point(22, y), Size = new Size(24, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };
            var label = new Label
            {
                Text = text, Font = Theme.Small, ForeColor = Theme.Muted, BackColor = Color.White,
                AutoSize = false, Location = new Point(54, y + 2), Size = new Size(260, 20)
            };
            card.Controls.Add(icon);
            card.Controls.Add(label);
            y += 38;
        }

        var refund = new Label
        {
            Text = course.Kind == CourseKind.Online
                ? "Online courses are non-refundable after purchase."
                : "Offline refunds: 100% before the start, 50% in the first week.",
            Font = Theme.Small, ForeColor = Theme.SoftText, BackColor = Color.White,
            AutoSize = false, Location = new Point(22, 280), Size = new Size(320, 40)
        };
        card.Controls.Add(refund);
        return card;
    }

    private void BuildBatchTable(Course course)
    {
        var batches = CatalogService.BatchesForCourse(course.Id)
            .Where(b => b.Status != BatchStatus.Cancelled)
            .ToList();
        var table = MakeTable(
            new[] { "Batch", "Starts", "Schedule", "Seats", "Status" },
            new[] { 2.4f, 1.2f, 2.4f, 0.9f, 1.2f },
            pillColumns: new[] { 4 });
        table.SetRows(batches.Select(b => new[]
        {
            b.Title,
            Fmt.Date(b.StartDate),
            b.Schedule,
            b.SeatsLeft + "/" + b.Capacity,
            b.StatusText
        }));
        table.RowClicked += (_, index) =>
        {
            var batch = batches[index];
            if (CatalogService.IsEnrolled(Session.Student!.Id, course.Id))
            {
                Navigator.Toast("You already own this course.", false);
                return;
            }
            if (batch.IsFull)
            {
                Navigator.Toast("This batch is full. Please choose another one.", false);
                return;
            }
            Navigator.Go("checkout", new CheckoutRequest(course.Id, batch.Id));
        };
        AddTableCard("Upcoming batches", "Click a batch to enroll and pay", table,
            emptyText: "There are no upcoming batches for this course.");
    }

    private void BuildReviewTable(Course course)
    {
        var reviews = ReviewService.VisibleForCourse(course.Id).Take(8).ToList();
        var table = MakeTable(
            new[] { "Student", "Comment", "Date" },
            new[] { 1.6f, 5f, 1.2f },
            avatar: true);
        table.SetRows(reviews.Select(r => new[]
        {
            CatalogService.StudentName(r.StudentId),
            r.Text,
            Fmt.Date(r.CreatedOn)
        }));
        table.RowClicked += (_, index) => DetailsDialog.Show(this, "Student review",
            CatalogService.Course(course.Id)?.Title ?? "Course",
            new[]
            {
                ("Student", CatalogService.StudentName(reviews[index].StudentId)),
                ("Date", Fmt.Date(reviews[index].CreatedOn)),
                ("Comment", reviews[index].Text)
            });
        AddTableCard("Student reviews", "What learners say about this course", table,
            emptyText: "No reviews have been written yet.");
    }
}
