using ProjectLEGO.Desktop.Controls;

namespace ProjectLEGO.Desktop;

partial class MainForm
{
    private TextBox searchBox = null!;
    private Button searchButton = null!;
    private Button resetButton = null!;
    private ComboBox templateCombo = null!;
    private ComboBox categoryCombo = null!;
    private ComboBox sortCombo = null!;
    private DynamicFilterPanel filterPanel = null!;
    private ListView resultList = null!;
    private Label countLabel = null!;
    private Label selectionLabel = null!;
    private Label statusLabel = null!;
    private Button openFolderButton = null!;
    private Button insertButton = null!;

    private void InitializeComponent()
    {
        Text = "Project LEGO";
        MinimumSize = new Size(980, 650);
        Size = new Size(1180, 760);
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1, Padding = new(10) };
        root.RowStyles.Add(new(SizeType.Absolute, 52));
        root.RowStyles.Add(new(SizeType.Percent, 100));
        root.RowStyles.Add(new(SizeType.Absolute, 48));
        root.RowStyles.Add(new(SizeType.Absolute, 28));

        var searchBar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
        searchBar.ColumnStyles.Add(new(SizeType.Percent, 100));
        searchBar.ColumnStyles.Add(new(SizeType.Absolute, 90));
        searchBar.ColumnStyles.Add(new(SizeType.Absolute, 90));
        searchBar.ColumnStyles.Add(new(SizeType.Absolute, 150));
        searchBox = new() { Dock = DockStyle.Fill, PlaceholderText = "검색어를 입력하세요" };
        searchButton = new() { Text = "검색", Dock = DockStyle.Fill };
        resetButton = new() { Text = "초기화", Dock = DockStyle.Fill };
        sortCombo = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        searchBar.Controls.Add(searchBox, 0, 0); searchBar.Controls.Add(searchButton, 1, 0); searchBar.Controls.Add(resetButton, 2, 0); searchBar.Controls.Add(sortCombo, 3, 0);

        var content = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1, SplitterDistance = 250 };
        var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5 };
        left.RowStyles.Add(new(SizeType.Absolute, 26)); left.RowStyles.Add(new(SizeType.Absolute, 36)); left.RowStyles.Add(new(SizeType.Absolute, 26)); left.RowStyles.Add(new(SizeType.Absolute, 36)); left.RowStyles.Add(new(SizeType.Percent, 100));
        left.Controls.Add(new Label { Text = "LEGO 종류", Font = new Font(Font, FontStyle.Bold), AutoSize = true }, 0, 0);
        templateCombo = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        left.Controls.Add(templateCombo, 0, 1);
        left.Controls.Add(new Label { Text = "Category", Font = new Font(Font, FontStyle.Bold), AutoSize = true }, 0, 2);
        categoryCombo = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        left.Controls.Add(categoryCombo, 0, 3);
        filterPanel = new() { Dock = DockStyle.Fill };
        left.Controls.Add(filterPanel, 0, 4);
        content.Panel1.Controls.Add(left);

        var right = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        right.RowStyles.Add(new(SizeType.Absolute, 30)); right.RowStyles.Add(new(SizeType.Percent, 100));
        countLabel = new() { Text = "검색 결과 0개", AutoSize = true, Font = new Font(Font, FontStyle.Bold) };
        resultList = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false, HideSelection = false };
        resultList.Columns.Add("이름", 190); resultList.Columns.Add("Template", 110); resultList.Columns.Add("Category", 110); resultList.Columns.Add("주요 규격", 360); resultList.Columns.Add("Tags", 180);
        right.Controls.Add(countLabel, 0, 0); right.Controls.Add(resultList, 0, 1);
        content.Panel2.Controls.Add(right);

        var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
        bottom.ColumnStyles.Add(new(SizeType.Percent, 100)); bottom.ColumnStyles.Add(new(SizeType.Absolute, 110)); bottom.ColumnStyles.Add(new(SizeType.Absolute, 120));
        selectionLabel = new() { Text = "선택: 없음", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        openFolderButton = new() { Text = "폴더 열기", Dock = DockStyle.Fill, Enabled = false };
        insertButton = new() { Text = "삽입 요청", Dock = DockStyle.Fill, Enabled = false };
        bottom.Controls.Add(selectionLabel, 0, 0); bottom.Controls.Add(openFolderButton, 1, 0); bottom.Controls.Add(insertButton, 2, 0);
        statusLabel = new() { Text = "준비", Dock = DockStyle.Fill, ForeColor = Color.DimGray };

        root.Controls.Add(searchBar, 0, 0); root.Controls.Add(content, 0, 1); root.Controls.Add(bottom, 0, 2); root.Controls.Add(statusLabel, 0, 3);
        Controls.Add(root);
    }
}
