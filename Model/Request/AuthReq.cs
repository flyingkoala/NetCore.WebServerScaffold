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
}
