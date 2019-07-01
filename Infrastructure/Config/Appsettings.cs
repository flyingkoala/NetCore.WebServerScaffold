using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Config
{

    public class Config
    {
        public ApiAuth apiauth { get; set; }
    }

    public class ApiAuth
    {
        /// <summary>
        /// auth秘钥
        /// </summary>
        public  string securitykey { get; set; }
        /// <summary>
        /// token有效时间：单位min
        /// </summary>
        public int exp_auth { get; set; }
        /// <summary>
        /// refresh_token有效时间：单位min
        /// </summary>
        public int exp_auth_refresh { get; set; }
    }
}
