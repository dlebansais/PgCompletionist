namespace Selectors;

using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using WpfLayout;

public abstract class NextOrLastTemplateSelector : DataTemplateSelector
{
    protected abstract string NextTemplateName { get; }
    protected abstract string LastTemplateName { get; }

    public override DataTemplate SelectTemplate(object? item, DependencyObject? container)
    {
        if (container is null)
            throw new ArgumentNullException(nameof(container));

        if (!SelectorTools.FindItemOwnerControl(container, out ItemsControl Control))
            throw new ArgumentException($"Container '{container}' not supported in {this}.");

        string TemplateName = NextTemplateName;

        if (Control.Items is IList AsIList)
        {
            int Index = AsIList.IndexOf(item);
            if (Index + 1 >= Control.Items.Count)
                TemplateName = LastTemplateName;
        }

        DataTemplate? Result = Control.FindResource(TemplateName) as DataTemplate;
        return Result ?? throw new TypeLoadException($"Unexpected missing resource {TemplateName}.");
    }
}
