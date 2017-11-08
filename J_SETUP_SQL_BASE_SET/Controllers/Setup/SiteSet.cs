namespace J_SETUP_SQL.Controllers.Setup
{
    public class SiteSet
    {
        public string Database { get; set; }
        public string Databasename { get; set; }
        public string Databaseid { get; set; }
        public string Databasepassword { get; set; }
        public string Sitename { get; set; }
        public string Userid { get; set; }
        public string Userpassword { get; set; }
        public string Useremail { get; set; }
    }
    public class AppsettingsRead
    {
        public ConnectionStrings ConnectionStrings { get; set; } 
        public Logging Logging { get; set; }
    }
    public class ConnectionStrings
    {
        public string DefaultConnection { get; set; }
    }
    public class Logging
    {
        public bool IncludeScopes { get; set; }
        public Debug Debug { get; set; }
        public Console Console { get; set; }
    }

    public class LogLevel2
    {
        public string Default { get; set; }
    }

    public class Console
    {
        public LogLevel2 LogLevel { get; set; }
    }
    public class LogLevel
    {
        public string Default { get; set; }
    }
    public class Debug
    {
        public LogLevel LogLevel { get; set; }
    }
}