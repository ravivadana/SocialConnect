using Facebook;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using VMA.CMS.MyAccount.Web.SocialConnect.FacebookConnect;

namespace SocialConnectClient
{
    public partial class facebook : System.Web.UI.Page
    {
       
        string facebook_AppID = "255642851472510";
        string facebook_AppSecret = "1bbb3264d9b0048cb653bb0a1c8b3f87";
        FaceBookConnect fbConnect = new FaceBookConnect();
        public facebook()
        {
            fbConnect.facebook_AppID = facebook_AppID;
            fbConnect.facebook_AppSecret = facebook_AppSecret;
        }
        protected void Page_Load(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(Request["code"]))
            {
                string scope = "publish_actions";
                fbConnect.Authorize(scope);
            }
            else
            {
                if (IsPostBack)
                {
                    string code = Request.QueryString["code"];
                    string accessToken = fbConnect.GetAccessToken(code);
                    Dictionary<string, string> data = new Dictionary<string, string>();
                    data.Add("link", "http://www.virginmobile.com.au");
                    data.Add("picture", "https://www.virginmobile.com.au/img/logos/Desktop_Logo_DropShadow_170x160.png");
                    data.Add("caption", "VMA");
                    data.Add("name", "VMA");
                    data.Add("message", txtMessage.Text);
                    fbConnect.Post(accessToken, data);
                }
            }
        }
        
    }
}
