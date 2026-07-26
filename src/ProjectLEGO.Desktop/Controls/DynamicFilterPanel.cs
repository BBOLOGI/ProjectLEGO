using ProjectLEGO.Core.Legos.Search;
using ProjectLEGO.Core.Templates;

namespace ProjectLEGO.Desktop.Controls;

public sealed class DynamicFilterPanel : FlowLayoutPanel
{
    private readonly List<(TemplateField Field, Control Control)> _controls = [];

    public DynamicFilterPanel()
    {
        AutoScroll = true;
        FlowDirection = FlowDirection.TopDown;
        WrapContents = false;
        Padding = new(8);
    }

    public void SetTemplate(LegoTemplate? template)
    {
        SuspendLayout();
        Controls.Clear();
        _controls.Clear();
        if (template is null)
        {
            Controls.Add(new Label { Text = "LEGO 종류를 선택하면\r\n상세 필터가 표시됩니다.", AutoSize = true, ForeColor = Color.DimGray });
            ResumeLayout();
            return;
        }

        foreach (var field in template.Fields.Where(field => field.Searchable).OrderBy(field => field.SortOrder))
        {
            var group = new GroupBox { Text = string.IsNullOrWhiteSpace(field.Unit) ? field.DisplayName : $"{field.DisplayName} ({field.Unit})", Width = 220, AutoSize = true, Padding = new(8) };
            var control = CreateControl(field);
            control.Width = 190;
            group.Controls.Add(control);
            group.Height = control.Height + 42;
            Controls.Add(group);
            _controls.Add((field, control));
        }

        ResumeLayout();
    }

    public IReadOnlyList<LegoSearchFilter> GetFilters()
    {
        var filters = new List<LegoSearchFilter>();
        foreach (var (field, control) in _controls)
        {
            switch (field.DataType)
            {
                case TemplateDataType.Text:
                    var textPanel = (TableLayoutPanel)control;
                    var text = ((TextBox)textPanel.Controls[0]).Text.Trim();
                    if (text.Length > 0) filters.Add(new() { InternalName = field.InternalName, Operator = ((ComboBox)textPanel.Controls[1]).SelectedIndex == 0 ? LegoFilterOperator.TextContains : LegoFilterOperator.TextEquals, TextValue = text });
                    break;
                case TemplateDataType.Number:
                    var numberPanel = (TableLayoutPanel)control;
                    var minimum = ((NumericUpDown)numberPanel.Controls[0]).Value;
                    var maximum = ((NumericUpDown)numberPanel.Controls[1]).Value;
                    if (minimum != 0 || maximum != 0) filters.Add(new() { InternalName = field.InternalName, Operator = LegoFilterOperator.NumberRange, Minimum = minimum == 0 ? null : minimum, Maximum = maximum == 0 ? null : maximum });
                    break;
                case TemplateDataType.Boolean:
                    var boolean = (ComboBox)control;
                    if (boolean.SelectedIndex > 0) filters.Add(new() { InternalName = field.InternalName, Operator = LegoFilterOperator.BooleanEquals, BooleanValue = boolean.SelectedIndex == 1 });
                    break;
                case TemplateDataType.Select:
                    var selected = ((CheckedListBox)control).CheckedItems.Cast<TemplateOption>().Select(option => option.Value).ToArray();
                    if (selected.Length > 0) filters.Add(new() { InternalName = field.InternalName, Operator = LegoFilterOperator.SelectAny, SelectedValues = selected });
                    break;
            }
        }

        return filters;
    }

    private static Control CreateControl(TemplateField field)
    {
        return field.DataType switch
        {
            TemplateDataType.Number => NumberControl(),
            TemplateDataType.Boolean => Combo(["전체", "예", "아니오"]),
            TemplateDataType.Select => SelectControl(field),
            _ => TextControl()
        };
    }

    private static TableLayoutPanel TextControl()
    {
        var panel = new TableLayoutPanel { ColumnCount = 1, RowCount = 2, AutoSize = true };
        panel.Controls.Add(new TextBox { Width = 185 });
        panel.Controls.Add(Combo(["포함", "정확히 일치"]));
        return panel;
    }

    private static TableLayoutPanel NumberControl()
    {
        var panel = new TableLayoutPanel { ColumnCount = 2, RowCount = 2, AutoSize = true };
        panel.Controls.Add(new Label { Text = "최소", AutoSize = true });
        panel.Controls.Add(new Label { Text = "최대", AutoSize = true });
        panel.Controls.Add(new NumericUpDown { DecimalPlaces = 2, Maximum = 1000000, Width = 90 });
        panel.Controls.Add(new NumericUpDown { DecimalPlaces = 2, Maximum = 1000000, Width = 90 });
        return panel;
    }

    private static CheckedListBox SelectControl(TemplateField field)
    {
        var list = new CheckedListBox { Height = Math.Min(110, 25 + field.Options.Count * 22), CheckOnClick = true, DisplayMember = nameof(TemplateOption.DisplayName) };
        list.Items.AddRange(field.Options.OrderBy(option => option.SortOrder).Cast<object>().ToArray());
        return list;
    }

    private static ComboBox Combo(object[] items)
    {
        var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 185 };
        combo.Items.AddRange(items);
        combo.SelectedIndex = 0;
        return combo;
    }
}
