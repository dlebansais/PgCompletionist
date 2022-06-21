namespace Selectors;

public class NextOrLastMissingRecipeTemplateSelector : NextOrLastTemplateSelector
{
    protected override string NextTemplateName { get { return "NextMissingRecipeTemplate"; } }
    protected override string LastTemplateName { get { return "LastMissingRecipeTemplate"; } }
}
