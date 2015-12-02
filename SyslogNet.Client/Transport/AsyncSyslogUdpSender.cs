using Sockets.Plugin;
using SyslogNet.Client.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SyslogNet.Client.Transport
{
    public sealed class AsyncSyslogUdpSender : AsyncSyslogSenderBase
    {
        private readonly UdpSocketClient udpClient;

        public AsyncSyslogUdpSender(string hostname, int port)
            : base(hostname, port)
        {
            udpClient = new UdpSocketClient();
        }

        public override void Dispose()
        {
            udpClient.Dispose();
        }

        protected override async Task ConnectAsync(string hostname, int port)
        {
            await udpClient.ConnectAsync(hostname, port).ConfigureAwait(false);
        }

        public override async Task DisconnectAsync()
        {
            await udpClient.DisconnectAsync().ConfigureAwait(false);
        }

        public override Task ReconnectAsync()
        {
            // UDP is connectionless
            return new TaskCompletionSource<object>().Task;
        }

        protected override async Task WriteAsync(byte[] datagramBytes, ISyslogMessageSerializer serializer, CancellationToken cancellationToken)
        {
            await udpClient.SendAsync(datagramBytes).ConfigureAwait(false);
        }
    }
}