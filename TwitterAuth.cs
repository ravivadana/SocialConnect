using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace VMA.CMS.MyAccount.Web.SocialConnect
{
    internal class TwitterAuth : TwitterBase
    {
        public enum Method
        {
            GET,
            POST,
            DELETE
        }

        public const string RequestToken = "https://api.twitter.com/oauth/request_token";

        public const string Authorize = "https://api.twitter.com/oauth/authorize";

        public const string AccessToken = "https://api.twitter.com/oauth/access_token";

        private string consumerKey = string.Empty;

        private string consumerSecret = string.Empty;

        private string token = string.Empty;

        private string tokenSecret = string.Empty;

        private string screenName = string.Empty;

        private string twitterId = string.Empty;

        private string callBackUrl = "oob";

        private string oauthVerifier = string.Empty;

        public string ConsumerKey
        {
            get
            {
                return this.consumerKey;
            }
            set
            {
                this.consumerKey = value;
            }
        }

        public string ConsumerSecret
        {
            get
            {
                return this.consumerSecret;
            }
            set
            {
                this.consumerSecret = value;
            }
        }

        public string Token
        {
            get
            {
                return this.token;
            }
            set
            {
                this.token = value;
            }
        }

        public string TokenSecret
        {
            get
            {
                return this.tokenSecret;
            }
            set
            {
                this.tokenSecret = value;
            }
        }

        public string ScreenName
        {
            get
            {
                return this.screenName;
            }
            set
            {
                this.screenName = value;
            }
        }

        public string TwitterId
        {
            get
            {
                return this.twitterId;
            }
            set
            {
                this.twitterId = value;
            }
        }

        public string CallBackUrl
        {
            get
            {
                return this.callBackUrl;
            }
            set
            {
                this.callBackUrl = value;
            }
        }

        public string OAuthVerifier
        {
            get
            {
                return this.oauthVerifier;
            }
            set
            {
                this.oauthVerifier = value;
            }
        }

        internal TwitterAuth(string key, string secret, string callBackUrl)
        {
            this.ConsumerKey = key;
            this.ConsumerSecret = secret;
            this.CallBackUrl = callBackUrl;
        }

        public string AuthorizationLinkGet()
        {
            string result = null;
            string text = this.OAuthWebRequest(TwitterAuth.Method.GET, "https://api.twitter.com/oauth/request_token", string.Empty);
            if (text.Length > 0)
            {
                NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(text);
                if (nameValueCollection["oauth_callback_confirmed"] != null)
                {
                    if (nameValueCollection["oauth_callback_confirmed"] != "true")
                    {
                        throw new Exception("OAuth callback not confirmed.");
                    }
                }
                if (nameValueCollection["oauth_token"] != null)
                {
                    result = "https://api.twitter.com/oauth/authorize?oauth_token=" + nameValueCollection["oauth_token"];
                }
            }
            return result;
        }

        public void AccessTokenGet(string authToken, string oauthVerifier)
        {
            this.Token = authToken;
            this.OAuthVerifier = oauthVerifier;
            string text = this.OAuthWebRequest(TwitterAuth.Method.GET, "https://api.twitter.com/oauth/access_token", string.Empty);
            if (text.Length > 0)
            {
                NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(text);
                if (nameValueCollection["oauth_token"] != null)
                {
                    this.Token = nameValueCollection["oauth_token"];
                }
                if (nameValueCollection["oauth_token_secret"] != null)
                {
                    this.TokenSecret = nameValueCollection["oauth_token_secret"];
                }
                if (nameValueCollection["screen_name"] != null)
                {
                    this.ScreenName = nameValueCollection["screen_name"];
                }
                if (nameValueCollection["user_id"] != null)
                {
                    this.TwitterId = nameValueCollection["user_id"];
                }
            }
        }

        public string OAuthWebRequest(TwitterAuth.Method method, string url, string postData)
        {
            string str = string.Empty;
            string text = string.Empty;
            string empty = string.Empty;
            if (method == TwitterAuth.Method.POST || method == TwitterAuth.Method.DELETE || method == TwitterAuth.Method.GET)
            {
                if (postData.Length > 0)
                {
                    NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(postData);
                    postData = string.Empty;
                    string[] allKeys = nameValueCollection.AllKeys;
                    for (int i = 0; i < allKeys.Length; i++)
                    {
                        string text2 = allKeys[i];
                        if (postData.Length > 0)
                        {
                            postData += "&";
                        }
                        nameValueCollection[text2] = HttpUtility.UrlDecode(nameValueCollection[text2]);
                        nameValueCollection[text2] = TwitterBase.UrlEncode(nameValueCollection[text2]);
                        postData = postData + text2 + "=" + nameValueCollection[text2];
                    }
                    if (url.IndexOf("?") > 0)
                    {
                        url += "&";
                    }
                    else
                    {
                        url += "?";
                    }
                    url += postData;
                }
            }
            Uri url2 = new Uri(url);
            string nonce = this.GenerateNonce();
            string timeStamp = this.GenerateTimeStamp();
            string value = base.GenerateSignature(url2, this.ConsumerKey, this.ConsumerSecret, this.Token, this.TokenSecret, this.CallBackUrl, this.OAuthVerifier, method.ToString(), timeStamp, nonce, out str, out text);
            text = text + "&oauth_signature=" + TwitterBase.UrlEncode(value);
            if (method == TwitterAuth.Method.POST || method == TwitterAuth.Method.DELETE)
            {
                postData = text;
                text = string.Empty;
            }
            if (text.Length > 0)
            {
                str += "?";
            }
            return this.WebRequest(method, str + text, postData);
        }

        public string WebRequest(TwitterAuth.Method method, string url, string postData)
        {
            HttpWebRequest httpWebRequest = null;
            StreamWriter streamWriter = null;
            string result = string.Empty;
            httpWebRequest = (System.Net.WebRequest.Create(url) as HttpWebRequest);
            httpWebRequest.Method = method.ToString();
            httpWebRequest.ServicePoint.Expect100Continue = false;
            if (method == TwitterAuth.Method.POST || method == TwitterAuth.Method.DELETE)
            {
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                streamWriter = new StreamWriter(httpWebRequest.GetRequestStream());
                try
                {
                    streamWriter.Write(postData);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    streamWriter.Close();
                    streamWriter = null;
                }
            }
            result = this.WebResponseGet(httpWebRequest);
            httpWebRequest = null;
            return result;
        }

        public string WebResponseGet(HttpWebRequest webRequest)
        {
            StreamReader streamReader = null;
            string result = string.Empty;
            try
            {
                streamReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                result = streamReader.ReadToEnd();
            }
            catch
            {
                throw;
            }
            finally
            {
                webRequest.GetResponse().GetResponseStream().Close();
                streamReader.Close();
                streamReader = null;
            }
            return result;
        }
    }
}
