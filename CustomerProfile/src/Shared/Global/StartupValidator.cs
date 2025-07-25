namespace src.Shared.Global
{
    public static class StartupValidator
    {
        public static string ConnectionString()
        {
            DotNetEnv.Env.Load();

            string? _dbName = Environment.GetEnvironmentVariable("DB_NAME");
            string? _dbUser = Environment.GetEnvironmentVariable("DB_USER");
            string? _dbHost = Environment.GetEnvironmentVariable("DB_HOST");
            string? _dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
            string? _dbPort = Environment.GetEnvironmentVariable("DB_PORT");

            //validate
            if (string.IsNullOrEmpty(_dbName))
                throw new ServiceException("DB_NAME environment variable is not set.");
            if (string.IsNullOrEmpty(_dbUser))
                throw new ServiceException("DB_USER environment variable is not set.");
            if (string.IsNullOrEmpty(_dbHost))
                throw new ServiceException("DB_HOST environment variable is not set.");
            if (string.IsNullOrEmpty(_dbPassword))
                throw new ServiceException("DB_PASSWORD environment variable is not set.");
            if (string.IsNullOrEmpty(_dbPort))
                throw new ServiceException("DB_PORT environment variable is not set.");

            return $"Server={_dbHost};Database={_dbName};User Id={_dbUser};Password={_dbPassword};Port={_dbPort};";
        }

    }
    internal class ServiceException(string message) : Exception(message);
}
