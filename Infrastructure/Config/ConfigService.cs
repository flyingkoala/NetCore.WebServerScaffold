using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Options;

namespace Infrastructure.Config
{
    public class ConfigService
    {
        private readonly IOptions<Config> _configuration;
        public ConfigService(IOptions<Config> configuration)
        {
            _configuration = configuration;

        }

        public Config config
        {
            get
            {
                return _configuration.Value;
            }
        }
    }
}
