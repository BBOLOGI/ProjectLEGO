using ProjectLEGO.Core.Legos;
using ProjectLEGO.Core.Legos.Insert;
using ProjectLEGO.Core.Legos.Search;
using ProjectLEGO.Core.Templates;
using ProjectLEGO.Desktop.Services;

namespace ProjectLEGO.Desktop;

public partial class MainForm : Form
{
    private readonly string _repositoryRoot;
    private IHierarchicalLegoSearchService? _searchService;
    private string? _selectedTemplateId;
    private IReadOnlyList<string> _selectedSpecLabels = [];
    private LegoRecord? _finalRecord;
    private bool _updatingSpecs;

    public MainForm(string repositoryRoot)
    {
        _repositoryRoot = repositoryRoot;
        InitializeComponent();
        Load += OnLoad;
        searchButton.Click += (_, _) => RunSearch();
        searchBox.KeyDown += (_, eventArgs) => { if (eventArgs.KeyCode == Keys.Enter) { RunSearch(); eventArgs.SuppressKeyPress = true; } };
        resultList.SelectedIndexChanged += (_, _) => SelectSearchResult();
        for (var index = 0; index < specCombos.Length; index++)
        {
            var level = index;
            specCombos[index].SelectedIndexChanged += (_, _) => OnSpecificationChanged(level);
        }
        openButton.Click += (_, _) => OpenSelected();
        insertButton.Click += (_, _) => RaiseInsertRequest();
        closeButton.Click += (_, _) => Close();
    }

    public event EventHandler<LegoInsertRequestedEventArgs>? LegoInsertRequested;

    private void OnLoad(object? sender, EventArgs eventArgs)
    {
        var templateResult = new TemplateLoader().LoadAll(Path.Combine(_repositoryRoot, "data", "templates"));
        ILegoRepository repository = new JsonLegoRepository(Path.Combine(_repositoryRoot, "data", "legos"), templateResult.Templates);
        var recordResult = repository.LoadAll();
        _searchService = new HierarchicalLegoSearchService(repository, templateResult.Templates);
        statusLabel.Text = $"Template 오류 {templateResult.Errors.Count}건 · LEGO 오류 {recordResult.Errors.Count}건";
        ShowEmptySearchResults();
    }

    private void RunSearch()
    {
        if (_searchService is null) return;
        var results = _searchService.Search(searchBox.Text);
        resultList.BeginUpdate();
        resultList.Items.Clear();
        foreach (var result in results)
        {
            var item = new ListViewItem(result.Template.Name) { Tag = result };
            item.SubItems.Add(result.MatchingRecordCount.ToString());
            resultList.Items.Add(item);
        }
        resultList.EndUpdate();
        resultCountLabel.Text = results.Count == 0 ? "검색 결과 0개" : $"검색 결과 {results.Count}개";
        ResetSpecifications();
    }

    private void SelectSearchResult()
    {
        if (_searchService is null || resultList.SelectedItems.Count != 1) return;
        var result = (HierarchicalSearchResult)resultList.SelectedItems[0].Tag!;
        _selectedTemplateId = result.Template.TemplateId;
        _selectedSpecLabels = result.Template.Fields.Where(field => field.Searchable).OrderBy(field => field.SortOrder).Take(4).Select(field => field.DisplayName).ToArray();
        _updatingSpecs = true;
        ResetSpecificationControls();
        SetOptions(0, _searchService.GetSpec1(_selectedTemplateId), SpecLabel(0));
        _updatingSpecs = false;
        UpdateFinalRecord();
    }

    private void OnSpecificationChanged(int level)
    {
        if (_updatingSpecs || _searchService is null || _selectedTemplateId is null) return;
        _updatingSpecs = true;
        for (var index = level + 1; index < 4; index++) ClearSpec(index);
        var values = SelectedValues();
        if (level == 0 && values[0] is not null) SetOptions(1, _searchService.GetSpec2(_selectedTemplateId, values[0]!), SpecLabel(1));
        if (level == 1 && values[0] is not null && values[1] is not null) SetOptions(2, _searchService.GetSpec3(_selectedTemplateId, values[0]!, values[1]!), SpecLabel(2));
        if (level == 2 && values[0] is not null && values[1] is not null && values[2] is not null) SetOptions(3, _searchService.GetSpec4(_selectedTemplateId, values[0]!, values[1]!, values[2]!), SpecLabel(3));
        _updatingSpecs = false;
        UpdateFinalRecord();
    }

    private void UpdateFinalRecord()
    {
        if (_searchService is null || _selectedTemplateId is null) { SetFinal(null); return; }
        var values = SelectedValues();
        SetFinal(_searchService.FindFinalRecord(_selectedTemplateId, values[0], values[1], values[2], values[3]));
    }

    private void SetFinal(LegoRecord? record)
    {
        _finalRecord = record;
        openButton.Enabled = record is not null;
        insertButton.Enabled = record is not null;
        previewFileLabel.Text = record is null ? "-" : Path.GetFileName(record.AssetPath);
        previewPathLabel.Text = record?.AssetPath ?? "-";
    }

    private void SetOptions(int index, IReadOnlyList<LegoSpecificationOption> options, string label)
    {
        if (options.Count == 0) return;
        specLabels[index].Text = label;
        specLabels[index].Enabled = true;
        specCombos[index].Enabled = true;
        specCombos[index].DisplayMember = nameof(LegoSpecificationOption.DisplayName);
        specCombos[index].Items.AddRange(options.Cast<object>().ToArray());
    }

    private string?[] SelectedValues() => specCombos.Select(combo => (combo.SelectedItem as LegoSpecificationOption)?.Value).ToArray();

    private void ResetSpecifications()
    {
        _selectedTemplateId = null;
        _selectedSpecLabels = [];
        _updatingSpecs = true;
        ResetSpecificationControls();
        _updatingSpecs = false;
        SetFinal(null);
    }

    private void ShowEmptySearchResults()
    {
        resultList.Items.Clear();
        resultCountLabel.Text = "검색 결과 0개";
        ResetSpecifications();
    }

    private void ResetSpecificationControls() { for (var index = 0; index < 4; index++) ClearSpec(index); }
    private string SpecLabel(int index) => index < _selectedSpecLabels.Count ? _selectedSpecLabels[index] : $"규격{index + 1}";
    private void ClearSpec(int index) { specCombos[index].Items.Clear(); specCombos[index].Enabled = false; specLabels[index].Text = $"규격{index + 1}"; specLabels[index].Enabled = false; }

    private void OpenSelected()
    {
        if (_finalRecord is null) return;
        MessageBox.Show(this, $"열기 요청: {_finalRecord.AssetPath}\r\n실제 도면 열기는 후속 Adapter에서 연결됩니다.", "Project LEGO", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void RaiseInsertRequest()
    {
        var request = LegoInsertRequestFactory.Create(_finalRecord);
        if (request is null) return;
        LegoInsertRequested?.Invoke(this, new(request));
        MessageBox.Show(this, "삽입 요청이 생성되었습니다.\r\n실제 ZWCAD 삽입은 포함되지 않습니다.", "Project LEGO", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
