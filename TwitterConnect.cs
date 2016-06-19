using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace VMA.CMS.MyAccount.Web.SocialConnect
{
    public class TwitterConnect
    {
        public static string API_Key
        {
            get;
            set;
        }

        public static string API_Secret
        {
            get;
            set;
        }

        public string CallBackUrl
        {
            get
            {
                return (HttpContext.Current.Session["CallBackUrl"] == null) ? null : HttpContext.Current.Session["CallBackUrl"].ToString();
            }
            set
            {
                HttpContext.Current.Session["CallBackUrl"] = value;
            }
        }

        public string OAuthToken
        {
            get
            {
                return (HttpContext.Current.Session["OAuthToken"] == null) ? null : HttpContext.Current.Session["OAuthToken"].ToString();
            }
            set
            {
                HttpContext.Current.Session["OAuthToken"] = value;
            }
        }

        public string OAuthTokenSecret
        {
            get
            {
                return (HttpContext.Current.Session["OAuthTokenSecret"] == null) ? null : HttpContext.Current.Session["OAuthTokenSecret"].ToString();
            }
            set
            {
                HttpContext.Current.Session["OAuthTokenSecret"] = value;
            }
        }

        public static bool IsAuthorized
        {
            get
            {
                return !HttpContext.Current.Request.QueryString["oauth_token"].IsEmpty();
            }
        }

        public static bool IsDenied
        {
            get
            {
                return !HttpContext.Current.Request.QueryString["denied"].IsEmpty();
            }
        }

        public void Authorize(string callBackUrl)
        {
            this.OAuthToken = null;
            this.OAuthTokenSecret = null;
            this.CallBackUrl = callBackUrl;
            TwitterAuth twitterAuth = new TwitterAuth(TwitterConnect.API_Key, TwitterConnect.API_Secret, this.CallBackUrl);
            HttpContext.Current.Response.Redirect(twitterAuth.AuthorizationLinkGet());
        }

        public void Tweet(string content)
        {
            string url = string.Empty;
            string text = string.Empty;
            TwitterAuth twitterAuth = new TwitterAuth(TwitterConnect.API_Key, TwitterConnect.API_Secret, this.CallBackUrl);
            if (this.OAuthToken == null || this.OAuthTokenSecret == null)
            {
                twitterAuth.AccessTokenGet(HttpContext.Current.Request.QueryString["oauth_token"], HttpContext.Current.Request.QueryString["oauth_verifier"]);
                this.OAuthToken = twitterAuth.TokenSecret;
                this.OAuthTokenSecret = twitterAuth.Token;
            }
            else
            {
                twitterAuth.TokenSecret = this.OAuthToken;
                twitterAuth.Token = this.OAuthTokenSecret;
                twitterAuth.OAuthVerifier = HttpContext.Current.Request.QueryString["oauth_verifier"];
            }
            if (twitterAuth.TokenSecret.Length > 0)
            {
                url = "https://api.twitter.com/1.1/statuses/update.json";
                text = twitterAuth.OAuthWebRequest(TwitterAuth.Method.POST, url, "status=" + TwitterBase.UrlEncode(content));
            }
        }

        public DataTable FetchProfile(string screenName)
        {
            return this.FetchTwitterProfile(screenName);
        }

        public DataTable FetchProfile()
        {
            return this.FetchTwitterProfile(null);
        }

        private DataTable FetchTwitterProfile(string screenName)
        {
            string url = string.Empty;
            string text = string.Empty;
            TwitterAuth twitterAuth = new TwitterAuth(TwitterConnect.API_Key, TwitterConnect.API_Secret, this.CallBackUrl);
            if (this.OAuthToken == null || this.OAuthTokenSecret == null)
            {
                twitterAuth.AccessTokenGet(HttpContext.Current.Request.QueryString["oauth_token"], HttpContext.Current.Request.QueryString["oauth_verifier"]);
                this.OAuthToken = twitterAuth.TokenSecret;
                this.OAuthTokenSecret = twitterAuth.Token;
            }
            else
            {
                twitterAuth.TokenSecret = this.OAuthToken;
                twitterAuth.Token = this.OAuthTokenSecret;
                twitterAuth.OAuthVerifier = HttpContext.Current.Request.QueryString["oauth_verifier"];
            }
            if (twitterAuth.TokenSecret.Length > 0)
            {
                url = "https://api.twitter.com/1.1/users/show.json";
                if (screenName == null)
                {
                    screenName = twitterAuth.ScreenName;
                }
                text = twitterAuth.OAuthWebRequest(TwitterAuth.Method.GET, url, string.Format("screen_name={0}", screenName));
                try
                {
                    using (DataSet dataSet = new DataSet())
                    {
                        string text2 = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><twitter>";
                        string[] array = text.Split(new string[]
                        {
                            ",\""
                        }, StringSplitOptions.None);
                        string[] array2 = array;
                        for (int i = 0; i < array2.Length; i++)
                        {
                            string text3 = array2[i];
                            string arg = text3.Split(new string[]
                            {
                                "\":"
                            }, StringSplitOptions.None)[0].Replace("\"", string.Empty).Replace("{", string.Empty);
                            string text4 = text3.Split(new string[]
                            {
                                "\":"
                            }, StringSplitOptions.None)[1].Replace("\"", string.Empty).Replace("}", string.Empty).Replace("\\", string.Empty);
                            if (!text4.StartsWith("{") && !text4.StartsWith("["))
                            {
                                text4 = text4.Replace("null", string.Empty).Replace("[]", string.Empty);
                                text2 += string.Format("<{0}>{1}</{0}>", arg, text4);
                            }
                        }
                        text2 += "</twitter>";
                        using (StringReader stringReader = new StringReader(text2))
                        {
                            dataSet.ReadXml(stringReader);
                        }
                        DataTable dataTable = dataSet.Tables["twitter"].Copy();
                        foreach (DataTable dataTable2 in dataSet.Tables)
                        {
                            if (dataTable2.TableName != "twitter")
                            {
                                string text5 = dataTable2.Rows[0][0].ToString();
                                if (!text5.Contains("{") && !text5.Contains("}"))
                                {
                                    dataTable.Columns.Add(dataTable2.TableName);
                                    dataTable.Rows[0][dataTable2.TableName] = text5;
                                }
                            }
                        }
                        dataTable.PrimaryKey = null;
                        if (dataTable.Columns.IndexOf("twitter_id") != -1)
                        {
                            dataTable.Columns.Remove(dataTable.Columns["twitter_id"]);
                        }
                        dataTable.Columns["id"].SetOrdinal(0);
                        return dataTable;
                    }
                }
                catch
                {
                    throw new Exception("An error occured while parsing the Twitter response.");
                }
            }
            throw new Exception("Invalid Twitter token.");
        }
    }
}

