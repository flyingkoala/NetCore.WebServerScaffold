using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Config
{

    public class ServerConfig
    {
        public ApiAuth apiauth { get; set; }

        public Redis redis { get; set; }

        public Mysql mysql { get; set; }
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


    public class Redis
    {
        /// <summary>
        /// redis地址链接
        /// </summary>
        public string conn { get; set; }
        /// <summary>
        /// redis中key值前缀
        /// </summary>
        public string keyprefix { get; set; }
        /// <summary>
        /// token防刷新有效期；单位：秒
        /// </summary>
        public int exp_token { get; set; }
        /// <summary>
        /// redis缓存时间；单位：分钟
        /// </summary>
        public int exp_cache { get; set; }
    }

    public class Mysql
    {
        /// <summary>
        /// mysql地址链接字符串
        /// </summary>
        public string connstring { get; set; }
    }
}
