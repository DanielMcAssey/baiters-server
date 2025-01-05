namespace GLOKON.Baiters.Server.Configuration
{
    public class SslOptions
    {
        public string? CertificatePath { get; set; } = null;

        public string? CertificatePassword { get; set; } = null;

        public bool IsEnabled()
        {
            return !string.IsNullOrEmpty(CertificatePath);
        }
    }
}
