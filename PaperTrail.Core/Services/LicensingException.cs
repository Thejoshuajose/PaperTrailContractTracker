namespace PaperTrail.Core.Services;

/// <summary>
/// Exception thrown when a feature is accessed that requires a Pro licence.
/// </summary>
public class LicensingException : Exception
{
    public LicensingException(string? message = null) : base(message) { }
}
