using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Response
{
    public class AuthRes_login
    {
        /// <summary>
        /// 授权成功以后返回的授权token；时效：2分钟；
        /// 使用方法：
        /// 在请求的Headers中加入参数Authorization=Bearer {token}
        /// </summary>
        public string token { get; set; }
        /// <summary>
        /// refresh_token 用于获取重新获取token；时效：2小时；
        /// </summary>
        public string refresh_token { get; set; }
    }

    public class AuthRes_RefreshToken
    {
        /// <summary>
        /// 授权成功以后返回的授权token；时效：2分钟；
        /// 使用方法：
        /// 在请求的Headers中加入参数Authorization=Bearer {token}
        /// </summary>
        public string token { get; set; }
    }

}
