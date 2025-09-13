namespace PaperTrail.Core.Services;

/// <summary>
/// Thrown when a Pro-gated feature is invoked without a valid license.
/// </summary>
public sealed class LicensingException : Exception
{
    public LicensingException() { }
    public LicensingException(string message) : base(message) { }
    public LicensingException(string message, Exception inner) : base(message, inner) { }
}

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
