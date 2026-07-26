using System.Diagnostics;
using ProjectLEGO.Core.Legos;
using ProjectLEGO.Core.Legos.Insert;
using ProjectLEGO.Core.Legos.Search;
using ProjectLEGO.Core.Templates;
using ProjectLEGO.Desktop.Services;

namespace ProjectLEGO.Desktop;

public partial class MainForm : Form
{
    private readonly string _repositoryRoot;
    private IReadOnlyList<LegoTemplate> _templates = [];
    private LegoSearchService? _searchService;
    private LegoSearchResult? _selected;

    public MainForm(string repositoryRoot)
    {
        _repositoryRoot = repositoryRoot;
        InitializeComponent();
        Load += OnLoad;
        searchButton.Click += (_, _) => RunSearch();
        searchBox.KeyDown += (_, eventArgs) => { if (eventArgs.KeyCode == Keys.Enter) { RunSearch(); eventArgs.SuppressKeyPress = true; } };
        resetButton.Click += (_, _) => ResetSearch();
        templateCombo.SelectedIndexChanged += (_, _) => { filterPanel.SetTemplate(SelectedTemplate()); RunSearch(); };
        categoryCombo.SelectedIndexChanged += (_, _) => RunSearch();
        sortCombo.SelectedIndexChanged += (_, _) => RunSearch();
        resultList.SelectedIndexChanged += (_, _) => UpdateSelection();
        resultList.DoubleClick += (_, _) => RaiseInsertRequest();
        insertButton.Click += (_, _) => RaiseInsertRequest();
        openFolderButton.Click += (_, _) => OpenAssetFolder();
    }

    public event EventHandler<LegoInsertRequestedEventArgs>? LegoInsertRequested;

    private void OnLoad(object? sender, EventArgs eventArgs)
    {
        var templateResult = new TemplateLoader().LoadAll(Path.Combine(_repositoryRoot, "data", "templates"));
        _templates = templateResult.Templates;
        var recordResult = new JsonLegoRepository(Path.Combine(_repositoryRoot, "data", "legos"), _templates).LoadAll();
        _searchService = new(recordResult.Records, _templates);

        templateCombo.Items.Add(new TemplateChoice(null, "전체"));
        foreach (var template in _templates.OrderBy(template => template.Name)) templateCombo.Items.Add(new TemplateChoice(template, template.Name));
        templateCombo.DisplayMember = nameof(TemplateChoice.Name);
        templateCombo.SelectedIndex = 0;
        categoryCombo.Items.Add("전체");
        foreach (var category in recordResult.Records.Select(record => record.Category).Distinct().OrderBy(value => value)) categoryCombo.Items.Add(category);
        categoryCombo.SelectedIndex = 0;
        sortCombo.Items.AddRange(["관련도순", "이름 오름차순", "이름 내림차순", "최근 수정순"]);
        sortCombo.SelectedIndex = 0;
        statusLabel.Text = $"Template 오류 {templateResult.Errors.Count}건 · LEGO 오류 {recordResult.Errors.Count}건";
        RunSearch();
    }

    private void RunSearch()
    {
        if (_searchService is null || templateCombo.SelectedIndex < 0 || categoryCombo.SelectedIndex < 0 || sortCombo.SelectedIndex < 0) return;
        var request = new LegoSearchRequest
        {
            Keyword = searchBox.Text,
            TemplateId = SelectedTemplate()?.TemplateId,
            Category = categoryCombo.SelectedIndex == 0 ? null : categoryCombo.SelectedItem?.ToString(),
            Filters = filterPanel.GetFilters(),
            Sort = (LegoSortOption)sortCombo.SelectedIndex
        };
        var results = _searchService.Search(request);
        resultList.BeginUpdate();
        resultList.Items.Clear();
        foreach (var result in results)
        {
            var item = new ListViewItem(result.Record.Name) { Tag = result };
            item.SubItems.Add(result.Template.Name); item.SubItems.Add(result.Record.Category); item.SubItems.Add(result.SpecificationSummary); item.SubItems.Add(string.Join(", ", result.Record.Tags));
            resultList.Items.Add(item);
        }
        resultList.EndUpdate();
        countLabel.Text = results.Count == 0 ? "조건에 맞는 LEGO가 없습니다. 검색어나 상세 필터를 변경해 보세요." : $"검색 결과 {results.Count}개";
        ClearSelection();
    }

    private void ResetSearch()
    {
        searchBox.Clear(); templateCombo.SelectedIndex = 0; categoryCombo.SelectedIndex = 0; sortCombo.SelectedIndex = 0; filterPanel.SetTemplate(null); RunSearch();
    }

    private LegoTemplate? SelectedTemplate() => (templateCombo.SelectedItem as TemplateChoice)?.Template;

    private void UpdateSelection()
    {
        _selected = resultList.SelectedItems.Count == 1 ? resultList.SelectedItems[0].Tag as LegoSearchResult : null;
        selectionLabel.Text = _selected is null ? "선택: 없음" : $"선택: {_selected.Record.Name} / {_selected.Record.LegoId}";
        insertButton.Enabled = _selected is not null;
        openFolderButton.Enabled = _selected is not null;
    }

    private void ClearSelection()
    {
        _selected = null; selectionLabel.Text = "선택: 없음"; insertButton.Enabled = false; openFolderButton.Enabled = false;
    }

    private void RaiseInsertRequest()
    {
        var request = LegoInsertRequestFactory.Create(_selected?.Record);
        if (request is null) return;
        LegoInsertRequested?.Invoke(this, new(request));
        MessageBox.Show(this, "삽입 요청이 생성되었습니다.\r\n실제 ZWCAD 삽입 기능은 후속 단계에서 연결됩니다.", "Project LEGO", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OpenAssetFolder()
    {
        if (_selected is null) return;
        try
        {
            var asset = Path.GetFullPath(Path.Combine(_repositoryRoot, _selected.Record.AssetPath));
            var folder = File.Exists(asset) ? Path.GetDirectoryName(asset) : Directory.Exists(asset) ? asset : null;
            if (folder is null) throw new DirectoryNotFoundException("샘플 AssetPath가 존재하지 않습니다.");
            Process.Start(new ProcessStartInfo("explorer.exe", folder) { UseShellExecute = true });
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.ComponentModel.Win32Exception)
        {
            MessageBox.Show(this, exception.Message, "폴더를 열 수 없습니다", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private sealed record TemplateChoice(LegoTemplate? Template, string Name);
}
