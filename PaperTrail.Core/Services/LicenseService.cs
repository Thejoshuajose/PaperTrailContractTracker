namespace PaperTrail.Core.Services;

public interface ILicenseService
{
    bool IsPro { get; }
    void Load(string licenseKey);
}

public class LicenseService : ILicenseService
{
    public bool IsPro { get; private set; }

    public void Load(string licenseKey)
    {
        IsPro = !string.IsNullOrWhiteSpace(licenseKey);
    }
}
