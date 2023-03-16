namespace Kook;

public struct PokeQuality : IEntity<uint>
{
    public uint Id { get; internal set; }

    public Color Color { get; internal set; }

    public IReadOnlyDictionary<string, string> Resources { get; internal set; }

    internal PokeQuality(uint id, Color color, IReadOnlyDictionary<string, string> resources)
    {
        Id = id;
        Color = color;
        Resources = resources;
    }

    internal static PokeQuality Create(uint id, Color color, IReadOnlyDictionary<string, string> resources)
        => new PokeQuality(id, color, resources);
}
