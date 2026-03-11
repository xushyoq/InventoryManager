namespace InventoryManager.Models;

public class InventoryStatistics
{
    public int ItemsCount { get; set; }
    public List<NumericFieldStats> NumericStats { get; set; } = new();
    public List<StringFieldStats> StringStats { get; set; } = new();
}

public class NumericFieldStats
{
    public string Name { get; set; } = string.Empty;
    public double Avg { get; set; }
    public int Min { get; set; }
    public int Max { get; set; }
}

public class StringFieldStats
{
    public string Name { get; set; } = string.Empty;
    public List<StringCount> Top { get; set; } = new();
}

public class StringCount
{
    public string Value { get; set; } = string.Empty;
    public int Count { get; set; }
}
