namespace Reformat.Framework.SqlSugar.Domain;

public class PageList<T>
{
    public int Total { get; set; }
    public List<T> List { get; set; } = new List<T>();
}