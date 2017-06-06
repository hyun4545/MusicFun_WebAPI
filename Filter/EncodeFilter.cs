using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;

namespace MusicFun_WebAPI.Filter
{
    public class EncodeFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
        public string[] form_names;
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            Dictionary<string, string> encoded_result = new Dictionary<string, string>();
            foreach(string form_name in form_names)
            {
                string result = HttpContext.Current.Request.Form[form_name];
                result = HttpUtility.UrlDecode(result, System.Text.Encoding.UTF8);
                encoded_result.Add(form_name, result);
            }
            actionContext.Request.Properties.Add("EncodeResult", encoded_result);
        }
    }
}