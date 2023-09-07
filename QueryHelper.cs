using System.Reflection;
using Reformat.Framework.Core.Common.Extensions.lang;
using Reformat.Framework.SqlSugar.Annotation;
using Reformat.Framework.SqlSugar.Domain;
using SqlSugar;

namespace Reformat.Framework.SqlSugar;

public class QueryHelper
{
    /// <summary>
    /// 转换排序规则
    /// </summary>
    /// <param name="order"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string GetOrderString<T>(string order) where T : class, new()
    {
        if (string.IsNullOrEmpty(order)) return "";

        var _s = order.Split(',');
        string orderstr = "";
        foreach (var _str in _s)
        {
            if (!string.IsNullOrEmpty(_str))
            {
                var _c = _str.Split('|');
                string key = _c[0];
                string asc = "desc";
                if (_c.Length > 1)
                {
                    if (_c[1].ToLower() == "a")
                    {
                        asc = "asc";
                    }
                    else
                    {
                        asc = "desc";
                    }
                }
                var _t = typeof(T);
                if (_t.GetProperties().Count(T => T.Name.Trim() == key.Trim()) > 0)
                {
                    orderstr += $"{key} {asc},";
                }
            }
        }
        return orderstr.Trim(',');
    }
    
    /// <summary>
    /// 生成查询where部份
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="BlackList"></param>
    /// <returns></returns>
    public async static Task<List<IConditionalModel>> GetWhere<T>(SqlQuery<T> SQLQuery, string BlackList = "")
    {
        var query = SQLQuery.QueryParms;

        if (query == null) return new List<IConditionalModel>();
        var _type = query.GetType();
        var _defaultvalue = _type.GetConstructor(Type.EmptyTypes).Invoke(null);
        var _diff = query.GetDiffentMap(_defaultvalue);
        List<IConditionalModel> conModels = new List<IConditionalModel>();
        var _list = new List<KeyValuePair<WhereType, ConditionalModel>>();

        var _plist = _type.GetProperties();
        foreach (var _p in _plist)
        {
            // 20230717: 支持Nullable对象的赋值
            dynamic? _v = _p.GetValue(query);
            if (_v != null)
            {
                var _vtype = _v.GetType();
                
                ///差过默认值
                if (_p.PropertyType == typeof(DateTime) || _p.PropertyType == typeof(DateTime?))
                {
                    if (-5 < (DateTime.Now - (DateTime)_v).TotalSeconds &&
                        (DateTime.Now - (DateTime)_v).TotalSeconds < 6)
                    {
                        _v = null;
                        continue;
                    }
                }

                string value = "";

                if (_vtype.BaseType.Name.ToLower() == "enum")
                {
                    value = _v.ToString("D");
                }
                else
                {
                    value = _v.ToString();
                }

                //if (_p.PropertyType.Name.ToLower().Contains("bool"))
                if (_vtype.Name == "Boolean")
                {
                    if (_v)
                    {
                        value = "1";
                    }
                    else
                    {
                        value = "0";
                    }
                }

                var _sqlsugar = (SugarColumn)_p.GetCustomAttribute(typeof(SugarColumn));
                bool _sqlIsIgnore = false;
                if (_sqlsugar != null) _sqlIsIgnore = _sqlsugar.IsIgnore;
                var _where = (WhereAttribute)_p.GetCustomAttribute(typeof(WhereAttribute));


                bool isdef = false;
                //_where == null ? false : _where.defValue == value;
                if (_where != null && _where.defValue.NotIsNullOrEmpty())
                {
                    isdef = _where.defValue == value;
                }
                else
                {
                    isdef = _diff.Count(T => T.Key == _p.Name) == 0;
                }

                if (_sqlIsIgnore || isdef || BlackList.Contains(_p.Name))
                {
                    continue;
                }

                //if (_where != null && _where.rule.NotIsNullOrEmpty())
                //{
                //    // var _rule = httpContext.RequestServices.GetService(typeof(Rule_Engine)) as Rule_Engine;
                //    ///  有点复杂这里，想好再来，绝大部份可以在loginAuTOSetVALUE来搞定
                //}
                if (_where != null && _where.isMiss)
                {
                    continue;
                }

                string _key = _where == null ? _p.Name : string.IsNullOrEmpty(_where.key) ? _p.Name : _where.key;

                // 20230711: 修复sqlsugar的ColumnName属性无效的问题
                if (_sqlsugar != null && _sqlsugar.ColumnName.NotIsNullOrEmpty())
                {
                    _key = _sqlsugar.ColumnName;
                }

                object _secvod = new object();
                if (_where != null && _where.seckey.NotIsNullOrEmpty())
                {
                    var _secp = _plist.First(T => T.Name == _where.seckey);
                    if (_secp != null)
                    {
                        _secvod = _secp.GetValue(query);
                    }

                    if (_secp.PropertyType == typeof(DateTime) || _secp.PropertyType == typeof(DateTime?))
                    {
                        value = DateTime.Parse(value).ToString("yyyy-MM-dd HH:mm:ss");
                        _secvod = DateTime.Parse(_secvod.ToStringMissNull("")).ToString("yyyy-MM-dd HH:mm:ss");
                    }

                    var _sek = new ConditionalCollections()
                    {
                        ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                        {
                            new KeyValuePair<WhereType, ConditionalModel>(
                                _where == null ? WhereType.And : _where.wheretype,
                                new ConditionalModel()
                                {
                                    FieldName = _key, ConditionalType = ConditionalType.GreaterThanOrEqual,
                                    FieldValue = value
                                }),
                            new KeyValuePair<WhereType, ConditionalModel>(WhereType.And,
                                new ConditionalModel()
                                {
                                    FieldName = _key, ConditionalType = ConditionalType.LessThanOrEqual,
                                    FieldValue = _secvod.ToStringMissNull("")
                                })
                        }
                    };
                    conModels.Add(_sek);
                }
                else
                {
                    _list.Add(new KeyValuePair<WhereType, ConditionalModel>(
                        _where == null ? WhereType.And : _where.wheretype,
                        new ConditionalModel()
                        {
                            FieldName = _key,
                            ConditionalType = _where == null ? ConditionalType.Equal : _where.type,
                            FieldValue = value
                        }));
                }
            }
        }
        conModels.Add(new ConditionalCollections() { ConditionalList = _list });
        return conModels;
    }
}