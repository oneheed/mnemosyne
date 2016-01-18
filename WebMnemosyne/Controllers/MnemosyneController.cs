using Mnemosyne;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMnemosyne.Models;
using Newtonsoft.Json.Linq;

namespace WebMnemosyne.Controllers
{
    public class MnemosyneController : Controller
    {
        private static MnemosyneLogger _logger;

        public ActionResult Log(LogMessage inputObj)
        {
            if (_logger == null)
                _logger = new MnemosyneLogger(Path.Combine(Server.MapPath("~"), string.Format(@"{0}.config", "Mnemosyne")));
            try
            {
                if (string.IsNullOrEmpty(inputObj.Message))
                {
                    return Json(new BaseResultObj() { Success = false, Message = "Your message is null or empty" });
                }
                //VWLogger.Debug(Path.Combine(Server.MapPath("~"), string.Format(@"{0}.config", "Mnemosyne")));
                switch (inputObj.Level.ToUpper())
                {
                    case "ERROR":
                        _logger.Error(inputObj);
                        break;
                    case "WARN":
                        _logger.Warn(inputObj);
                        break;
                    case "INFO":
                        _logger.Info(inputObj);
                        break;
                    case "FATAL":
                        _logger.Fatal(inputObj);
                        break;
                    case "TRACE":
                        _logger.Trace(inputObj);
                        break;
                    case "DEBUG":
                    default:
                        _logger.Debug(inputObj);
                        break;
                }
                return Json(new BaseResultObj() { Success = true });
            }
            catch (Exception e)
            {
                return Json(new BaseResultObj() { Success = false, Message = e.Message });
            }
        }

    }
}
