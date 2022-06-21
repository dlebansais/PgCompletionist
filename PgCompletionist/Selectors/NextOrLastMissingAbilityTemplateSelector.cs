namespace Selectors;

public class NextOrLastMissingAbilityTemplateSelector : NextOrLastTemplateSelector
{
    protected override string NextTemplateName { get { return "NextMissingAbilityTemplate"; } }
    protected override string LastTemplateName { get { return "LastMissingAbilityTemplate"; } }
}
