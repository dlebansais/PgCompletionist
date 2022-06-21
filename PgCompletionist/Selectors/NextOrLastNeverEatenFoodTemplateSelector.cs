namespace Selectors;

public class NextOrLastNeverEatenFoodTemplateSelector : NextOrLastTemplateSelector
{
    protected override string NextTemplateName { get { return "NextNeverEatenFoodTemplate"; } }
    protected override string LastTemplateName { get { return "LastNeverEatenFoodTemplate"; } }
}
