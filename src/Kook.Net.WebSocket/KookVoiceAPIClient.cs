using Kook.API;
using Kook.API.Voice;
using Kook.Net.Converters;
using Kook.Net.Udp;
using Kook.Net.WebSockets;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Kook.Audio;

internal class KookVoiceAPIClient : IDisposable
{
    #region KookVoiceAPIClient
    public const int MaxBitrate = 128 * 1024;
    public const string Mode = "xsalsa20_poly1305";

    public event Func<string, string, double, Task> SentRequest
    {
        add => _sentRequestEvent.Add(value);
        remove => _sentRequestEvent.Remove(value);
    }
    private readonly AsyncEvent<Func<string, string, double, Task>> _sentRequestEvent = new AsyncEvent<Func<string, string, double, Task>>();
    public event Func<VoiceSocketFrameType, Task> SentGatewayMessage
    {
        add => _sentGatewayMessageEvent.Add(value);
        remove => _sentGatewayMessageEvent.Remove(value);
    }
    private readonly AsyncEvent<Func<VoiceSocketFrameType, Task>> _sentGatewayMessageEvent = new AsyncEvent<Func<VoiceSocketFrameType, Task>>();
    public event Func<Task> SentDiscovery
    {
        add => _sentDiscoveryEvent.Add(value);
        remove => _sentDiscoveryEvent.Remove(value);
    }
    private readonly AsyncEvent<Func<Task>> _sentDiscoveryEvent = new AsyncEvent<Func<Task>>();
    public event Func<int, Task> SentData
    {
        add => _sentDataEvent.Add(value);
        remove => _sentDataEvent.Remove(value);
    }
    private readonly AsyncEvent<Func<int, Task>> _sentDataEvent = new AsyncEvent<Func<int, Task>>();
    public event Func<uint, bool, object, Task> ReceivedEvent
    {
        add => _receivedEvent.Add(value);
        remove => _receivedEvent.Remove(value);
    }
    private readonly AsyncEvent<Func<uint, bool, object, Task>> _receivedEvent = new AsyncEvent<Func<uint, bool, object, Task>>();
    public event Func<byte[], Task> ReceivedPacket
    {
        add => _receivedPacketEvent.Add(value);
        remove => _receivedPacketEvent.Remove(value);
    }
    private readonly AsyncEvent<Func<byte[], Task>> _receivedPacketEvent = new AsyncEvent<Func<byte[], Task>>();
    public event Func<Exception, Task> Disconnected
    {
        add => _disconnectedEvent.Add(value);
        remove => _disconnectedEvent.Remove(value);
    }
    private readonly AsyncEvent<Func<Exception, Task>> _disconnectedEvent = new AsyncEvent<Func<Exception, Task>>();

    private readonly JsonSerializerOptions _serializerOptions;
    private readonly SemaphoreSlim _connectionLock;
    private readonly IUdpSocket _udp;
    private CancellationTokenSource _connectCancelToken;
    private bool _isDisposed;
    private ulong _nextKeepalive;
    private uint _sequence;

    public ulong GuildId { get; }
    internal IWebSocketClient WebSocketClient { get; }
    public ConnectionState ConnectionState { get; private set; }

    public ushort UdpPort => _udp.Port;

    internal KookVoiceAPIClient(ulong guildId, WebSocketProvider webSocketProvider,
        UdpSocketProvider udpSocketProvider, JsonSerializerOptions serializerOptions = null)
    {
        GuildId = guildId;
        _sequence = 1000000;
        _connectionLock = new SemaphoreSlim(1, 1);
        _udp = udpSocketProvider();
        _udp.ReceivedDatagram += async (data, index, count) =>
        {
            if (index != 0 || count != data.Length)
            {
                var newData = new byte[count];
                Buffer.BlockCopy(data, index, newData, 0, count);
                data = newData;
            }
            await _receivedPacketEvent.InvokeAsync(data).ConfigureAwait(false);
        };

        WebSocketClient = webSocketProvider();
        //_gatewayClient.SetHeader("user-agent", KookConfig.UserAgent); //(Causes issues in .Net 4.6+)
        WebSocketClient.BinaryMessage += async (data, index, count) =>
        {
            using (var compressed = new MemoryStream(data, index + 2, count - 2))
            using (var decompressed = new MemoryStream())
            {
                using (var zlib = new DeflateStream(compressed, CompressionMode.Decompress))
                    zlib.CopyTo(decompressed);
                decompressed.Position = 0;
                using (var reader = new StreamReader(decompressed))
                {
                    var msg = JsonSerializer.Deserialize<VoiceSocketResponseFrame>(reader.ReadToEnd(), serializerOptions);
                    await _receivedEvent.InvokeAsync(msg.Id, msg.Okay, msg.Payload).ConfigureAwait(false);
                }
            }
        };
        WebSocketClient.TextMessage += async text =>
        {
            var msg = JsonSerializer.Deserialize<VoiceSocketResponseFrame>(text, serializerOptions);
            await _receivedEvent.InvokeAsync(msg.Id, msg.Okay, msg.Payload).ConfigureAwait(false);
        };
        WebSocketClient.Closed += async ex =>
        {
            await DisconnectAsync().ConfigureAwait(false);
            await _disconnectedEvent.InvokeAsync(ex).ConfigureAwait(false);
        };

        _serializerOptions = serializerOptions ?? new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
    }
    private void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _connectCancelToken?.Dispose();
                _udp?.Dispose();
                WebSocketClient?.Dispose();
                _connectionLock?.Dispose();
            }
            _isDisposed = true;
        }
    }
    public void Dispose() => Dispose(true);

    public async Task SendAsync(VoiceSocketFrameType type, object payload, RequestOptions options = null)
    {
        byte[] bytes = null;
        payload = new VoiceSocketRequestFrame { Type = type, Id = _sequence++, Request = true, Payload = payload };
        bytes = System.Text.Encoding.UTF8.GetBytes(SerializeJson(payload));
        await WebSocketClient.SendAsync(bytes, 0, bytes.Length, true).ConfigureAwait(false);
        await _sentGatewayMessageEvent.InvokeAsync(type).ConfigureAwait(false);
    }
    public async Task SendAsync(byte[] data, int offset, int bytes)
    {
        await _udp.SendAsync(data, offset, bytes).ConfigureAwait(false);
        await _sentDataEvent.InvokeAsync(bytes).ConfigureAwait(false);
    }
    #endregion

    #region WebSocket

    public async Task SendGetRouterRTPCapabilitiesRequestAsync(RequestOptions options = null)
    {
        await SendAsync(VoiceSocketFrameType.GetRouterRtpCapabilities, null, options).ConfigureAwait(false);
    }

    public async Task SendJoinRequestAsync(RequestOptions options = null)
    {
        await SendAsync(VoiceSocketFrameType.Join, new JoinParams
        {
            DisplayName = string.Empty
        }, options).ConfigureAwait(false);
    }

    public async Task SendCreatePlainTransportRequestAsync(RequestOptions options = null)
    {
        await SendAsync(VoiceSocketFrameType.CreatePlainTransport, new CreatePlainTransportParams
        {
            Comedia = true,
            RTCPMultiplexing = false,
            Type = "plain"
        }, options).ConfigureAwait(false);
    }
    public async Task SendProduceRequestAsync(Guid transportId, uint ssrc, RequestOptions options = null)
    {
        await SendAsync(VoiceSocketFrameType.Produce, new ProduceParams(transportId)
        {
            AppData = new object(),
            PeerId = string.Empty,
            RTPParameters = new RTPParameters
            {
                Codecs = new[]
                {
                    new Codec()
                    {
                        Channels = 2,
                        ClockRate = 48000,
                        MimeType = "audio/opus",
                        Parameters = new Parameters
                        {
                            SenderProduceStereo = 1
                        },
                        PayloadType = 100
                    }
                },
                Encodings = new[]
                {
                    new API.Voice.Encoding
                    {
                        SSRC = ssrc
                    }
                }
            }
        }, options).ConfigureAwait(false);
    }
    // public async Task SendHeartbeatAsync(RequestOptions options = null)
    // {
    //     await SendAsync(VoiceOpCode.Heartbeat, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), options: options).ConfigureAwait(false);
    // }

    public async Task ConnectAsync(string url)
    {
        await _connectionLock.WaitAsync().ConfigureAwait(false);
        try
        {
            await ConnectInternalAsync(url).ConfigureAwait(false);
        }
        finally { _connectionLock.Release(); }
    }
    private async Task ConnectInternalAsync(string url)
    {
        ConnectionState = ConnectionState.Connecting;
        try
        {
            _connectCancelToken?.Dispose();
            _connectCancelToken = new CancellationTokenSource();
            var cancelToken = _connectCancelToken.Token;

            WebSocketClient.SetCancelToken(cancelToken);
            await WebSocketClient.ConnectAsync(url).ConfigureAwait(false);

            _udp.SetCancelToken(cancelToken);
            await _udp.StartAsync().ConfigureAwait(false);

            ConnectionState = ConnectionState.Connected;
        }
        catch
        {
            await DisconnectInternalAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        await _connectionLock.WaitAsync().ConfigureAwait(false);
        try
        {
            await DisconnectInternalAsync().ConfigureAwait(false);
        }
        finally { _connectionLock.Release(); }
    }
    private async Task DisconnectInternalAsync()
    {
        if (ConnectionState == ConnectionState.Disconnected)
            return;
        ConnectionState = ConnectionState.Disconnecting;

        try
        { _connectCancelToken?.Cancel(false); }
        catch
        {
            // ignored
        }

        //Wait for tasks to complete
        await _udp.StopAsync().ConfigureAwait(false);
        await WebSocketClient.DisconnectAsync().ConfigureAwait(false);

        ConnectionState = ConnectionState.Disconnected;
    }
    #endregion

    #region Udp
    public async Task SendDiscoveryAsync(uint ssrc)
    {
        var packet = new byte[70];
        packet[0] = (byte)(ssrc >> 24);
        packet[1] = (byte)(ssrc >> 16);
        packet[2] = (byte)(ssrc >> 8);
        packet[3] = (byte)(ssrc >> 0);
        await SendAsync(packet, 0, 70).ConfigureAwait(false);
        await _sentDiscoveryEvent.InvokeAsync().ConfigureAwait(false);
    }
    public async Task<ulong> SendKeepaliveAsync()
    {
        var value = _nextKeepalive++;
        var packet = new byte[8];
        packet[0] = (byte)(value >> 0);
        packet[1] = (byte)(value >> 8);
        packet[2] = (byte)(value >> 16);
        packet[3] = (byte)(value >> 24);
        packet[4] = (byte)(value >> 32);
        packet[5] = (byte)(value >> 40);
        packet[6] = (byte)(value >> 48);
        packet[7] = (byte)(value >> 56);
        await SendAsync(packet, 0, 8).ConfigureAwait(false);
        return value;
    }

    public void SetUdpEndpoint(string ip, int port)
    {
        _udp.SetDestination(ip, port);
    }
    #endregion

    #region Helpers
    private static double ToMilliseconds(Stopwatch stopwatch) => Math.Round((double)stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0, 2);

    private string SerializeJson(object payload)
    {
        return payload is null
            ? string.Empty
            : JsonSerializer.Serialize(payload, _serializerOptions);
    }

    private T DeserializeJson<T>(Stream jsonStream)
    {
        return JsonSerializer.Deserialize<T>(jsonStream, _serializerOptions);
    }
    #endregion
}
