using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MusicFun_WebAPI.Controllers
{
    public class MyResponse
    {
        public static int ERROR_STATUS = 0;
        public static int SUCCESS_STATUS = 1;
        public int status { get; set; }
        public string message { get; set; }
        public string name { get; set; }
    }
}