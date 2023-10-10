using System.Collections.Generic;

public class FingeringData
{
    public string LastSelectedSet { get; set; }
    public Dictionary<string, SetData> Sets { get; set; } = new();
}

public class SetData
{
    public Dictionary<string, int> Combinations { get; set; } = new();
    public Dictionary<string, int> Alterations { get; set; } = new();
}

