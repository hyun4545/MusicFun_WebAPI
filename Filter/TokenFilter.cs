using Jose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using MusicFun_WebAPI.Controllers;
using MusicFun_WebAPI.Factory;
using MusicFun_WebAPI.Models;

namespace MusicFun_WebAPI.Filter
{
    public class TokenFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
        const string HEADER_NAME = "access_token";
        const string ANTI_TOKEN_NAME = "anti_token";
        private MusicFunEntities1 db = new MusicFunEntities1();
        public override void OnActionExecuting(HttpActionContext actionContext)
        {

            if (actionContext.Request.Headers.Contains(HEADER_NAME))
            {

                string token = actionContext.Request.Headers.GetValues(HEADER_NAME).FirstOrDefault();
                if (!String.IsNullOrEmpty(token))
                {
                    try
                    {
                        UserInfo userInfo = CheckUser(token);
                        UserIdentity identity = new UserIdentity(userInfo);
                        UserPrincipal principal = new UserPrincipal(identity);
                        HttpContext.Current.User = principal;
                    }
                    catch
                    {
                        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized));
                    }



                }
                else {


                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized));
                }
            }
            else {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            }
            base.OnActionExecuting(actionContext);

        }

        UserInfo CheckUser(string token)
        {
            UserInfo user_info = TokenFactory.TokenDecoder(token);
            if (user_info == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            };
            member mm = db.member.Where(c => c.id == user_info.id).FirstOrDefault();
            if (mm != null && mm.email == user_info.email)
            {
                return new UserInfo { id = mm.id, email = mm.email, userName = mm.name };
            }
            else
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            }
        }
    }
}