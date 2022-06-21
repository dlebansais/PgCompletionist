namespace Selectors;

public class NextOrLastMissingAbilitesBySkillTemplateSelector : NextOrLastTemplateSelector
{
    protected override string NextTemplateName { get { return "NextMissingAbilitiesTemplate"; } }
    protected override string LastTemplateName { get { return "LastMissingAbilitiesTemplate"; } }
}
