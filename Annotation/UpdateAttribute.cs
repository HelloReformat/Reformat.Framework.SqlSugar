using SqlSugar;

namespace Reformat.Framework.SqlSugar.Annotation;

/// <summary>
/// 修改
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class UpdateAttribute : Attribute
{
    /// <summary>
    /// 默认值
    /// </summary>
    /// <remarks>
    /// 可以用$$$now表示当前时间
    /// $$$set{xxx}把当前用
    /// 
    /// </remarks>
    public object defvalue { get; set; }

    /// <summary>
    /// 步骤
    /// </summary>
    public string step { get; set; } = "";

    public bool miss { get; set; }

    /// <summary>
    /// 是否忽略接口输入的值
    /// </summary>
    // todo 新增特性 是否忽略接口输入的值
    public bool ignore { get; set; }

    /// <summary>
    /// 是否更新前检查
    /// </summary>
    public bool IsUpCheck { set; get; }

    /// <summary>
    /// 关系
    /// </summary>
    public ConditionalType type { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string depict { set; get; }
}