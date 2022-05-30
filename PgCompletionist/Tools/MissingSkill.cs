﻿namespace PgCompletionist;

public class MissingSkill
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int IconId { get; set; }

    public override string ToString()
    {
        return Name;
    }
}