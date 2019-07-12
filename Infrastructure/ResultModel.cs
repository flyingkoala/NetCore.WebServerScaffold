using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Infrastructure
{
    public class ResultModel
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool success { get; set; } = false;
        /// <summary>
        /// 响应码
        /// </summary>
        public int code { get; set; } =(int)ResultCode.ServiceInternalAbnormal;

        public string msg { get; set; }

        public object data { get; set; }

        /// <summary>
        /// 响应成功
        /// </summary>
        /// <param name="data"></param>
        /// <param name="msg"></param>
        public void Success(object data,string msg)
        {
            success = true;
            this.code = (int)ResultCode.OK;
            this.data = data;
            this.msg = msg;
        }
        /// <summary>
        /// 响应失败
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        public void Failure(string msg, int code = (int)ResultCode.ServiceInternalAbnormal)
        {
            success = false;
            this.code = code;
            this.msg = msg;
        }

    }

    public enum ResultCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        [Description("成功")]
        OK = 200,

        /// <summary>
        /// 未授权
        /// </summary>
        [Description("未授权")]
        Unauthorized = 401,

        /// <summary>
        /// 接口不存在
        /// </summary>
        [Description("接口不存在")]
        NotFound = 404,

        /// <summary>
        /// 服务内部异常
        /// </summary>
        [Description("服务内部异常")]
        ServiceInternalAbnormal = 500,

        /// <summary>
        /// 请求参数效验失败
        /// </summary>
        [Description("请求参数效验失败")]
        ArgumentVerifyFail = 600,

        /// <summary>
        /// TOKEN效验失败
        /// </summary>
        [Description("TOKEN效验失败")]
        TokenVerifyFail = 700,

        /// <summary>
        /// 用户不存在
        /// </summary>
        [Description("用户不存在")]
        User_IsNotExist = 11011,





    }
}
