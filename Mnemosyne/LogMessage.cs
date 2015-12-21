using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mnemosyne
{
    public class LogMessage
    {
        public string Category { get; set; }
        public string User { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string FuncName { get; set; }
        public string Project { get; set; }
        public string ProcessName { get; set; }
        public string PID { get; set; }
        public string TID { get; set; }
        public string TimeStamp { get; set; }
        public Dictionary<string, object> ExtraData { get; set; }

        public LogMessage()
        {
            try
            {
                this.PID = GetPID();
                this.TID = GetTID();
                this.TimeStamp = GetTimeStamp();
                this.ProcessName = GetProcessName();
                this.Project = GetProject();
                this.User = GetUser();
                this.FuncName = GetFuncName();
                this.ExtraData = new Dictionary<string, object>();
            }
            catch { }
        }

        public LogMessage(string message)
        {
            try
            {
                this.Message = message;
                this.PID = GetPID();
                this.TID = GetTID();
                this.TimeStamp = GetTimeStamp();
                this.ProcessName = GetProcessName();
                this.Project = GetProject();
                this.User = GetUser();
                this.FuncName = GetFuncName();
                this.ExtraData = new Dictionary<string, object>();
            }
            catch { }
        }

        public LogMessage(string user, string message)
        {
            this.User = user;
            this.Message = message;
            try
            {
                this.PID = GetPID();
                this.TID = GetTID();
                this.TimeStamp = GetTimeStamp();
                this.ProcessName = GetProcessName();
                this.Project = GetProject();
                this.User = GetUser();
                this.FuncName = GetFuncName();
                this.ExtraData = new Dictionary<string, object>();
            }
            catch { }
        }

        private string GetTID()
        {
            try
            {
                return Thread.CurrentThread.ManagedThreadId.ToString();
            }
            catch { }
            return string.Empty;
        }

        private string GetPID()
        {
            try
            {
                return Process.GetCurrentProcess().Id.ToString();
            }
            catch { }
            return string.Empty;
        }

        private string GetTimeStamp()
        {
            try
            {
                return DateTime.Now.ToString("yyyyMMddHHmmssfff");
            }
            catch { }
            return string.Empty;
        }

        private string GetProcessName()
        {
            try
            {
                return Process.GetCurrentProcess().ProcessName;
            }
            catch { }
            return string.Empty;
        }

        private string GetUser()
        {
            try
            {
                return string.Format(@"{0}___{1}\{2}", Environment.MachineName, Environment.UserDomainName, Environment.UserName);
            }
            catch { }
            return string.Empty;
        }

        private string GetFuncName()
        {
            try
            {
                return new StackTrace().GetFrame(2).GetMethod().Name;
            }
            catch { }
            return string.Empty;
        }

        private string GetProject()
        {
            try
            {
                return new StackTrace().GetFrame(2).GetMethod().DeclaringType.ToString();
            }
            catch { }
            return string.Empty;
        }
    }
}
