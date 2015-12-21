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
    public class MnemosyneLogger : Logger
    {
        private static ConcurrentDictionary<string, Logger> dic;
        private static MongoCollection _db;
        private string _target;
        private readonly string _configPath;
        public MnemosyneLogger()
        {
            if (dic == null)
            {
                dic = new ConcurrentDictionary<string, Logger>();
            }
            _configPath = string.Format(@".\{0}.config", typeof(MnemosyneLogger).Namespace);
            Init();
        }

        public MnemosyneLogger(string configPath)
        {
            if (dic == null)
            {
                dic = new ConcurrentDictionary<string, Logger>();
            }
            _configPath = configPath;
            Init();
        }

        private void Init()
        {
            var mongoServer = GetConfig("MongoDB", "mongo");
            if (!string.IsNullOrEmpty(mongoServer))
            {
                try
                {
                    var str = MongoServerSettings.FromUrl(new MongoUrl(mongoServer));
                    _db = new MongoServer(str).GetDatabase("NLog").GetCollection("nlog");
                }
                catch (Exception e)
                {
                    var msg = new LogMessage("Cannot init VWLogger");
                    msg.ExtraData.Add("ExMsg", e.Message);
                    Fatal(msg);
                    throw e;
                }
            }

            _target = GetConfig("Target", "target");
            if (string.IsNullOrEmpty(_target))
            {
                var msg = new LogMessage("Cannot init VWLogger");
                msg.ExtraData.Add("ExMsg", "Cannot get target of logger");
                Fatal(msg);
                throw new Exception("Cannot get target of logger");
            }
        }

        //public static new void Debug(string message, int callHierarchyIdx = 1)
        //{
        //    Write(GetLogger(2), LogLevel.Debug, string.Format("[{0}] {1}", GetCallMethod(2), message));
        //}

        public void Debug(LogMessage messageObj, int callHierarchyIdx = 1)
        {
            Write(GetLogger(2), LogLevel.Debug, messageObj);
        }

        //public static new void Info(string message, int callHierarchyIdx = 1)
        //{
        //    Write(GetLogger(2), LogLevel.Info, string.Format("[{0}] {1}", GetCallMethod(2), message));
        //}

        public void Info(LogMessage messageObj)
        {
            Write(GetLogger(2), LogLevel.Info, messageObj);
        }

        //public static new void Warn(string message, int callHierarchyIdx = 1)
        //{
        //    Write(GetLogger(2), LogLevel.Warn, string.Format("[{0}] {1}", GetCallMethod(2), message));
        //}

        public void Warn(LogMessage messageObj)
        {
            Write(GetLogger(2), LogLevel.Warn, messageObj);
        }

        //public static new void Trace(string message, int callHierarchyIdx = 1)
        //{
        //    Write(GetLogger(2), LogLevel.Trace, string.Format("[{0}] {1}", GetCallMethod(2), message));
        //}

        public void Trace(LogMessage messageObj)
        {
            Write(GetLogger(2), LogLevel.Trace, messageObj);
        }

        //public static new void Error(string message, int callHierarchyIdx = 1)
        //{
        //    Write(GetLogger(2), LogLevel.Error, string.Format("[{0}] {1}", GetCallMethod(2), message));
        //}

        public void Error(LogMessage messageObj)
        {
            Write(GetLogger(2), LogLevel.Error, messageObj);
        }

        //public static new void Fatal(string message, int callHierarchyIdx = 1)
        //{
        //    Write(GetLogger(2), LogLevel.Fatal, string.Format("[{0}] {1}", GetCallMethod(2), message));
        //}

        public void Fatal(LogMessage messageObj)
        {
            Write(GetLogger(2), LogLevel.Fatal, messageObj);
        }

        private Logger GetLogger(int callHierarchyIdx = 1)
        {
            try
            {
                var c = new StackTrace().GetFrame(callHierarchyIdx);
                var loggerName = c.GetMethod().DeclaringType.ToString();
                return dic.GetOrAdd(loggerName, LogManager.GetLogger(loggerName));
            }
            catch
            {
                return dic.GetOrAdd(typeof(MnemosyneLogger).ToString(), LogManager.GetLogger(typeof(MnemosyneLogger).ToString()));
            }
        }

        //private static string GetCallMethod(int callHierarchyIdx = 1)
        //{
        //    try
        //    {
        //        var c = new StackTrace().GetFrame(callHierarchyIdx);
        //        return c.GetMethod().Name;
        //    }
        //    catch { return string.Empty; }
        //}

        //private static void Write(Logger logger, LogLevel level, string message)
        //{
        //    logger.Log(level, message);
        //    if ((_target.Equals("ALL", StringComparison.CurrentCultureIgnoreCase) || _target.Contains("MONGO")) &&
        //        level >= LogLevel.FromString(GetConfig("MongoDB", "level")))
        //        WriteMongo<string>(level.ToString(), message);
        //}

        private void Write(Logger logger, LogLevel level, LogMessage messageObj)
        {
            //${ level: uppercase = true} | ${ logger} | [PID:${ processid}][TID:${threadid}] | ${message} ${newline}
            string msg = string.Format("{0} | {1} | {2}", level, logger.Name, GetMessageFromObj(messageObj));
            messageObj.Level = level.ToString();
            logger.Log(level, msg);
            if ((_target.Equals("ALL", StringComparison.CurrentCultureIgnoreCase) || _target.Contains("MONGO")) &&
                level >= LogLevel.FromString(GetConfig("MongoDB", "level")))
                WriteMongo(level.ToString(), messageObj);
        }

        private string GetConfig(string section, string key)
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
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private void WriteMongo(string level, LogMessage message)
        {
            if (_db != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        _db.Insert(message);
                        break;
                    }
                    catch { }
                }
            }
        }

        private void FillLogMessageObj(LogMessage messageObj)
        {
            //if (string.IsNullOrEmpty(messageObj.PID))
            //{
            //    messageObj.PID = Process.GetCurrentProcess().Id.ToString();
            //}

            //if (string.IsNullOrEmpty(messageObj.TID))
            //{
            //    messageObj.TID = Thread.CurrentThread.ManagedThreadId.ToString();
            //}

            //if (string.IsNullOrEmpty(messageObj.TimeStamp))
            //{
            //    messageObj.TimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            //}

            //if (string.IsNullOrEmpty(messageObj.Project))
            //{
            //    messageObj.Project = Process.GetCurrentProcess().ProcessName;
            //}

            //if (string.IsNullOrEmpty(messageObj.User))
            //{
            //    messageObj.User = string.Format(@"{0}___{1}\{2}", Environment.MachineName, Environment.UserDomainName, Environment.UserName);
            //}
        }

        private string GetMessageFromObj(LogMessage messageObj)
        {
            string msg = string.Format("[PID:{0}][TID:{1}] | [Project:{2}][Process:{3}] | [User:{4}] | [Category:{5}][Func:{6}] | Message:{7}| Extra:{8}",
                messageObj.PID, messageObj.TID,
                messageObj.Project,
                messageObj.ProcessName,
                messageObj.User,
                messageObj.Category,
                messageObj.FuncName,
                messageObj.Message,
                GetExtra(messageObj.ExtraData));
            return msg;
        }

        private string GetExtra(Dictionary<string, object> extra)
        {
            string msg = string.Empty;
            foreach (var ex in extra)
            {
                msg += string.Format("[{0}: {1}]", ex.Key ?? "Unknown", ex.Value ?? "Unknown");
            }
            return msg;
        }
    }
}
