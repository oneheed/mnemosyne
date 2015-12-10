using MongoDB.Bson;
using MongoDB.Driver;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Mnemosyne
{
    public class VWLogger : Logger
    {
        private static ConcurrentDictionary<string, Logger> dic;
        private static MongoCollection _db;
        private static string _target;
        private static string _configPath = string.Format(@".\{0}.config", typeof(VWLogger).Namespace);
        static VWLogger()
        {
            if (dic == null)
            {
                dic = new ConcurrentDictionary<string, Logger>();
            }
            Init();
        }

        private static void Init()
        {
            if (GetConfig("MongoDB", "mongo") != null)
            {
                try
                {
                    var str = MongoServerSettings.FromUrl(new MongoUrl(GetConfig("MongoDB", "mongo")));
                    _db = new MongoServer(str).GetDatabase("NLog").GetCollection("nlog");
                }
                catch (Exception e) { Fatal(e.Message); }
            }

            _target = GetConfig("Target", "target");
        }

        public static void SetupConfigPath(string config)
        {
            _configPath = config;
            Init();
        }

        public static new void Debug(string message, int callHierarchyIdx = 1)
        {
            var c = new StackTrace().GetFrame(callHierarchyIdx);
            Write(GetLogger(2), LogLevel.Debug, string.Format("[{0}] {1}", GetCallMethod(2), message));
        }

        public static new void Info(string message, int callHierarchyIdx = 1)
        {
            var c = new StackTrace().GetFrame(callHierarchyIdx);
            Write(GetLogger(2), LogLevel.Info, string.Format("[{0}] {1}", GetCallMethod(2), message));
        }

        public static new void Warn(string message, int callHierarchyIdx = 1)
        {
            var c = new StackTrace().GetFrame(callHierarchyIdx);
            Write(GetLogger(2), LogLevel.Warn, string.Format("[{0}] {1}", GetCallMethod(2), message));
        }

        public static new void Trace(string message, int callHierarchyIdx = 1)
        {
            var c = new StackTrace().GetFrame(callHierarchyIdx);
            Write(GetLogger(2), LogLevel.Trace, string.Format("[{0}] {1}", GetCallMethod(2), message));
        }

        public static new void Error(string message, int callHierarchyIdx = 1)
        {
            var c = new StackTrace().GetFrame(callHierarchyIdx);
            Write(GetLogger(2), LogLevel.Error, string.Format("[{0}] {1}", GetCallMethod(2), message));
        }

        public static new void Fatal(string message, int callHierarchyIdx = 1)
        {
            var c = new StackTrace().GetFrame(callHierarchyIdx);
            Write(GetLogger(2), LogLevel.Fatal, string.Format("[{0}] {1}", GetCallMethod(2), message));
        }

        public static void Error(string message, Exception ex, int callHierarchyIdx = 1)
        {
            var c = new StackTrace().GetFrame(callHierarchyIdx);
            Write(GetLogger(2), LogLevel.Error, string.Format("[{0}] {1} | [Ex]{2} {3}", GetCallMethod(2), message, ex, ex.StackTrace));
        }

        public static void Fatal(string message, Exception ex, int callHierarchyIdx = 1)
        {
            var c = new StackTrace().GetFrame(callHierarchyIdx);
            Write(GetLogger(2), LogLevel.Fatal, string.Format("[{0}] {1} | [Ex]{2} {3}", GetCallMethod(2), message, ex, ex.StackTrace));
        }

        private static Logger GetLogger(int callHierarchyIdx = 1)
        {
            try
            {
                var c = new StackTrace().GetFrame(callHierarchyIdx);
                var loggerName = c.GetMethod().DeclaringType.ToString();
                return dic.GetOrAdd(loggerName, LogManager.GetLogger(loggerName));
            }
            catch
            {
                return dic.GetOrAdd(typeof(VWLogger).ToString(), LogManager.GetLogger(typeof(VWLogger).ToString()));
            }
        }

        private static string GetCallMethod(int callHierarchyIdx = 1)
        {
            try
            {
                var c = new StackTrace().GetFrame(callHierarchyIdx);
                return c.GetMethod().Name;
            }
            catch { return string.Empty; }
        }

        private static void Write(Logger logger, LogLevel level, string message)
        {
            logger.Log(level, message);
            if ((_target.Equals("ALL", StringComparison.CurrentCultureIgnoreCase) || _target.Contains("MONGO")) &&
                level >= LogLevel.FromString(GetConfig("MongoDB", "level")))
                WriteMongo(level.ToString(), message);
        }

        private static string GetConfig(string section, string key)
        {
            try
            {
                var doc = XDocument.Load(_configPath);

                foreach (var o in doc.Root.Elements(section).Elements().Where(o => o.Name.ToString().ToLower().Equals("add")))
                {
                    if (o.Attribute("key").Value.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                        return o.Attribute("value").Value;
                }
                return "";
            }
            catch
            {
                return null;
            }
        }

        private static void WriteMongo(string level, string message)
        {
            if (_db != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        _db.Insert(new
                        {
                            Level = level,
                            TimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                            User = string.Format(@"{0}___{1}\{2}", Environment.MachineName, Environment.UserDomainName, Environment.UserName),
                            PID = Process.GetCurrentProcess().Id,
                            TID = Thread.CurrentThread.ManagedThreadId,
                            Process = Process.GetCurrentProcess().ProcessName,
                            Message = message
                        });
                        break;
                    }
                    catch { }
                }
            }
        }
    }
}
