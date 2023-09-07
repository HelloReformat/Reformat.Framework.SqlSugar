using SqlSugar;

namespace Reformat.Framework.SqlSugar.Annotation;

/// <summary>
/// 查询
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class WhereAttribute : Attribute
{
    /// <summary>
    /// Key空为当前字段名
    ///
    /// </summary>
    public string key { get; set; }
    /// <summary>
    /// 关系
    /// </summary>
    public ConditionalType type { get; set; }

    /// <summary>
    /// 分组名
    /// </summary>
    public string groupname { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    public string defValue { get; set; }

    /// <summary>
    /// 软删除
    /// </summary>
    public bool isSoftDelete { get; set; }
    
    /// <summary>
    /// 软删除的表达式
    /// </summary>
    public string softDeleteValue { set; get; }
    
    /// <summary>
    /// 条件间的关系
    /// </summary>
    public WhereType wheretype { get; set; } = WhereType.And;

    /// <summary>
    /// 搜索时不加入 默认false搜索
    /// </summary>
    public bool isMiss { set; get; } = false;
    /// <summary>
    /// 区间值绑定的KEY-区间查询
    /// </summary>
    public string seckey { get; set; }
}