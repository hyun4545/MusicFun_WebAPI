using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace MusicFun_WebAPI.Models
{
    public class UserIdentity : GenericIdentity
    {
        public UserIdentity(UserInfo userInfo) : base(userInfo.userName)
        {
            this.userInfo = userInfo;

        }
        public UserInfo userInfo;

    }
}