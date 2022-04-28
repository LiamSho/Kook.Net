using System.Collections.Immutable;
using Model = KaiHeiLa.API.Game;

namespace KaiHeiLa.Rest;

public class RestGame : RestEntity<int>, IGame
{
    private ImmutableArray<string> _productNames;
    private ImmutableArray<string> _processNames;
    
    public string Name { get; private set; }
    public ActivityType Type { get; private set; }
    public string Options { get; private set; }
    public bool RequireAdminPrivilege { get; private set; }
    public string Icon { get; private set; }

    public IReadOnlyCollection<string> ProductNames => _productNames.ToReadOnlyCollection();
    public IReadOnlyCollection<string> ProcessNames => _processNames.ToReadOnlyCollection();
    
    internal RestGame(BaseKaiHeiLaClient kaiHeiLa, int id)
        : base(kaiHeiLa, id)
    {
    }
    
    internal static RestGame Create(BaseKaiHeiLaClient kaiHeiLa, Model model)
    {
        var entity = new RestGame(kaiHeiLa, model.Id);
        entity.Update(model);
        return entity;
    }

    internal void Update(Model model)
    {
        Name = model.Name;
        Type = model.Type;
        Options = model.Options;
        RequireAdminPrivilege = model.KmHookAdmin;
        Icon = model.Icon;
        _productNames = model.ProductNames.ToImmutableArray();
        _processNames = model.ProcessNames.ToImmutableArray();
    }

    public async Task<RestGame> ModifyAsync(Action<GameProperties> func, RequestOptions options = null)
    {
        return await GameHelper.ModifyAsync(this, KaiHeiLa, func, options).ConfigureAwait(false);
    }
    public async Task DeleteAsync(RequestOptions options = null)
    {
        await GameHelper.DeleteAsync(this, KaiHeiLa, options).ConfigureAwait(false);
    }
    
    /// <inheritdoc />
    async Task<IGame> IGame.ModifyAsync(Action<GameProperties> func, RequestOptions options) 
        => await ModifyAsync(func, options);
}