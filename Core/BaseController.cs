using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Reformat.Framework.Core.Aspects;
using Reformat.Framework.Core.Exceptions;
using Reformat.Framework.Core.IOC.Services;
using Reformat.Framework.Core.MVC;
using Reformat.Framework.SqlSugar.Domain;

namespace Reformat.Framework.SqlSugar.Core;

[ApiController]
[Route("api/[controller]/[action]")]
[ExceptionHandle]
public abstract class BaseController : ControllerBase
{
    public IocScoped iocScoped;
    
    public BaseController(IocScoped iocScoped)
    {
        iocScoped.Autowired(this);
        this.iocScoped = iocScoped;
    }
}

[ApiController]
[Route("api/[controller]/[action]")]
[ExceptionHandle]
public abstract class BaseController<T,TService> : BaseController where TService : BaseService<T> where T : BaseEntity, new()
{
    protected abstract TService GetServiceInstance();
    
    protected BaseController(IocScoped iocScoped) : base(iocScoped)
    {
    }
    
    /// <summary>
    /// 通用: ID查询接口
    /// </summary>
    [HttpGet]
    public virtual APIResponse<T> Entity([FromQuery] [Required] long id)
    {
        T result = GetServiceInstance().GetById(id,true);
        return Api.Rest(result != null,result);
    }
    
    /// <summary>
    /// 通用: 列表查询接口
    /// </summary>
    // [HttpGet]
    // public virtual APIResponse<List<T>> List([FromBody] [Required] long[] ids)
    // {
    //     List<T> records = GetServiceInstance().GetByIds(ids);
    //     return Api.Rest(records.Count == ids.Length,records);
    // }
    
    /// <summary>
    /// 通用: 分頁查詢接口
    /// </summary>
    [HttpPost]
    public virtual async Task<APIResponse<PageList<T>>> Page([FromBody] SqlQuery<T> query)
    {
        return Api.RestSuccess( await GetServiceInstance().GetPage(query));
    }
    
    /// <summary>
    /// 通用: 新增接口
    /// </summary>
    [HttpPut]
    public virtual APIResponse<string> Create([FromBody] [Required] List<T> request)
    {
        int count = GetServiceInstance().SaveBatch(request);
        if (count != request.Count)
        {
            throw new BusinessException("新增失败","失败条目数：" + (request.Count - count));
        }
        return Api.RestSuccess();
    }
    
    /// <summary>
    /// 通用: 更新接口
    /// </summary>
    [HttpPut]
    public virtual APIResponse<string> Update([FromBody] [Required] List<T> request)
    {
        bool result = GetServiceInstance().UpdateBatch(request);
        if (!result)throw new BusinessException("更新失败");
        return Api.RestSuccess();
    }
    
    /// <summary>
    /// 通用: 刪除接口
    /// </summary>
    [HttpDelete]
    public virtual APIResponse<string> Detele([FromBody] [Required] long[] ids)
    {
        bool result = GetServiceInstance().DeleteByIds(ids);
        if (!result)throw new BusinessException("删除失败");
        return Api.RestSuccess();
    }
} 