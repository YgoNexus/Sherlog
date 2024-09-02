#define LLOG2TCP
#define LLOG
#define LLOG2FILE
using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

using Object = UnityEngine.Object;
using Logger = Sherlog.Logger;
using Sherlog.Appenders;
using System.Net;
using Sherlog.Formatters;


public static class LLog
{
    static LogLevel UnityLogThreshold = LogLevel.On;
    public static Action<LogLevel, string, Object> OnPrint;
    public static bool RegisterPrint = false;
    public static void SetLogThreshold(LogLevel logType) => UnityLogThreshold = logType;

    static TcpSocketAppender socketAppender;
    [Conditional("LLOG2TCP")]
    public static void OutputSherlogTCPInit(int tcpPort = 12345)
    {
        if (socketAppender == null)
        {
            socketAppender = new TcpSocketAppender();
            socketAppender.Connect(IPAddress.Loopback, tcpPort);
        }
        string loggerName = typeof(LLog).ToString();
        var logger = Logger.GetLogger(loggerName);
        logger.LogLevel = UnityLogThreshold;
        if (logger.LogLevel < LogLevel.Debug)
            logger.LogLevel = LogLevel.Debug;

        LogMessageFormatter messageFormatter = new LogMessageFormatter();
        TimestampFormatter timestampFormatter = new TimestampFormatter(() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        ColorCodeFormatter colorCodeFormatter = new ColorCodeFormatter();
        Logger.AddAppender((logger, level, message) =>
        {
            message = timestampFormatter.FormatMessage(logger, level, message);
            message = messageFormatter.FormatMessage(logger, level, message);
            message = colorCodeFormatter.FormatMessage(logger, level, message);
            socketAppender.Send(logger, level, message);
        });
        Output2SherlogEvent(loggerName);
    }
    [Conditional("LLOG2FILE")]
    public static void OutputSherlogFileInit(string filePath = null)
    {
        if (filePath == null)
            filePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}/log.txt";
        string loggerName = typeof(LLog).ToString();
        var logger = Logger.GetLogger(loggerName);
        logger.LogLevel = UnityLogThreshold;

        var timestampFormatter = new TimestampFormatter(() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        var messageFormatter = new LogMessageFormatter();
        var stackInfoFormatter = new StackInfoFormatter();
        // out put 2 file do not support color code
        //var colorCodeFormatter = new ColorCodeFormatter();

        FileWriterAppender fw = new FileWriterAppender(filePath);
        Logger.AddAppender((logger, level, message) =>
        {
            message = timestampFormatter.FormatMessage(logger, level, message);
            message = messageFormatter.FormatMessage(logger, level, message);
            message = stackInfoFormatter.FormatMessage(logger, level, message);
            fw.WriteLine(logger, level, message);
        });
        Output2SherlogEvent(loggerName);
    }
    [Conditional("LLOG")]
    static void Output2SherlogEvent(string loggerName)
    {
        if (RegisterPrint == false)
        {
            RegisterPrint = true;
            // first initialization
            OnPrint = (lv, msg, obj) =>
            {
                // TcpSocketAppender also uses Sherlog under the hood and logs sending messages
                // with logger.Debug(message). Set its log level higher than LogLevel.Debug
                // to avoid stack overflow.
                Logger.GetLogger(loggerName).Log(lv, msg);
            };
            LLog.Log(GetUserSystemInfo());
        }
    }

    static bool CanPrint(LogLevel lv)
    {
        return lv >= UnityLogThreshold;
    }
    [Conditional("LLOG")]
    static void Print(LogLevel logType, object message, Object context = null)
    {
        if (CanPrint(logType))
        {
            if (logType <= LogLevel.Info)
                UnityEngine.Debug.Log(message, context);
            else if (logType <= LogLevel.Warn)
                UnityEngine.Debug.LogWarning(message, context);
            else
                UnityEngine.Debug.LogError(message, context);
            OnPrint?.Invoke(logType, message.ToString(), context);
        }
    }

    [Conditional("LLOG")]
    static void PrintFormat(LogLevel logType, string format, Object context = null, params object[] args)
    {
        if (CanPrint(logType))
        {
            if (logType <= LogLevel.Info)
                UnityEngine.Debug.LogFormat(format, args, context);
            else if (logType <= LogLevel.Warn)
                UnityEngine.Debug.LogWarning(format, context);
            else
                UnityEngine.Debug.LogError(format, context);
            OnPrint?.Invoke(logType, format.ToString(), context);
        }
    }

    [Conditional("LLOG")]
    public static void Log(object message, Object obj = null)
    {
        Print(LogLevel.Debug, message, obj);
    }
    [Conditional("LLOG")]
    public static void Warning(object message, Object obj = null)
    {
        Print(LogLevel.Warn, message, obj);
    }
    [Conditional("LLOG")]
    public static void Error(object message, Object obj = null)
    {
        Print(LogLevel.Error, message, obj);
    }
    [Conditional("LLOG")]
    public static void LogFormat(string format, params object[] args)
    {
        LogFormat(format, null, args);
    }
    [Conditional("LLOG")]
    public static void LogFormat(string format, Object contex, params object[] args)
    {
        PrintFormat(LogLevel.Debug, format, contex, args);
    }
    [Conditional("LLOG")]
    public static void WarningFormat(string format, params object[] args)
    {
        WarningFormat(format, null, args);
    }
    [Conditional("LLOG")]
    public static void WarningFormat(string format, Object contex, params object[] args)
    {
        PrintFormat(LogLevel.Warn, format, contex, args);
    }
    [Conditional("LLOG")]
    public static void ErrorFormat(string format, params object[] args)
    {
        ErrorFormat(format, null, args);
    }
    [Conditional("LLOG")]
    public static void ErrorFormat(string format, Object contex, params object[] args)
    {
        PrintFormat(LogLevel.Error, format, contex, args);
    }

    public static string GetUserSystemInfo()
    {
        StringBuilder sb = new StringBuilder(256 * 8);
        sb.AppendLine();
        sb.AppendLine("---------System Info-----");
        sb.AppendLine("Device: " + SystemInfo.deviceName);
        sb.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        sb.AppendLine("OS:  " + SystemInfo.operatingSystem);
        sb.AppendLine("SystemMemory:  " + SystemInfo.systemMemorySize);
        sb.AppendLine("DeviceModel:  " + SystemInfo.deviceModel);
        sb.AppendLine("deviceUniqueIdentifier:  " + SystemInfo.deviceUniqueIdentifier);
        sb.AppendLine("processorCount:  " + SystemInfo.processorCount);
        sb.AppendLine("processorType:  " + SystemInfo.processorType);
        sb.AppendLine("graphicsDeviceID:  " + SystemInfo.graphicsDeviceID);
        sb.AppendLine("graphicsDeviceName:  " + SystemInfo.graphicsDeviceName);
        sb.AppendLine("graphicsDeviceVendorID:  " + SystemInfo.graphicsDeviceVendorID);
        sb.AppendLine("graphicsDeviceVendor:  " + SystemInfo.graphicsDeviceVendor);
        sb.AppendLine("graphicsDeviceVersion:  " + SystemInfo.graphicsDeviceVersion);
        sb.AppendLine("GraphicsMemorySize:  " + SystemInfo.graphicsMemorySize);
        sb.AppendLine("GraphicsShaderLevel:  " + SystemInfo.graphicsShaderLevel);
        sb.AppendLine("SupportShadows:  " + SystemInfo.supportsShadows);
        //sb.AppendLine("操作系统:  " + SystemInfo.operatingSystem);
        //sb.AppendLine("内存容量:  " + SystemInfo.systemMemorySize);
        //sb.AppendLine("设备模型:  " + SystemInfo.deviceModel); 
        //sb.AppendLine("设备唯一标识符:  " + SystemInfo.deviceUniqueIdentifier);
        //sb.AppendLine("处理器数量:  " + SystemInfo.processorCount);
        //sb.AppendLine("处理器类型:  " + SystemInfo.processorType);
        //sb.AppendLine("显卡标识符:  " + SystemInfo.graphicsDeviceID);
        //sb.AppendLine("显卡名称:  " + SystemInfo.graphicsDeviceName);
        //sb.AppendLine("显卡标识符:  " + SystemInfo.graphicsDeviceVendorID);
        //sb.AppendLine("显卡厂商:  " + SystemInfo.graphicsDeviceVendor);
        //sb.AppendLine("显卡版本:  " + SystemInfo.graphicsDeviceVersion);
        //sb.AppendLine("显存大小:  " + SystemInfo.graphicsMemorySize);
        //sb.AppendLine("显卡着色器级别:  " + SystemInfo.graphicsShaderLevel);
        //sb.AppendLine("是否支持内置阴影:  " + SystemInfo.supportsShadows);
        sb.AppendLine("---------End System Info-----");
        return sb.ToString();
    }

}
