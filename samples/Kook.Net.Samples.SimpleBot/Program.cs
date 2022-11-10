﻿// See https://aka.ms/new-console-template for more information

using System.Collections.Immutable;
using Kook;
using Kook.Rest;
using Kook.WebSocket;

class Program
{
    private readonly KookSocketClient _client;
    private readonly string _token;
    private readonly ulong _guildId;
    private readonly ulong _channelId;
    public static Task Main(string[] args) => new Program().MainAsync();

    public Program()
    {
        _token = Environment.GetEnvironmentVariable("KookDebugToken", EnvironmentVariableTarget.User)
                 ?? throw new ArgumentNullException(nameof(_token));
        _guildId = ulong.Parse(Environment.GetEnvironmentVariable("KookDebugGuild", EnvironmentVariableTarget.User)
                               ?? throw new ArgumentNullException(nameof(_token)));
        _channelId = ulong.Parse(Environment.GetEnvironmentVariable("KookDebugChannel", EnvironmentVariableTarget.User)
                                 ?? throw new ArgumentNullException(nameof(_token)));
        _client = new(new KookSocketConfig()
        {
            AlwaysDownloadUsers = true,
            AlwaysDownloadVoiceStates = true,
            AlwaysDownloadBoostSubscriptions = true,
            MessageCacheSize = 100,
            LogLevel = LogSeverity.Debug
        });

        _client.Log += ClientOnLog;
        _client.GuildMemberOnline += ClientOnGuildMemberOnline;
        // _client.MessageReceived += ClientOnMessageReceived;
        // _client.DirectMessageReceived += ClientOnDirectMessageReceived;
        _client.Ready += ClientOnReady;
        _client.EmoteCreated += (emote, guild) => Task.CompletedTask;
        _client.EmoteDeleted += (emote, guild) => Task.CompletedTask;
        _client.EmoteUpdated += (before, after, guild) => Task.CompletedTask;
        // _client.MessageButtonClicked += ClientOnMessageButtonClicked;
        // _client.MessageDeleted += async (msg, channel) =>
        // {
        //     Console.WriteLine($"Message {(await msg.GetOrDownloadAsync()).CleanContent} deleted in {(await channel.GetOrDownloadAsync()).Name}");
        // };
        // _client.MessageButtonClicked += async (value, user, message, channel, guild) =>
        // {
        //     
        // };
    }

    private Task ClientOnGuildMemberOnline(IReadOnlyCollection<SocketGuildUser> arg1, DateTimeOffset arg2)
    {
        return Task.CompletedTask;
    }
    //
    // private Task ClientOnDirectMessageReceived(SocketMessage arg)
    // {
    //     return Task.CompletedTask;
    // }

    public async Task MainAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }

    // private async Task ClientOnMessageReceived(SocketMessage arg)
    // {
    //     Console.WriteLine($"{arg.Author.Username} in {arg.Channel.Name}: {arg.Content}");
    //     string argCleanContent = arg.CleanContent;
    //     if (arg.Author.Id == _client.CurrentUser.Id) return;
    //     if (arg.Author.IsBot == true) return;
    //     if (arg.Content != "/test") return;
    //     await arg.Channel.SendTextAsync("收到了！", quote: new Quote(arg.Id));
    //     // await msg.UpdateAsync();
    //     // await CardDemo(arg);
    //     await ModifyMessageDemo(arg);
    // }
    //
    private async Task ClientOnReady()
    {
        await Task.Delay(TimeSpan.FromSeconds(5));
        // KookSocketClient kookSocketClient = _client;
        // SocketGuildUser socketGuildUser = kookSocketClient.GetGuild(7557797319758285).GetUser(821393881);
        // List<SocketGuildUser> socketGuildUsers = kookSocketClient.GetGuild(7557797319758285).Users.Where(x => x.IsDeafened == true).ToList();
        // IReadOnlyCollection<SocketGuildUser> readOnlyCollection = kookSocketClient.GetGuild(7557797319758285).GetVoiceChannel(9816956151862920).ConnectedUsers;
        // IReadOnlyCollection<SocketGuildUser> connectedUsers = await kookSocketClient.GetGuild(7557797319758285).GetVoiceChannel(9816956151862920).GetConnectedUsersAsync();
        // Stream stream = await new HttpClient().GetStreamAsync("https://img.kaiheila.cn/attachments/2021-01/21/600975671b9ab.mp3");
        // await _client.GetUser(2810246202).SendFileAsync(new FileAttachment(stream, "Filename", AttachmentType.Audio));
        // Cacheable<IUserMessage,Guid> message = await (await _client.GetUserAsync(2810246202)).SendTextAsync("1234567");
        // IUserMessage msg = await message.GetOrDownloadAsync();
        // (Guid messageId, DateTimeOffset messageTimestamp) = await _client.GetGuild(7557797319758285).GetTextChannel(7888175654136995)
            // .SendFileAsync(new FileAttachment(stream, "Filename", AttachmentType.Audio));
        // (Guid messageId, DateTimeOffset messageTimestamp) = await _client.GetGuild(7557797319758285)
        //     .GetTextChannel(7888175654136995)
        //     .SendVideoMessageAsync("D:\\1.mp4");
        // IMessage message = await _client.GetGuild(7557797319758285).GetTextChannel(7888175654136995)
        //     .GetMessageAsync(Guid.Parse("dad3df69-af9e-4bb9-8b8c-c2ca2239685e"));
        // SocketDMChannel socketDMChannel = await _client.GetUser(2810246202).CreateDMChannelAsync();
        // IMessage messageAsync = await socketDMChannel.GetMessageAsync(Guid.Parse("0f2b0e9f-82ea-4c30-b91d-7956b10fbd29"));
        IMessage listAsync = await _client.GetGuild(7557797319758285).GetTextChannel(7888175654136995).GetMessageAsync(Guid.Parse("617573e1-edb5-43c7-93d1-3c6430078c6b"));
        // IDMChannel socketDMChannel = await (await _client.GetUserAsync(2810246202)).CreateDMChannelAsync();
        // List<IReadOnlyCollection<IMessage>> async = await socketDMChannel.GetMessagesAsync().ToListAsync();
    }

    // {
    //     // await _client.Rest.AddReactionAsync(Guid.Parse("9062d5a9-9290-434c-b295-5b5835121cb1"), Emote.Parse("(emj)loading(emj)[1990044438283387/WiGtuv3F1d05k05k]", TagMode.KMarkdown));
    //     // await Task.Delay(TimeSpan.FromSeconds(5));
    //     // IUser result = await _client.GetUserAsync(2810246202);
    //     // IUser userAsync = await _client.GetUserAsync(1896684851);
    //     return Task.CompletedTask;
    // }
    //
    private async Task ClientOnLog(LogMessage arg)
    {
        await Task.Delay(0);
        Console.WriteLine(arg.ToString());
    }
    //
    // private async Task ClientOnMessageButtonClicked(string value, SocketUser user, IMessage msg, SocketTextChannel channel, SocketGuild guild)
    // {
    //     // await msg.AddReactionAsync(Emote.Parse("[:djbigfan:1990044438283387/hvBcVC4nHX03k03k]", TagMode.PlainText));
    //     // await msg.AddReactionAsync(Emote.Parse("(emj)djbigfan(emj)[1990044438283387/hvBcVC4nHX03k03k]", TagMode.KMarkdown));
    //     // await msg.RemoveReactionAsync(Emote.Parse("[:djbigfan:1990044438283387/hvBcVC4nHX03k03k]", TagMode.PlainText), _client.CurrentUser);
    //     // IEnumerable<IMessage> selectMany = (await _client.GetGuild(1990044438283387).GetTextChannel(6286033651700207).GetMessagesAsync(Guid.Parse("ed260ee9-1616-44ec-abff-d5cfcf9903a0"), Direction.Around, 5).ToListAsync()).SelectMany(x => x.ToList());
    //     // await _client.GetUserAsync(0);
    //     // IReadOnlyCollection<RestMessage> pinnedMessagesAsync = await _client.GetGuild(1990044438283387).GetTextChannel(6286033651700207).GetPinnedMessagesAsync();
    //     // await (user as SocketGuildUser).AddRoleAsync(1681537);
    //     // IEnumerable<IGuildUser> flattenAsync = await _client.GetGuild(1990044438283387).GetRole(300643).GetUsersAsync().FlattenAsync().ConfigureAwait(false);
    //     // IReadOnlyCollection<IInvite> readOnlyCollection = await _client.GetGuild(1990044438283387).GetInvitesAsync();
    //     // IInvite invite = await _client.GetGuild(1990044438283387).CreateInviteAsync(InviteMaxAge.NeverExpires, InviteMaxUses.Fifty);
    //     // SocketGuild socketGuild = _client.GetGuild(1990044438283387);
    //     // IEnumerable<RestGame> games = await _client.Rest.GetGamesAsync().FlattenAsync().ConfigureAwait(false);
    //     await _client.GetGuild(1990044438283387).GetTextChannel(6286033651700207)
    //         .SendImageMessageAsync("E:\\OneDrive\\Pictures\\86840227_p0_新年.png");
    // }
    //
    // private async Task CardDemo(SocketMessage message)
    // {
    //     if (message.Author.Id == _client.CurrentUser.Id) return;
    //     if (message.Content != "/test") return;
    //     CardBuilder cardBuilder = new CardBuilder()
    //         .WithSize(CardSize.Large)
    //         .AddModule(new HeaderModuleBuilder().WithText("This is header"))
    //         .AddModule(new DividerModuleBuilder())
    //         .AddModule(new SectionModuleBuilder().WithText("**This** *is* ~~kmarkdown~~", true))
    //         .AddModule(new SectionModuleBuilder()
    //             .WithText(new ParagraphStructBuilder()
    //                 .WithColumnCount(2)
    //                 .AddField(new PlainTextElementBuilder().WithContent("多列文本测试"))
    //                 .AddField(new KMarkdownElementBuilder().WithContent("**昵称**\n白给Doc"))
    //                 .AddField(new KMarkdownElementBuilder().WithContent("**在线时间**\n9:00-21:00"))
    //                 .AddField(new KMarkdownElementBuilder().WithContent("**服务器**\n吐槽中心")))
    //             .WithAccessory(new ImageElementBuilder()
    //                 .WithSource("https://img.kaiheila.cn/assets/2021-01/7kr4FkWpLV0ku0ku.jpeg")
    //                 .WithSize(ImageSize.Small))
    //             .WithMode(SectionAccessoryMode.Unspecified))
    //         .AddModule(new SectionModuleBuilder()
    //             .WithText(new PlainTextElementBuilder().WithContent("您是否认为\"KOOK\"是最好的语音软件？"))
    //             .WithAccessory(new ButtonElementBuilder().WithTheme(ButtonTheme.Primary).WithText("完全同意", false))
    //             .WithMode(SectionAccessoryMode.Right))
    //         .AddModule(new ContainerModuleBuilder()
    //             .AddElement(new ImageElementBuilder().WithSource("https://img.kaiheila.cn/assets/2021-01/7kr4FkWpLV0ku0ku.jpeg")))
    //         .AddModule(new ImageGroupModuleBuilder()
    //             .AddElement(new ImageElementBuilder().WithSource("https://img.kaiheila.cn/assets/2021-01/pWsmcLsPJq08c08c.jpeg"))
    //             .AddElement(new ImageElementBuilder().WithSource("https://img.kaiheila.cn/assets/2021-01/YIfHfnvxaV0dw0dw.jpg")))
    //         .AddModule(new ContextModuleBuilder()
    //             .AddElement(new PlainTextElementBuilder().WithContent("KOOK 气氛组，等你一起来搞事情"))
    //             .AddElement(new ImageElementBuilder().WithSource("https://img.kaiheila.cn/assets/2021-01/7kr4FkWpLV0ku0ku.jpeg"))
    //             .AddElement(new ImageElementBuilder().WithSource("https://img.kaiheila.cn/assets/2021-01/7kr4FkWpLV0ku0ku.jpeg"))
    //             .AddElement(new ImageElementBuilder().WithSource("https://img.kaiheila.cn/assets/2021-01/7kr4FkWpLV0ku0ku.jpeg")))
    //         .AddModule(new ActionGroupModuleBuilder()
    //             .AddElement(new ButtonElementBuilder().WithTheme(ButtonTheme.Primary).WithText("确定").WithValue("ok"))
    //             .AddElement(new ButtonElementBuilder().WithTheme(ButtonTheme.Danger).WithText("取消").WithValue("cancel")))
    //         .AddModule(new FileModuleBuilder()
    //             .WithSource("https://img.kaiheila.cn/attachments/2021-01/21/600972b5d0d31.txt")
    //             .WithTitle("KOOK 介绍.txt"))
    //         .AddModule(new AudioModuleBuilder()
    //             .WithSource("https://img.kaiheila.cn/attachments/2021-01/21/600975671b9ab.mp3")
    //             .WithTitle("命运交响曲")
    //             .WithCover("https://img.kaiheila.cn/assets/2021-01/rcdqa8fAOO0hs0mc.jpg"))
    //         .AddModule(new VideoModuleBuilder()
    //             .WithSource("https://img.kaiheila.cn/attachments/2021-01/20/6008127e8c8de.mp4")
    //             .WithTitle("有本事别笑"))
    //         .AddModule(new DividerModuleBuilder())
    //         .AddModule(new CountdownModuleBuilder().WithMode(CountdownMode.Day).WithEndTime(DateTimeOffset.Now.AddMinutes(1)))
    //         .AddModule(new CountdownModuleBuilder().WithMode(CountdownMode.Hour).WithEndTime(DateTimeOffset.Now.AddMinutes(1)))
    //         .AddModule(new CountdownModuleBuilder().WithMode(CountdownMode.Second).WithEndTime(DateTimeOffset.Now.AddMinutes(2)).WithStartTime(DateTimeOffset.Now.AddMinutes(1)));
    //     
    //     (Guid MessageId, DateTimeOffset MessageTimestamp) response = await _client.GetGuild(((SocketUserMessage) message).Guild.Id)
    //         .GetTextChannel(message.Channel.Id)
    //         .SendCardAsync(cardBuilder.Build(), quote: new Quote(message.Id));
    // }
    //
    // private static async Task ModifyMessageDemo(SocketMessage message)
    // {
    //     await Task.Delay(TimeSpan.FromSeconds(1));
    //
    //     SocketTextChannel channel = message.Channel as SocketTextChannel;
    //     (Guid MessageId, DateTimeOffset MessageTimestamp) response = await channel
    //         .SendTextAsync("BeforeModification");
    //     await Task.Delay(TimeSpan.FromSeconds(1));
    //     
    //     IUserMessage msg = await channel.GetMessageAsync(response.MessageId) as IUserMessage;
    //     await msg!.ModifyAsync(properties => properties.Content += "\n==========\nModified");
    //     await Task.Delay(TimeSpan.FromSeconds(1));
    //     
    //     await msg.DeleteAsync();
    //     await Task.Delay(TimeSpan.FromSeconds(1));
    //
    //     response = await channel.SendCardAsync(new CardBuilder()
    //         .AddModule(new HeaderModuleBuilder().WithText("Test")).Build());
    //     await Task.Delay(TimeSpan.FromSeconds(1));
    //     
    //     msg = await channel.GetMessageAsync(response.MessageId) as IUserMessage;
    //     await msg!.ModifyAsync(properties =>
    //     {
    //         properties.Cards = properties.Cards.Append(new CardBuilder()
    //             .AddModule(new DividerModuleBuilder())
    //             .AddModule(new HeaderModuleBuilder().WithText("ModificationHeader")).Build());
    //     });
    // }
}