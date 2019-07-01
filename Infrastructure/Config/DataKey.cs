using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Config
{
    public class DataKey
    {
        //JWT配置
        public const string JWT_ValidIssuer = "service.user";//Issuer，这两项和前面签发jwt的设置一致
        public const string JWT_ValidAudience = "Scaffold.user";//Audience
        public const string JWT_Name = "webserver";
        public const string JWT_Password = "123456";
    }
}
