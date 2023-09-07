namespace Reformat.Framework.SqlSugar.Domain;

public class SqlQuery<T> 
{
    /// <summary>
    /// 查询条件
    /// </summary>
    public T QueryParms { get; set; }
    
    /// <summary>
    /// 分页大小 0的时候返回所有 (默认：20)
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 第几页（默认：1）
    /// </summary>
    public int PageIndex { get; set; } = 1;
    
    /// <summary>
    /// 排序 字段名1|A,字段名2|D A正序,D倒序
    /// </summary>
    public string OrderCondition { get; set; }
}