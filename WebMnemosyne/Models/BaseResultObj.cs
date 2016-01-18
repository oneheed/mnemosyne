using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebMnemosyne.Models
{
    public class BaseResultObj
    {
        public bool Success { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string Result { get; set; }
    }
}