using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Reformat.Framework.Core.Aspects;
using Reformat.Framework.Core.Core;
using Reformat.Framework.Core.IOC.Attributes;
using Reformat.Framework.Core.IOC.Services;
using Reformat.Framework.Core.JWT.interfaces;
using Reformat.Framework.Core.MVC;
using Reformat.Framework.SqlSugar.Domain;

namespace Reformat.Framework.SqlSugar.Core;

[ApiController]
[Route("api/[controller]/[action]")]
[ExceptionHandle]
public abstract class SugarController<TService,TEntity> : BaseController,IUserSupport where TService : SugarService<TEntity> where TEntity : SugarEntity, new()
{
    [Autowired] public IUserService UserService { get; set; }
    
    [Autowired] public TService ServiceInstance { get; set; }
    
    protected SugarController(IocScoped iocScoped) : base(iocScoped)
    {
    }

    /// <summary>
    /// 通用: ID查询接口
    /// </summary>
    [HttpGet]
    public abstract ApiResult<TEntity> Entity([FromQuery] [Required] long id);



    /// <summary>
    /// 通用: 分頁查詢接口
    /// </summary>
    [HttpPost]
    public abstract Task<ApiResult<PageList<TEntity>>> Page([FromBody] SqlQuery<TEntity> query);


    /// <summary>
    /// 通用: 新增接口
    /// </summary>
    [HttpPut]
    public abstract ApiResult<string> Create([FromBody] [Required] List<TEntity> request);


    /// <summary>
    /// 通用: 更新接口
    /// </summary>
    [HttpPut]
    public abstract ApiResult<string> Update([FromBody] [Required] List<TEntity> request);


    /// <summary>
    /// 通用: 刪除接口
    /// </summary>
    [HttpDelete]
    public abstract ApiResult<string> Detele([FromBody] IdsVo deletedVo);
} 