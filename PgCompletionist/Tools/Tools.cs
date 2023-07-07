namespace PgCompletionist;

using System.Diagnostics;
using PgObjects;

public static class Tools
{
    public static PgSkill GetSkill(string key)
    {
        Debug.Assert(key.Length > 1 && key[0] == 'S');

        return SkillObjects.Get(key.Substring(1));
    }

    public static PgAbility GetAbility(string key)
    {
        Debug.Assert(key.Length > 1 && key[0] == 'A');

        return AbilityObjects.Get(key.Substring(1));
    }

    public static PgItem GetItem(string key)
    {
        Debug.Assert(key.Length > 1 && key[0] == 'I');

        return ItemObjects.Get(key.Substring(1));
    }

    public static PgEffect GetEffect(string key)
    {
        Debug.Assert(key.Length > 1 && key[0] == 'E');

        return EffectObjects.Get(key.Substring(1));
    }

    public static PgNpc GetNpc(string key)
    {
        Debug.Assert(key.Length > 1 && key[0] == 'N');

        return NpcObjects.Get(key.Substring(1));
    }

    public static PgQuest GetQuest(string key)
    {
        Debug.Assert(key.Length > 1 && key[0] == 'Q');

        return QuestObjects.Get(key.Substring(1));
    }
}
