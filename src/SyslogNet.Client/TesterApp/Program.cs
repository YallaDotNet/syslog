using System;
using CommandLine;
using SyslogNet.Client;
using SyslogNet.Client.Serialization;
using SyslogNet.Client.Transport;
using System.Threading.Tasks;

namespace TesterApp
{
	internal class Options
	{
		[Option("h", "hostName", Required = false, HelpText = "The host name. If not set, defaults to the NetBIOS name of the local machine")]
		public string LocalHostName { get; set; }

		[Option("a", "appName", Required = false, HelpText = "The application name")]
		public string AppName { get; set; }

		[Option("p", "procId", Required = false, HelpText = "The process identifier")]
		public string ProcId { get; set; }

		[Option("t", "msgType", Required = false, HelpText = "The message type (called msgId in spec)")]
		public string MsgType { get; set; }

		[Option("m", "msg", Required = false, HelpText = "The message")]
		public string Message { get; set; }

		[Option("s", "syslogServer", Required = true, HelpText = "Host name of the syslog server")]
		public string SyslogServerHostname { get; set; }

		[Option("r", "syslogPort", Required = true, HelpText = "The syslog server port")]
		public int SyslogServerPort { get; set; }

		[Option("v", "version", Required = false, DefaultValue = "5424", HelpText = "The version of syslog protocol to use. Possible values are '3164' and '5424' (from corresponding RFC documents) or 'local' to send messages to a local syslog (only on Linux or OS X). Default is '5424'")]
		public string SyslogVersion { get; set; }

		[Option("o", "protocol", Required = false, DefaultValue = "tcp", HelpText = "The network protocol to use. Possible values are 'tcp' or 'udp' to send to a remote syslog server, or 'local' to send to a local syslog over Unix sockets (only on Linux or OS X). Default is 'tcp'. Note: TCP always uses SSL connection.")]
		public string NetworkProtocol { get; set; }

		[Option("c", "cert", Required = false, HelpText = "Optional path to a CA certificate used to verify Syslog server certificate when using TCP protocol")]
		public string CACertPath { get; set; }
	}

	public static class Program
	{
		public static void Main(string[] args)
		{
            MainAsync(args).GetAwaiter().GetResult();
		}

        private static async Task MainAsync(string[] args)
        {
			try
			{
				var options = new Options();
				if (new CommandLineParser().ParseArguments(args, options))
				{
					// string exceptionMessage = CreateExceptionMessageLevel1();

					ISyslogMessageSerializer serializer = options.SyslogVersion == "5424"
						? (ISyslogMessageSerializer)SyslogRfc5424MessageSerializer.Default
						: options.SyslogVersion == "3164"
                            ? (ISyslogMessageSerializer)SyslogRfc3164MessageSerializer.Default
							: (ISyslogMessageSerializer)SyslogLocalMessageSerializer.Default;

                    IAsyncSyslogMessageSender sender = options.NetworkProtocol == "tcp"
                        ? (IAsyncSyslogMessageSender)new AsyncSyslogSecureTcpSender(options.SyslogServerHostname, options.SyslogServerPort)
                        : (IAsyncSyslogMessageSender)new AsyncSyslogUdpSender(options.SyslogServerHostname, options.SyslogServerPort);

                    await sender.ConnectAsync();

					SyslogMessage msg1 = CreateSyslogMessage(options);
                    await sender.SendAsync(msg1, serializer);
					Console.WriteLine("Sent message 1");

                    await Task.Delay(5000);

					SyslogMessage msg2 = CreateSyslogMessage(options);
                    await sender.SendAsync(msg2, serializer);
					Console.WriteLine("Sent message 2");

                    await sender.DisconnectAsync();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("ERROR: " + ex);
			}
		}

		private static SyslogMessage CreateSyslogMessage(Options options)
		{
			return new SyslogMessage(
				DateTimeOffset.Now,
				Facility.UserLevelMessages,
				Severity.Error,
				options.LocalHostName ?? Environment.MachineName,
				options.AppName,
				options.ProcId,
				options.MsgType,
				options.Message ?? "Test message at " + DateTime.Now);
		}

		private static string CreateExceptionMessageLevel1()
		{
			try
			{
				return CreateExceptionMessageLevel2();
			}
			catch (Exception ex)
			{
				return ex.ToString();
			}
		}

		private static string CreateExceptionMessageLevel2()
		{
			return CreateExceptionMessageLevel3();
		}

		private static string CreateExceptionMessageLevel3()
		{
			return CreateExceptionMessageLevel4();
		}

		private static string CreateExceptionMessageLevel4()
		{
			throw new Exception("Foo bar");
		}
	}
}