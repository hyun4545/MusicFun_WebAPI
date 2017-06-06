using Jose;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using MusicFun_WebAPI.Factory;
using MusicFun_WebAPI.Filter;
using MusicFun_WebAPI.Models;

namespace MusicFun_WebAPI.Controllers
{

    [RoutePrefix("api/Account")]
    public class DefaultController : ApiController
    {

        private const string FTP_URL = "ftp://waws-prod-cq1-005.ftp.azurewebsites.windows.net";
        private const string FTP_USERNAME = "WebApplication5020160822045658\\$WebApplication5020160822045658";
        private const string FTP_PASSWORD = "P0MqrtNPMahsRH7AoNdcaMjnkkdyx8gz2apkkqAhNmtoSzjoGFBTXXNC8Frk";
        public static byte[] secretKey = Convert.FromBase64String("wedeafef");
        private MusicFunEntities1 db = new MusicFunEntities1();
        Dictionary<string, string> encode_result { get { return (Dictionary<string, string>)Request.Properties["EncodeResult"]; } }
        private UserInfo user
        {
            get
            {
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    return (HttpContext.Current.User.Identity as UserIdentity).userInfo;
                }
                else {
                    return null;
                }
            }
        }

        private string user_id
        {
            get
            {
                return user.id.ToString();
            }
        }
        HttpResponseMessage result = new HttpResponseMessage();


       
        [HttpPost, Route("AddList"), EncodeFilter(form_names = new string[] { "list_name" }), TokenFilter]
        public HttpResponseMessage addMusicList()
        {
            string list_name = encode_result["list_name"];
            if (list_name != null)
            {
                MusicLists musicLists = new MusicLists { user_id = user_id, list_name = list_name };
                db.MusicLists.Add(musicLists);
                db.SaveChanges();
                result = new HttpResponseMessage(HttpStatusCode.OK);
            }
            else {
                result = new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            return result;

        }
        [HttpPost, Route("AddListSong"), TokenFilter]
        public HttpResponseMessage addMusicToList()
        {
            string list_id = HttpContext.Current.Request.Form["list_id"];
            string song_id = HttpContext.Current.Request.Form["song_id"];
            int sid = int.Parse(song_id);
            int lid = int.Parse(list_id);
            if ((sid != 0) && (lid != 0))
            {
                MusicList musicList = new MusicList { song_id = sid, list_id = lid };
                db.MusicList.Add(musicList);
                db.SaveChanges();
                result = new HttpResponseMessage(HttpStatusCode.OK);
            }
            else {
                result = new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            return result;

        }
        [HttpPost, Route("DeleteListSong"), TokenFilter]
        public HttpResponseMessage deleteListSong()
        {
            string list_id = HttpContext.Current.Request.Form["list_id"];
            string song_id = HttpContext.Current.Request.Form["song_id"];
            int sid = int.Parse(song_id);
            int lid = int.Parse(list_id);
            if ((sid != 0) && (lid != 0))
            {
                MusicList musicList = db.MusicList.Where(a => a.list_id == lid && a.song_id == sid).FirstOrDefault();
                db.MusicList.Remove(musicList);
                db.SaveChanges();
                result = new HttpResponseMessage(HttpStatusCode.OK);
            }
            else {
                result = new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            return result;

        }
        [HttpPost, Route("DeleteList"), TokenFilter]
        public HttpResponseMessage deleteList()
        {
            string list_id = HttpContext.Current.Request.Form["list_id"];

            int lid = int.Parse(list_id);
            if (lid != 0)
            {
                MusicLists musicLists = db.MusicLists.Where(a => a.Id == lid).FirstOrDefault();
                db.MusicLists.Remove(musicLists);
                List<MusicList> musicList = db.MusicList.Where(a => a.list_id == lid).ToList();
                db.MusicList.RemoveRange(musicList);
                db.SaveChanges();
                result = new HttpResponseMessage(HttpStatusCode.OK);
            }
            else {
                result = new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            return result;

        }
        [HttpGet, Route("MusicLists"), TokenFilter]
        public HttpResponseMessage getMusicLists()
        {
            List<MusicLists> musicLists = db.MusicLists.Where(a => a.user_id == user_id).ToList();
            result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ObjectContent<List<MusicLists>>(musicLists, new JsonMediaTypeFormatter(), "application/json");
            return result;
        }
        [HttpPost, Route("ListSong"), TokenFilter]
        public HttpResponseMessage getListMusic()
        {
            string list_id = HttpContext.Current.Request.Form["list_id"];
            int lid = int.Parse(list_id);
            if (lid != 0)
            {
                List<MusicList> musicList = db.MusicList.Where(a => a.list_id == lid).ToList();
                result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new ObjectContent<List<MusicList>>(musicList, new JsonMediaTypeFormatter(), "application/json");

            }
            else {
                result = new HttpResponseMessage(HttpStatusCode.NoContent);
            }
            return result;
        }

        [Route("LatestList")]
        [HttpGet]
        [TokenFilter]
        public HttpResponseMessage getLatestList()
        {

            List<SongInfo> list = db.SongInfo.Where(a => a.user_id == user_id).OrderBy(a => a.add_time).ToList();
            list = list.Skip(Math.Max(0, list.Count() - 5)).ToList();
            result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ObjectContent<List<SongInfo>>(list, new JsonMediaTypeFormatter(), "application/json");
            return result;

        }

        [Route("AlwaysList")]
        [HttpGet]
        [TokenFilter]
        public HttpResponseMessage getAlwaysList()
        {

            List<SongInfo> list = db.SongInfo.Where(a => a.user_id == user_id).OrderBy(a => a.download_times).ToList();
            list = list.Skip(Math.Max(0, list.Count() - 5)).ToList();
            result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ObjectContent<List<SongInfo>>(list, new JsonMediaTypeFormatter(), "application/json");
            return result;

        }

        [Route("GetListSong")]
        [HttpPost]
        [TokenFilter]
        public HttpResponseMessage getListSong()
        {
            int list_id = int.Parse(HttpContext.Current.Request.Form["list_id"]);
            List<MusicList> music_list = db.MusicList.Where(a => a.list_id == list_id).ToList();
            List<SongInfo> songInfos = db.SongInfo.Where(c => db.MusicList.Where(a => a.list_id == list_id).Any(a => a.song_id == c.Id)).ToList();
            result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ObjectContent<List<SongInfo>>(songInfos, new JsonMediaTypeFormatter(), "application/json");
            return result;
        }

        [Route("CurrentList")]
        [HttpGet]
        [TokenFilter]
        public HttpResponseMessage getCurrentList()
        {
            List<SongInfo> list = db.SongInfo.Where(a => a.user_id == user_id).OrderBy(a => a.last_time).ToList();
            list = list.Skip(Math.Max(0, list.Count() - 5)).ToList();
            result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ObjectContent<List<SongInfo>>(list, new JsonMediaTypeFormatter(), "application/json");
            return result;

        }

        [Route("Register")]
        [HttpPost]
        public MyResponse Register2(member model)
        {
            Guid g = System.Guid.NewGuid();

            if (model.email != "" && model.password != "")
            {
                if (model.name != "")
                {
                    try
                    {
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(FTP_URL + "/music/" + g);
                        request.Method = WebRequestMethods.Ftp.MakeDirectory;
                        request.Credentials = new NetworkCredential(FTP_USERNAME, FTP_PASSWORD);
                        request.GetResponse();
                    }
                    catch (Exception)
                    {
                        return new MyResponse { message = "發生錯誤，請重新登入!!", status = MyResponse.ERROR_STATUS }; ;
                    }
                    var user = new member() { id = g, name = model.name, email = model.email, password = model.password };
                    try
                    {
                        db.member.Add(user);
                        db.SaveChanges();
                    }
                    catch (DbUpdateException)
                    {
                        return new MyResponse { message = "此帳號已註冊!!", status = MyResponse.ERROR_STATUS };

                    }
                    catch (SqlException)
                    {
                        return new MyResponse { message = "註冊失敗!!", status = MyResponse.ERROR_STATUS };
                    }
                }
                else
                {
                    return new MyResponse { message = "匿名不能為空!!", status = MyResponse.ERROR_STATUS };
                }

            }
            else
            {
                return new MyResponse { message = "帳號、密碼不能為空!!", status = MyResponse.ERROR_STATUS };
            }


            member mm = db.member.Where(c => c.id == g).FirstOrDefault();
            string token = TokenFactory.TokenEncoder(ToUserInfo(mm));
            return new MyResponse { message = token, status = MyResponse.SUCCESS_STATUS }; ;
        }

        [Route("Login")]
        [HttpPost]
        public MyResponse Post(LoginInfo login_info)
        {

            member mm = db.member.Where(c => c.email == login_info.email).FirstOrDefault();
            if (mm == null)
            {
                return new MyResponse { message = "帳號錯誤!!", status = MyResponse.ERROR_STATUS };
            }
            else {
                if (mm.password == login_info.password)
                {
                    string token = TokenFactory.TokenEncoder(ToUserInfo(mm));
                    return new MyResponse { message = token, status = MyResponse.SUCCESS_STATUS };
                }
                else {
                    return new MyResponse { message = "密碼不正確!!", status = MyResponse.ERROR_STATUS };
                }
            }


        }


        [HttpGet]
        [TokenFilter]
        [Route("GetStream/{id}")]
        public HttpResponseMessage Get(int id)
        {



            SongInfo songInfo = db.SongInfo.Where(c => c.Id == id).FirstOrDefault();
            if (songInfo.user_id != user_id)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(FTP_URL + "/music/" + user_id + "/" + songInfo.file_name);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            request.Credentials = new NetworkCredential(FTP_USERNAME, FTP_PASSWORD);
            try
            {

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = songInfo.file_name;
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                songInfo.download_times++;
                songInfo.last_time = DateTime.Now;
                db.Entry(songInfo).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return result;
            }
            catch (Exception)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }


        }
        
        [HttpPost, Route("Modify/{id}")]
        [TokenFilter]
        [EncodeFilter(form_names = new string[] { "author", "song_name" })]
        public HttpResponseMessage Modify(int id)
        {
            string author = encode_result["author"];
            string song_name = encode_result["song_name"];


            SongInfo songInfo = db.SongInfo.Where(c => c.Id == id).FirstOrDefault();
            if (songInfo.user_id != user_id)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            else {
                songInfo.song_title = song_name;
                songInfo.author = author;
                db.Entry(songInfo).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return new HttpResponseMessage(HttpStatusCode.OK);
            }




        }

        [HttpGet]
        [Route("Delete/{id}")]
        [TokenFilter]
        public HttpResponseMessage Delete(int id)
        {


            SongInfo songInfo = db.SongInfo.Where(c => c.Id == id).FirstOrDefault();
            if (songInfo != null)
            {
                if (songInfo.user_id != user_id)
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }


                db.SongInfo.Remove(songInfo);
                db.SaveChanges();
                result = new HttpResponseMessage(HttpStatusCode.OK);
                return result;
            }
            else {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }



        }
        [HttpGet]
        [Route("ListMusic")]
        [TokenFilter]
        public HttpResponseMessage ListMusic()
        {


            List<SongInfo> song_list = db.SongInfo.Where(c => c.user_id == user_id).ToList<SongInfo>();
            result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ObjectContent<List<SongInfo>>(song_list, new JsonMediaTypeFormatter(), "application/json");
            return result;


        }       


        [HttpPost, Route("GetAuthor")]
        public string GetAuthor()
        {
            string author = HttpContext.Current.Request.Form["author"];
            string song_name = HttpContext.Current.Request.Form["song_name"];
            string iid = HttpContext.Current.Request.Form["idd"];
            int idid = int.Parse(iid);
            idid += 1000;
            return author + "   " + song_name + "   " + idid;
        }



        [HttpPost, Route("upload3")]
        [TokenFilter]
        public async Task<HttpResponseMessage> Upload3()
        {

            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            byte[] b = new byte[128];
            string author = HttpContext.Current.Request.Form["author"];
            string song_name = HttpContext.Current.Request.Form["song_name"];
            string song_duration = HttpContext.Current.Request.Form["song_duration"];
            author = HttpUtility.UrlDecode(author, System.Text.Encoding.UTF8);
            song_name = HttpUtility.UrlDecode(song_name, System.Text.Encoding.UTF8);
            if (HttpContext.Current.Request.Files != null)
            {
                HttpFileCollection files = HttpContext.Current.Request.Files;
                for (int i = 0; i < files.Count; i++)
                {
                    if (files[i].ContentLength <= 0)
                    {
                        continue;
                    }
                    string filename = HttpUtility.UrlDecode(files[i].FileName, System.Text.Encoding.UTF8);


                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(FTP_URL + "/music/" + user_id + "/" + filename);
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = new NetworkCredential(FTP_USERNAME, FTP_PASSWORD);

                    request.ContentLength = files[i].ContentLength;
                    Stream requsetStream = request.GetRequestStream();

                    await files[i].InputStream.CopyToAsync(requsetStream);
                    requsetStream.Close();
                    SongInfo song_info = new SongInfo();
                    song_info.song_title = song_name;
                    if (author != null)
                    {
                        song_info.author = author;
                    }
                    song_info.user_id = user_id;
                    song_info.file_name = filename;
                    song_info.song_time = song_duration;
                    song_info.add_time = DateTime.Now;
                    song_info.last_time = DateTime.Now;
                    song_info.download_times = 0;
                    db.SongInfo.Add(song_info);
                    db.SaveChanges();

                }


                return Request.CreateResponse(HttpStatusCode.OK);

            }
            else {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

        }
        [HttpGet, Route("getUser")]
        [TokenFilter]
        public string getUser()
        {
          return  user.userName;
        }
        private UserInfo ToUserInfo(member m)
        {
            UserInfo userInfo = new UserInfo { id = m.id, email = m.email, userName = m.name };
            return userInfo;
        }


    }
}
