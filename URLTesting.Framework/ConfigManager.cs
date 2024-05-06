using Microsoft.Extensions.Configuration;

namespace URLTesting.Framework
{
    public static class ConfigManager
    {
        public static TestConfig TestConfig { get; set; }

        static ConfigManager()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", true, false)
                .Build();

            TestConfig = configuration.GetSection("Config").Get<TestConfig>() ?? new TestConfig();
        }
    }
}
