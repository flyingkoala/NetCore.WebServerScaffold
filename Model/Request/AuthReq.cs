using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Model.Request
{
    public class AuthReq_login
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required]
        public string name { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        [Required]
        public string pass { get; set; }
    }

    public class AuthReq_RefreshToken
    {
        /// <summary>
        /// 名称(heygears)
        /// </summary>
        [Required]
        public string refresh_token { get; set; }
    }
}
