using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Options;

namespace Infrastructure.Config
{
    public class ConfigService
    {
        private readonly IOptions<ServerConfig> _configuration;
        public ConfigService(IOptions<ServerConfig> configuration)
        {
            _configuration = configuration;

        }

        public ServerConfig config
        {
            get
            {
                return _configuration.Value;
            }
        }
    }
}
