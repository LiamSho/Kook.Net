namespace Kook;

public interface IVoiceRegion
{
    /// <summary>
    ///     Gets the unique identifier for this voice region.
    /// </summary>
    /// <returns>
    ///     A string that represents the identifier for this voice region (e.g. <c>eu-central</c>).
    /// </returns>
    string Id { get; }
    /// <summary>
    ///     Gets the name of this voice region.
    /// </summary>
    /// <returns>
    ///     A string that represents the human-readable name of this voice region (e.g. <c>Central Europe</c>).
    /// </returns>
    string Name { get; }
    /// <summary>
    ///     Gets the crowding of this voice region.
    /// </summary>
    /// <returns>
    ///     A decimal between <c>0.0</c> and <c>1.0</c> that represents the crowding of this voice region.
    /// </returns>
    decimal Crowding { get; }
}