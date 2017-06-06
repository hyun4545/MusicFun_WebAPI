using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace MusicFun_WebAPI.Models
{
    public class UserPrincipal : GenericPrincipal
    {
        public UserPrincipal(UserIdentity identity) : base(identity, new string[0]) { }
    }
}