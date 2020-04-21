using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HangFire.Demo1.Models.commom
{
    public class ConfigUtil
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<ConfigUtil> logger;

        public ConfigUtil(IConfiguration configuration,ILogger<ConfigUtil> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        public string App_key 
        {
            get 
            {
                string key = "";
                try
                {
                    key = configuration["App_key"].ToString();
                }
                catch (Exception ex) 
                {
                    logger.LogError($"configuration:{nameof(App_key)} is not found");
                }
                return key;
            }
        }

        public string App_secret 
        {
            get 
            {
                string secret = "";
                try
                {
                    secret = configuration["App_secret"].ToString();
                }
                catch (Exception ex)
                {
                    logger.LogError($"configuration:{nameof(secret)} is not found");
                }
                return secret; 
            }
        }
        public string IposApiUrl 
        {
            get 
            {
                string Url = "";
                try
                {
                    Url = configuration["IposApiUrl"].ToString();
                }
                catch (Exception ex)
                {
                    logger.LogError($"configuration:{nameof(IposApiUrl)} is not found");
                }
                return Url; 
            }
        }
    }
}
