namespace Selectors;

public class NextOrLastNonMaxedSkillTemplateSelector : NextOrLastTemplateSelector
{
    protected override string NextTemplateName { get { return "NextNonMaxedSkillTemplate"; } }
    protected override string LastTemplateName { get { return "LastNonMaxedSkillTemplate"; } }
}
