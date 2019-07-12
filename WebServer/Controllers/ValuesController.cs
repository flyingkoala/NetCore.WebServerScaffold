using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Log;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [Produces("application/json")]
    [Route("value")]
    [EnableCors("CorsPolicy")]
    public class ValuesController : ControllerBase
    {
        public ValuesController()
        {

        }

        /// <summary>
        /// 测试一下
        /// </summary>
        /// <remarks>
        /// 例子: Get api/Values/1
        /// </remarks>
        /// <returns></returns>
        [HttpGet("test1")]
        //[AllowAnonymous]
        public ActionResult<IEnumerable<string>> Get()
        {
            LogHelper.Log.Debug("测试日志功能");
            return new string[] { "value1", "value2" };
        }
    }
}
