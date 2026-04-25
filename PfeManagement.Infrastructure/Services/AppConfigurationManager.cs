using Microsoft.Extensions.Configuration;

namespace PfeManagement.Infrastructure.Services
{
    public sealed class AppConfigurationManager
    {
        private static Lazy<AppConfigurationManager>? _lazyInstance;

        public string JwtSecret { get; }
        public int JwtAccessTokenExpMinutes { get; }
        public int JwtRefreshTokenExpDays { get; }

        public string EmailHost { get; }
        public int EmailPort { get; }
        public bool EmailUseSsl { get; }
        public string EmailUsername { get; }
        public string EmailPassword { get; }
        public string EmailFromEmail { get; }
        public string EmailFromName { get; }

        private AppConfigurationManager(IConfiguration configuration)
        {
            var jwt = configuration.GetSection("JwtSettings");
            var email = configuration.GetSection("EmailSettings");

            JwtSecret = jwt["Secret"] ?? throw new InvalidOperationException("JwtSettings:Secret manquant");
            JwtAccessTokenExpMinutes = int.Parse(jwt["AccessTokenExpirationMinutes"] ?? "60");
            JwtRefreshTokenExpDays = int.Parse(jwt["RefreshTokenExpirationDays"] ?? "7");

            EmailHost = email["Host"] ?? throw new InvalidOperationException("EmailSettings:Host manquant");
            EmailPort = int.Parse(email["Port"] ?? "587");
            EmailUseSsl = bool.Parse(email["UseSsl"] ?? "false");
            EmailUsername = email["Username"] ?? string.Empty;
            EmailPassword = email["Password"] ?? string.Empty;
            EmailFromEmail = email["FromEmail"] ?? string.Empty;
            EmailFromName = email["FromName"] ?? "PFE Management";
        }

        public static void Initialize(IConfiguration configuration)
        {
            if (_lazyInstance is not null)
                return;

            _lazyInstance = new Lazy<AppConfigurationManager>(
                () => new AppConfigurationManager(configuration));
        }

        public static AppConfigurationManager Instance
        {
            get
            {
                if (_lazyInstance is null)
                    throw new InvalidOperationException("AppConfigurationManager non initialisé");

                return _lazyInstance.Value;
            }
        }
    }
}