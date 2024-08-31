// Use TCPeasy.Cli https://github.com/sschmid/TCPeasy
//TCPeasy.Cli.Program.Main(args);

// Manual setup using TCPeasy

// Setup Sherlog
Logger.GlobalLogLevel = LogLevel.Info;
Logger.AddAppender((logger, logLevel, message) => Console.WriteLine(message));
var logger = Logger.GetLogger(nameof(Program));

// Create TcpMessageParser to handle messages from a tcp connection
var messageParser = new TcpMessageParser();
messageParser.OnMessage += (parser, bytes) => logger.Info(Encoding.UTF8.GetString(bytes));

// Listen for incoming messages on port 12345 and
// forward bytes to the messageParser to extract the actual messages.
var server = new TcpServerSocket();
server.OnReceived += (socket, client, bytes) => messageParser.Receive(bytes);
server.Listen(12345);

Console.CancelKeyPress += delegate { server.Disconnect(); };

while (true) Console.ReadLine();

// output 2 single exe
// right click this csproj file then open terminal input command below
// dotnet publish SherlogServer.csproj -c Release -r win-x64 --self-contained
