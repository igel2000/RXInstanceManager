using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.ServiceProcess;
using YamlHandlers;
using NLog;

namespace RXInstanceManager
{
    public static class AppHandlers
    {
        private static Logger globalLogger = LogManager.GetCurrentClassLogger();

        #region Работа с ServiceRunner.

        public static string GetServiceStatus(Instance instance)
        {
            var serviceStatus = Constants.InstanceStatus.NeedInstall;

            using (var service = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == instance.ServiceName))
            {
                if (service != null)
                    serviceStatus = service.Status == ServiceControllerStatus.Running ?
                        Constants.InstanceStatus.Working :
                        Constants.InstanceStatus.Stopped;
            }

            return serviceStatus;
        }

        #endregion

        #region Работа с версиями.

        public static string GetInstanceVersion(string instancePath)
        {
            var versionsPath = AppHelper.GetVersionsPath(instancePath);
            if (!File.Exists(versionsPath))
                return Constants.NullVersion;

            var versions = File.ReadAllText(versionsPath);
            var versionsParser = new YamlParser(versions);
            return versionsParser.SelectToken("$.builds.platform_builds")["version"].ToString();
        }

        #endregion

        #region Работа с логом.

        public static void ShowProcessLog(string processLog)
        {
            var log = Path.Combine(Constants.LogPath, "log_" + DateTime.Now.ToString("ddMMyyHHmm") + ".log");
            File.WriteAllText(log, processLog);

            var process = new Process();
            process.StartInfo.FileName = "notepad.exe";
            process.StartInfo.Arguments = log;
            process.Start();
        }

        public static void ShowMainLog()
        {
            var log = Path.Combine(Constants.LogPath, DateTime.Now.ToShortDateString() + ".log");
            var process = new Process();
            process.StartInfo.FileName = "notepad.exe";
            process.StartInfo.Arguments = log;
            process.Start();
        }

        public static void InfoHandler(Instance instance, string message)
        {
            var code = instance != null ? instance.Code : string.Empty;
            var path = instance != null ? instance.InstancePath : string.Empty;
            var logBody = string.Format($"Code: {code}, Path: {path}, Message: {message}");
            globalLogger.Info(logBody);
        }

        public static void ErrorHandler(Instance instance, Exception exception)
        {
            var code = instance != null ? instance.Code : string.Empty;
            var path = instance != null ? instance.InstancePath : string.Empty;
            var logBody = string.Format($"Code: {code}, Path {path}, Message: {exception.Message}, {exception.StackTrace}");
            globalLogger.Error(logBody);
            if (exception.InnerException != null)
            {
                globalLogger.Error(string.Format($"Message: {exception.InnerException.Message}, {exception.InnerException.StackTrace}"));
                if (exception.InnerException.InnerException != null)
                    globalLogger.Error(string.Format($"Message: {exception.InnerException.InnerException.Message}, {exception.InnerException.InnerException.StackTrace}"));
            }

            ShowMainLog();
        }

        #endregion
    }
}
