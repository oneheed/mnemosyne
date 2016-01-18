using Mnemosyne;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;

namespace WebMnemosyne.Models
{
    public class InputDataObj
    {
        public string Token { get; set; }
        public JObject InputData { get; set; }
        public LogMessage LogMessage
        {
            get
            {
                return JsonConvert.DeserializeObject<LogMessage>(this.InputData.ToString());
            }
        }
    }
}