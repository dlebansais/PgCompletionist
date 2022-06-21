namespace Selectors;

public class NextOrLastMissingSkillTemplateSelector : NextOrLastTemplateSelector
{
    protected override string NextTemplateName { get { return "NextMissingSkillTemplate"; } }
    protected override string LastTemplateName { get { return "LastMissingSkillTemplate"; } }
}
