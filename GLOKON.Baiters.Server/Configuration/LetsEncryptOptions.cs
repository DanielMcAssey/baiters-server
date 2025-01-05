namespace GLOKON.Baiters.Server.Configuration
{
    public class LetsEncryptOptions
    {
        public bool UseStagingServer { get; set; } = false;

        public IList<string> Domains { get; set; } = [];

        public string EmailAddress { get; set; } = string.Empty;

        public bool IsEnabled()
        {
            return Domains?.Count > 0 && !string.IsNullOrEmpty(EmailAddress);
        }
    }
}
