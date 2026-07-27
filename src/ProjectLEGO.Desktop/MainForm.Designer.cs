namespace ProjectLEGO.Desktop;

partial class MainForm
{
    private TextBox searchBox = null!;
    private Button searchButton = null!;
    private ListView resultList = null!;
    private Label resultCountLabel = null!;
    private readonly Label[] specLabels = new Label[4];
    private readonly ComboBox[] specCombos = new ComboBox[4];
    private Label previewFileLabel = null!;
    private Label previewPathLabel = null!;
    private Label statusLabel = null!;
    private Button openButton = null!;
    private Button insertButton = null!;
    private Button closeButton = null!;

    private void InitializeComponent()
    {
        Text = "Project LEGO 검색 UI v2.0";
        MinimumSize = new Size(1000, 680);
        Size = new Size(1180, 780);
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 1, Padding = new(12) };
        root.RowStyles.Add(new(SizeType.Absolute, 52));
        root.RowStyles.Add(new(SizeType.Percent, 62));
        root.RowStyles.Add(new(SizeType.Percent, 28));
        root.RowStyles.Add(new(SizeType.Absolute, 52));
        root.RowStyles.Add(new(SizeType.Absolute, 26));

        var searchPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
        searchPanel.ColumnStyles.Add(new(SizeType.Absolute, 55));
        searchPanel.ColumnStyles.Add(new(SizeType.Percent, 100));
        searchPanel.ColumnStyles.Add(new(SizeType.Absolute, 90));
        searchPanel.Controls.Add(new Label { Text = "검색 :", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font(Font, FontStyle.Bold) }, 0, 0);
        searchBox = new() { Dock = DockStyle.Fill, PlaceholderText = "이름, 규격, 형식, 키워드를 입력하세요" };
        searchButton = new() { Text = "검색", Dock = DockStyle.Fill };
        searchPanel.Controls.Add(searchBox, 1, 0);
        searchPanel.Controls.Add(searchButton, 2, 0);

        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 590 };
        var resultPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        resultPanel.RowStyles.Add(new(SizeType.Absolute, 32)); resultPanel.RowStyles.Add(new(SizeType.Percent, 100));
        resultCountLabel = new() { Text = "검색 결과 0개", Dock = DockStyle.Fill, Font = new Font(Font, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        resultList = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false, HideSelection = false };
        resultList.Columns.Add("LEGO 이름", 360);
        resultList.Columns.Add("일치 레코드", 120);
        resultPanel.Controls.Add(resultCountLabel, 0, 0); resultPanel.Controls.Add(resultList, 0, 1);
        split.Panel1.Controls.Add(resultPanel);

        var specs = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 9, ColumnCount = 1, Padding = new(14) };
        specs.RowStyles.Add(new(SizeType.Absolute, 38));
        specs.Controls.Add(new Label { Text = "규격 선택", Dock = DockStyle.Fill, Font = new Font(Font.FontFamily, 13, FontStyle.Bold) }, 0, 0);
        for (var index = 0; index < 4; index++)
        {
            specLabels[index] = new Label { Text = $"규격{index + 1}", AutoSize = true, Enabled = false };
            specCombos[index] = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            specs.RowStyles.Add(new(SizeType.Absolute, 28)); specs.RowStyles.Add(new(SizeType.Absolute, 44));
            specs.Controls.Add(specLabels[index], 0, index * 2 + 1); specs.Controls.Add(specCombos[index], 0, index * 2 + 2);
        }
        split.Panel2.Controls.Add(specs);

        var preview = new GroupBox { Text = "Preview", Dock = DockStyle.Fill, Padding = new(16) };
        var previewLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 2 };
        previewLayout.ColumnStyles.Add(new(SizeType.Absolute, 80)); previewLayout.ColumnStyles.Add(new(SizeType.Percent, 100));
        previewLayout.Controls.Add(new Label { Text = "파일명", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 0, 0);
        previewLayout.Controls.Add(new Label { Text = "경로", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 0, 1);
        previewFileLabel = new() { Text = "-", AutoSize = true }; previewPathLabel = new() { Text = "-", AutoSize = true };
        previewLayout.Controls.Add(previewFileLabel, 1, 0); previewLayout.Controls.Add(previewPathLabel, 1, 1);
        preview.Controls.Add(previewLayout);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new(0, 7, 0, 0) };
        openButton = new() { Text = "열기", Width = 110, Height = 34, Enabled = false };
        insertButton = new() { Text = "삽입", Width = 110, Height = 34, Enabled = false };
        closeButton = new() { Text = "닫기", Width = 110, Height = 34 };
        buttons.Controls.Add(openButton); buttons.Controls.Add(insertButton); buttons.Controls.Add(closeButton);
        statusLabel = new() { Text = "준비", Dock = DockStyle.Fill, ForeColor = Color.DimGray };

        root.Controls.Add(searchPanel, 0, 0); root.Controls.Add(split, 0, 1); root.Controls.Add(preview, 0, 2); root.Controls.Add(buttons, 0, 3); root.Controls.Add(statusLabel, 0, 4);
        Controls.Add(root);
    }
}
