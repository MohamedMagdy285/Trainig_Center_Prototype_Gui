using System.Drawing.Drawing2D;

namespace ECMS;

/// <summary>
/// A simple lesson player. It shows the selected lesson, tracks completion,
/// and enforces the preview and sequential-access rules.
/// </summary>
public partial class LessonPlayerPage : BasePage
{
    private readonly Course _course;
    private readonly List<Section> _sections;
    private readonly List<Lesson> _lessons;
    private readonly bool _enrolled;
    private Lesson? _current;

    private readonly Label _sectionLabel = new();
    private readonly Label _titleLabel = new();
    private readonly Label _notesLabel = new();
    private readonly Label _progressLabel = new();
    private readonly Label _previewLabel = new();
    private readonly Panel _progressBar = new();
    private readonly PillButton _completeButton;
    private TableControl _sectionTable = null!;
    private TableControl _lessonTable = null!;
    private Card _lessonCard = null!;
    private List<Lesson> _shownLessons = new();

    /// <summary>Parameterless constructor for the Visual Studio designer.</summary>
    public LessonPlayerPage() : base(_ => { })
    {
        InitializeComponent();
        _course = new Course();
        _sections = new List<Section>();
        _lessons = new List<Lesson>();
        _completeButton = new PillButton();
    }

    public LessonPlayerPage(int courseId) : base(key => Navigator.Go(key))
    {
        InitializeComponent();
        if (DesignMode)
        {
            _course = new Course(); _sections = new List<Section>();
            _lessons = new List<Lesson>(); _completeButton = new PillButton();
            return;
        }
        var course = CatalogService.Course(courseId);
        if (course is null)
        {
            AddBlock(Header("Lesson player", "This course is no longer available."), 60);
            _course = new Course();
            _sections = new List<Section>();
            _lessons = new List<Lesson>();
            _completeButton = new PillButton();
            return;
        }

        _course = course;
        _sections = CatalogService.Sections(courseId).ToList();
        _lessons = _sections.SelectMany(s => s.Lessons.Where(l => !l.IsDeleted).OrderBy(l => l.Order)).ToList();
        _enrolled = CatalogService.IsEnrolled(Session.Student!.Id, courseId);

        AddBlock(Header(course.Title, "Lesson player  •  " + (_enrolled ? "You own this course" : "Preview mode"),
            _enrolled ? "My learning" : "Enroll now", "\uE76C",
            _enrolled ? () => Navigate("learning") : () => Navigator.Go("checkout", new CheckoutRequest(courseId, null))), 60);

        AddBlock(BuildProgressCard(), 104);

        _completeButton = new PillButton
        {
            Text = "Mark as complete", Filled = true, Accent = Theme.Accent, Size = new Size(180, 40)
        };
        _completeButton.Click += (_, _) => MarkComplete();

        AddBlock(Split(BuildContentCard(), BuildSectionCard(), 430, 62f), 430);
        AddBlock(BuildLessonTableCard(), 240);

        _current = InitialLesson();
        RefreshContent();
        RefreshTables();
    }

    private Lesson? InitialLesson()
    {
        if (_lessons.Count == 0) return null;
        if (_enrolled)
        {
            var next = CatalogService.NextLesson(Session.Student!.Id, _course.Id);
            if (next is not null) return next;
        }
        var preview = _lessons.FirstOrDefault(l =>
            _sections.Any(s => s.Id == l.SectionId && s.IsPreview));
        return preview ?? _lessons[0];
    }

    private Card BuildProgressCard()
    {
        var card = new Card { BackColor = Color.White };
        var title = new Label
        {
            Text = "Course progress", Font = Theme.BodyBold, ForeColor = Theme.Ink, BackColor = Color.White,
            AutoSize = true, Location = new Point(24, 16)
        };
        _progressLabel.Font = Theme.Small;
        _progressLabel.ForeColor = Theme.Muted;
        _progressLabel.BackColor = Color.White;
        _progressLabel.AutoSize = true;
        _progressLabel.Location = new Point(24, 38);

        _progressBar.Location = new Point(24, 64);
        _progressBar.Height = 10;
        _progressBar.BackColor = Color.White;
        _progressBar.Paint += (_, e) => DrawProgressBar(e.Graphics);

        _previewLabel.Font = Theme.Small;
        _previewLabel.ForeColor = Theme.Secondary;
        _previewLabel.BackColor = Color.White;
        _previewLabel.AutoSize = true;
        _previewLabel.Location = new Point(24, 80);
        _previewLabel.Text = _enrolled ? "" : "Preview mode: only the first section is unlocked until you enroll.";

        card.Controls.Add(title);
        card.Controls.Add(_progressLabel);
        card.Controls.Add(_progressBar);
        card.Controls.Add(_previewLabel);
        card.Resize += (_, _) => _progressBar.Width = Math.Max(100, card.Width - 48);
        _progressBar.Width = 500;
        return card;
    }

    private void DrawProgressBar(Graphics g)
    {
        Theme.SetupGraphics(g);
        var r = new RectangleF(0, 0, _progressBar.Width, _progressBar.Height);
        Theme.Fill(g, Theme.Field, r, 5);
        var completed = _lessons.Count(l => CatalogService.IsLessonCompleted(Session.Student!.Id, l.Id));
        var ratio = _lessons.Count == 0 ? 0f : completed / (float)_lessons.Count;
        if (ratio > 0)
            Theme.Fill(g, Theme.Accent, new RectangleF(0, 0, Math.Max(6, _progressBar.Width * ratio), _progressBar.Height), 5);
    }

    private Card BuildContentCard()
    {
        var card = new Card { BackColor = Color.White };

        var video = new Panel { Location = new Point(20, 20), Height = 150, BackColor = Color.White };
        video.Paint += (_, e) =>
        {
            var g = e.Graphics;
            Theme.SetupGraphics(g);
            using (var brush = new LinearGradientBrush(video.ClientRectangle, Theme.Primary, Theme.Secondary, 20f))
                g.FillRectangle(brush, video.ClientRectangle);
            using (var glow = new SolidBrush(Color.FromArgb(30, Theme.Accent)))
                g.FillEllipse(glow, video.Width - 140, -50, 220, 220);
            Theme.CenteredText(g, "\uE768", new Font("Segoe MDL2 Assets", 34f), Color.White,
                new Rectangle(0, 0, video.Width, video.Height));
            Theme.CenteredText(g, "Lesson video  •  simulation", Theme.Small, Color.FromArgb(210, 232, 236),
                new Rectangle(0, video.Height - 28, video.Width, 20));
        };

        _sectionLabel.Font = Theme.SmallBold;
        _sectionLabel.ForeColor = Theme.Accent;
        _sectionLabel.BackColor = Color.White;
        _sectionLabel.AutoSize = true;
        _sectionLabel.Location = new Point(22, 182);

        _titleLabel.Font = Theme.H2;
        _titleLabel.ForeColor = Theme.Ink;
        _titleLabel.BackColor = Color.White;
        _titleLabel.AutoSize = false;
        _titleLabel.Location = new Point(22, 204);
        _titleLabel.Size = new Size(500, 30);

        _notesLabel.Font = Theme.Body;
        _notesLabel.ForeColor = Theme.Muted;
        _notesLabel.BackColor = Color.White;
        _notesLabel.AutoSize = false;
        _notesLabel.Location = new Point(22, 242);
        _notesLabel.Size = new Size(500, 92);

        var previous = new PillButton { Text = "Previous", Filled = false, Size = new Size(110, 38) };
        var next = new PillButton { Text = "Next", Filled = false, Size = new Size(90, 38) };
        previous.Click += (_, _) => MoveLesson(-1);
        next.Click += (_, _) => MoveLesson(1);

        card.Controls.Add(video);
        card.Controls.Add(_sectionLabel);
        card.Controls.Add(_titleLabel);
        card.Controls.Add(_notesLabel);
        card.Controls.Add(previous);
        card.Controls.Add(next);
        card.Controls.Add(_completeButton);

        card.Resize += (_, _) =>
        {
            video.Width = Math.Max(200, card.Width - 40);
            _titleLabel.Width = Math.Max(200, card.Width - 44);
            _notesLabel.Width = Math.Max(200, card.Width - 44);
            previous.Location = new Point(22, card.Height - 58);
            next.Location = new Point(142, card.Height - 58);
            _completeButton.Location = new Point(card.Width - 204, card.Height - 58);
        };
        return card;
    }

    private Card BuildSectionCard()
    {
        var card = new Card { BackColor = Color.White };
        AddCardHeader(card, "Sections", "Jump to a part of the course");

        _sectionTable = MakeTable(
            new[] { "Section", "Type", "Lessons", "Done" },
            new[] { 2.2f, 0.9f, 0.8f, 0.8f },
            pillColumns: new[] { 1 });
        _sectionTable.Location = new Point(20, 72);
        _sectionTable.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _sectionTable.RowClicked += (_, index) =>
        {
            var section = _sections[index];
            var lesson = section.Lessons.Where(l => !l.IsDeleted).OrderBy(l => l.Order).FirstOrDefault();
            if (lesson is not null) OpenLesson(lesson);
        };
        card.Controls.Add(_sectionTable);
        card.Resize += (_, _) => _sectionTable.Width = Math.Max(100, card.Width - 40);
        _sectionTable.Width = 400;
        return card;
    }

    private Card BuildLessonTableCard()
    {
        _lessonTable = MakeTable(
            new[] { "Section", "Lesson", "Minutes", "Status" },
            new[] { 2f, 3.6f, 0.8f, 1.2f },
            pillColumns: new[] { 3 });
        _lessonTable.RowClicked += (_, index) => OpenLesson(_shownLessons[index]);
        _lessonCard = AddTableCard("All lessons", "Click any available lesson to open it", _lessonTable,
            emptyText: "This course has no lessons yet.");
        return _lessonCard;
    }

    private void RefreshContent()
    {
        if (_current is null)
        {
            _titleLabel.Text = "No lessons yet";
            _sectionLabel.Text = "";
            _notesLabel.Text = "The instructor has not added any lessons.";
            _completeButton.Visible = false;
            return;
        }

        var section = _sections.FirstOrDefault(s => s.Id == _current.SectionId);
        _sectionLabel.Text = (section?.Title ?? "Section") + "  •  " + Fmt.Minutes(_current.Minutes);
        _titleLabel.Text = _current.Title;
        _notesLabel.Text = string.IsNullOrWhiteSpace(_current.Notes)
            ? "In this lesson you will work through the topic step by step. Mark the lesson complete when you are ready to continue."
            : _current.Notes;

        var completed = CatalogService.IsLessonCompleted(Session.Student!.Id, _current.Id);
        _completeButton.Text = completed ? "Completed" : "Mark as complete";
        _completeButton.Accent = completed ? Theme.Muted : Theme.Accent;
        _completeButton.Visible = _enrolled;
        _completeButton.Invalidate();

        var done = _lessons.Count(l => CatalogService.IsLessonCompleted(Session.Student!.Id, l.Id));
        _progressLabel.Text = $"{done} of {_lessons.Count} lessons completed";
        _progressBar.Invalidate();
    }

    private void RefreshTables()
    {
        _sectionTable.SetRows(_sections.Select(section =>
        {
            var lessons = section.Lessons.Where(l => !l.IsDeleted).ToList();
            var done = lessons.Count(l => CatalogService.IsLessonCompleted(Session.Student!.Id, l.Id));
            return new[]
            {
                section.Title,
                section.KindLabel,
                lessons.Count.ToString(),
                done + "/" + lessons.Count
            };
        }));

        _shownLessons = _lessons;
        _lessonTable.SetRows(_lessons.Select(lesson =>
        {
            var section = _sections.FirstOrDefault(s => s.Id == lesson.SectionId);
            return new[]
            {
                section?.Title ?? "Section",
                lesson.Title,
                lesson.Minutes + " min",
                StatusOf(lesson)
            };
        }));
        FitTableCard(_lessonCard, _lessonTable);
        RefreshLayout();
    }

    private string StatusOf(Lesson lesson)
    {
        if (CatalogService.IsLessonCompleted(Session.Student!.Id, lesson.Id)) return "Completed";
        var section = _sections.FirstOrDefault(s => s.Id == lesson.SectionId);
        if (!_enrolled) return section?.IsPreview == true ? "Preview" : "Locked";
        return CatalogService.CanOpenLesson(Session.Student!.Id, _course.Id, lesson.Id) ? "Available" : "Locked";
    }

    private void OpenLesson(Lesson lesson)
    {
        var student = Session.Student!;
        var section = _sections.FirstOrDefault(s => s.Id == lesson.SectionId);
        var completed = CatalogService.IsLessonCompleted(student.Id, lesson.Id);

        if (!_enrolled && section?.IsPreview != true)
        {
            Navigator.Toast("Enroll in the course to unlock this lesson.", false);
            return;
        }

        if (_enrolled && !completed && !CatalogService.CanOpenLesson(student.Id, _course.Id, lesson.Id))
        {
            Navigator.Toast("Complete the previous lesson first.", false);
            return;
        }

        _current = lesson;
        RefreshContent();
    }

    private void MoveLesson(int direction)
    {
        if (_current is null) return;
        var index = _lessons.FindIndex(l => l.Id == _current.Id);
        var next = index + direction;
        if (next < 0 || next >= _lessons.Count)
        {
            Navigator.Toast(direction > 0 ? "You are on the last lesson." : "You are on the first lesson.");
            return;
        }
        OpenLesson(_lessons[next]);
    }

    private void MarkComplete()
    {
        if (_current is null) return;
        if (!_enrolled)
        {
            Navigator.Toast("Enroll to save your progress.", false);
            return;
        }

        var student = Session.Student!;
        if (CatalogService.IsLessonCompleted(student.Id, _current.Id))
        {
            Navigator.Toast("This lesson is already complete.");
            return;
        }

        AppData.Completions.Add(new LessonCompletion
        {
            Id = AppData.NextId(AppData.Completions, c => c.Id),
            StudentId = student.Id,
            LessonId = _current.Id,
            CompletedOn = DateTime.Today
        });

        Navigator.Toast("Lesson completed. Keep going!");
        RefreshContent();
        RefreshTables();
    }
}
