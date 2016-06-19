using Facebook;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace VMA.CMS.MyAccount.Web.SocialConnect.FacebookConnect
{
    public class FaceBookConnect
    {
        #region Public Variables
        public string facebook_AppID { get; set; }
        public string facebook_AppSecret { get; set; }
        #endregion Public Variables

        #region Private Variables
        private string facebook_urlAuthorize_base = "https://graph.facebook.com/oauth/authorize";
        private string facebook_urlGetToken_base = "https://graph.facebook.com/oauth/access_token";
        #endregion Private Variables

        #region Public Methods
        public void Authorize(string scope)
        {
            string urlAuthorize = this.facebook_urlAuthorize_base;
            urlAuthorize += "?client_id=" + facebook_AppID;
            urlAuthorize += "&redirect_uri=" + Facebook_GetRedirectUri();
            urlAuthorize += "&scope=" + scope;
            HttpContext.Current.Response.Redirect(urlAuthorize, true);
        }
        public string GetAccessToken(string pAuthorizationCode)
        {
            string urlGetAccessToken = this.facebook_urlGetToken_base;
            urlGetAccessToken += "?client_id=" + facebook_AppID;
            urlGetAccessToken += "&client_secret=" + facebook_AppSecret;
            urlGetAccessToken += "&redirect_uri=" + Facebook_GetRedirectUri();
            urlGetAccessToken += "&code=" + pAuthorizationCode;

            string responseData = RequestResponse(urlGetAccessToken); //we write RequestResponse a little later
            if (responseData == "")
            {
                return "";
            }
            NameValueCollection qs = HttpUtility.ParseQueryString(responseData);
            string access_token = qs["access_token"] == null ? "" : qs["access_token"];

            return access_token;
            //(The access_token is valid only from within the site domain specified for our Facebook application)
        }
        public void Post(string accesToken, Dictionary<string, string> data)
        {
            Facebook_WriteWall(accesToken, data);
        }
        #endregion Public Methods

        #region Private Methods
        private string Facebook_GetRedirectUri()
        {
            string urlCurrentPage = HttpContext.Current.Request.Url.AbsoluteUri.IndexOf('?') == -1 ? HttpContext.Current.Request.Url.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri.Substring(0, HttpContext.Current.Request.Url.AbsoluteUri.IndexOf('?'));
            NameValueCollection nvc = new NameValueCollection();
            foreach (string key in HttpContext.Current.Request.QueryString) { if (key != "code") { nvc.Add(key, HttpContext.Current.Request.QueryString[key]); } }
            string qs = "";
            foreach (string key in nvc)
            {
                qs += qs == "" ? "?" : "&";
                qs += key + "=" + nvc[key];
            }
            string redirect_uri = urlCurrentPage + qs; //urlCallback have to be exactly the same each time it is used (that's why the code key is removed)

            return redirect_uri;
        }

        private string RequestResponse(string pUrl)
        {
            HttpWebRequest webRequest = System.Net.WebRequest.Create(pUrl) as HttpWebRequest;
            webRequest.Method = "GET";
            webRequest.ServicePoint.Expect100Continue = false;
            webRequest.Timeout = 20000;

            Stream responseStream = null;
            StreamReader responseReader = null;
            string responseData = "";
            try
            {
                WebResponse webResponse = webRequest.GetResponse();
                responseStream = webResponse.GetResponseStream();
                responseReader = new StreamReader(responseStream);
                responseData = responseReader.ReadToEnd();
            }
            catch (Exception exc)
            {
                HttpContext.Current.Response.Write("<br /><br />ERROR : " + exc.Message);
            }
            finally
            {
                if (responseStream != null)
                {
                    responseStream.Close();
                    responseReader.Close();
                }
            }

            return responseData;
        }

        private void Facebook_WriteWall(string pAccessToken, Dictionary<string, string> data)
        {

            string username = "me";
            string datatype = "feed";
            string urlWriteWall = "https://graph.facebook.com/" + username + "/" + datatype + "?access_token=" + pAccessToken;
            string entityMessage = "message=" + data["message"];
            entityMessage = string.Format("{0}&picture={1}", entityMessage, data["picture"]);
            entityMessage = string.Format("{0}&link={1}", entityMessage, data["link"]);
            entityMessage = string.Format("{0}&caption={1}", entityMessage, data["caption"]);
            entityMessage = string.Format("{0}&name={1}", entityMessage, data["name"]);
            HttpPost(urlWriteWall, entityMessage); //we write HttpPost a little later
        }

        private string HttpPost(string pUrl, string pPostData)
        {
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(pUrl);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(pPostData);
            Stream requestWriter = webRequest.GetRequestStream(); //GetRequestStream
            requestWriter.Write(bytes, 0, bytes.Length);
            requestWriter.Close();

            Stream responseStream = null;
            StreamReader responseReader = null;
            string responseData = "";
            try
            {
                WebResponse webResponse = webRequest.GetResponse();
                responseStream = webResponse.GetResponseStream();
                responseReader = new StreamReader(responseStream);
                responseData = responseReader.ReadToEnd();
            }
            catch (Exception exc)
            {
                throw new Exception("could not post : " + exc.Message);
            }
            finally
            {
                if (responseStream != null)
                {
                    responseStream.Close();
                    responseReader.Close();
                }
            }

            return responseData;
        }
        #endregion Private Methods
    }
   
}



