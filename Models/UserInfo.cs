using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MusicFun_WebAPI.Models
{
    public class UserInfo
    {
        public System.Guid id { get; set; }
        public string email { get; set; }
        public string userName { get; set; }
    }
}