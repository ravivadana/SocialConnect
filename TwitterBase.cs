using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace VMA.CMS.MyAccount.Web.SocialConnect
{
    internal class TwitterBase
    {
        public enum SignatureTypes
        {
            HMACSHA1,
            PLAINTEXT,
            RSASHA1
        }

        protected class QueryParameter
        {
            private string name = null;

            private string value = null;

            public string Name
            {
                get
                {
                    return this.name;
                }
            }

            public string Value
            {
                get
                {
                    return this.value;
                }
            }

            public QueryParameter(string name, string value)
            {
                this.name = name;
                this.value = value;
            }
        }

        protected class QueryParameterComparer : IComparer<TwitterBase.QueryParameter>
        {
            public int Compare(TwitterBase.QueryParameter x, TwitterBase.QueryParameter y)
            {
                int result;
                if (x.Name == y.Name)
                {
                    result = string.Compare(x.Value, y.Value);
                }
                else
                {
                    result = string.Compare(x.Name, y.Name);
                }
                return result;
            }
        }

        protected const string OAuthVersion = "1.0";

        protected const string OAuthParameterPrefix = "oauth_";

        protected const string OAuthConsumerKeyKey = "oauth_consumer_key";

        protected const string OAuthCallbackKey = "oauth_callback";

        protected const string OAuthVersionKey = "oauth_version";

        protected const string OAuthSignatureMethodKey = "oauth_signature_method";

        protected const string OAuthSignatureKey = "oauth_signature";

        protected const string OAuthTimestampKey = "oauth_timestamp";

        protected const string OAuthNonceKey = "oauth_nonce";

        protected const string OAuthTokenKey = "oauth_token";

        protected const string OAuthTokenSecretKey = "oauth_token_secret";

        protected const string OAuthVerifierKey = "oauth_verifier";

        protected const string HMACSHA1SignatureType = "HMAC-SHA1";

        protected const string PlainTextSignatureType = "PLAINTEXT";

        protected const string RSASHA1SignatureType = "RSA-SHA1";

        protected Random random = new Random();

        protected static string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        public static string UrlEncode(string value)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (TwitterBase.unreservedChars.IndexOf(c) != -1)
                {
                    stringBuilder.Append(c);
                }
                else
                {
                    stringBuilder.Append('%' + string.Format("{0:X2}", (int)c));
                }
            }
            return stringBuilder.ToString();
        }

        protected string NormalizeRequestParameters(IList<TwitterBase.QueryParameter> parameters)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < parameters.Count; i++)
            {
                TwitterBase.QueryParameter queryParameter = parameters[i];
                stringBuilder.AppendFormat("{0}={1}", queryParameter.Name, queryParameter.Value);
                if (i < parameters.Count - 1)
                {
                    stringBuilder.Append("&");
                }
            }
            return stringBuilder.ToString();
        }

        public string GenerateSignatureBase(Uri url, string consumerKey, string token, string tokenSecret, string callBackUrl, string oauthVerifier, string httpMethod, string timeStamp, string nonce, string signatureType, out string normalizedUrl, out string normalizedRequestParameters)
        {
            if (token == null)
            {
                token = string.Empty;
            }
            if (tokenSecret == null)
            {
                tokenSecret = string.Empty;
            }
            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException("consumerKey");
            }
            if (string.IsNullOrEmpty(httpMethod))
            {
                throw new ArgumentNullException("httpMethod");
            }
            if (string.IsNullOrEmpty(signatureType))
            {
                throw new ArgumentNullException("signatureType");
            }
            normalizedUrl = null;
            normalizedRequestParameters = null;
            List<TwitterBase.QueryParameter> queryParameters = this.GetQueryParameters(url.Query);
            queryParameters.Add(new TwitterBase.QueryParameter("oauth_version", "1.0"));
            queryParameters.Add(new TwitterBase.QueryParameter("oauth_nonce", nonce));
            queryParameters.Add(new TwitterBase.QueryParameter("oauth_timestamp", timeStamp));
            queryParameters.Add(new TwitterBase.QueryParameter("oauth_signature_method", signatureType));
            queryParameters.Add(new TwitterBase.QueryParameter("oauth_consumer_key", consumerKey));
            if (!string.IsNullOrEmpty(callBackUrl))
            {
                queryParameters.Add(new TwitterBase.QueryParameter("oauth_callback", TwitterBase.UrlEncode(callBackUrl)));
            }
            if (!string.IsNullOrEmpty(oauthVerifier))
            {
                queryParameters.Add(new TwitterBase.QueryParameter("oauth_verifier", oauthVerifier));
            }
            if (!string.IsNullOrEmpty(token))
            {
                queryParameters.Add(new TwitterBase.QueryParameter("oauth_token", token));
            }
            queryParameters.Sort(new TwitterBase.QueryParameterComparer());
            normalizedUrl = string.Format("{0}://{1}", url.Scheme, url.Host);
            if ((!(url.Scheme == "http") || url.Port != 80) && (!(url.Scheme == "https") || url.Port != 443))
            {
                normalizedUrl = normalizedUrl + ":" + url.Port;
            }
            normalizedUrl += url.AbsolutePath;
            normalizedRequestParameters = this.NormalizeRequestParameters(queryParameters);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0}&", httpMethod.ToUpper());
            stringBuilder.AppendFormat("{0}&", TwitterBase.UrlEncode(normalizedUrl));
            stringBuilder.AppendFormat("{0}", TwitterBase.UrlEncode(normalizedRequestParameters));
            return stringBuilder.ToString();
        }

        public string GenerateSignatureUsingHash(string signatureBase, HashAlgorithm hash)
        {
            return this.ComputeHash(hash, signatureBase);
        }

        public string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string callBackUrl, string oauthVerifier, string httpMethod, string timeStamp, string nonce, out string normalizedUrl, out string normalizedRequestParameters)
        {
            return this.GenerateSignature(url, consumerKey, consumerSecret, token, tokenSecret, callBackUrl, oauthVerifier, httpMethod, timeStamp, nonce, TwitterBase.SignatureTypes.HMACSHA1, out normalizedUrl, out normalizedRequestParameters);
        }

        public string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string callBackUrl, string oauthVerifier, string httpMethod, string timeStamp, string nonce, TwitterBase.SignatureTypes signatureType, out string normalizedUrl, out string normalizedRequestParameters)
        {
            normalizedUrl = null;
            normalizedRequestParameters = null;
            string result;
            switch (signatureType)
            {
                case TwitterBase.SignatureTypes.HMACSHA1:
                    {
                        string signatureBase = this.GenerateSignatureBase(url, consumerKey, token, tokenSecret, callBackUrl, oauthVerifier, httpMethod, timeStamp, nonce, "HMAC-SHA1", out normalizedUrl, out normalizedRequestParameters);
                        result = this.GenerateSignatureUsingHash(signatureBase, new HMACSHA1
                        {
                            Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", TwitterBase.UrlEncode(consumerSecret), string.IsNullOrEmpty(tokenSecret) ? string.Empty : TwitterBase.UrlEncode(tokenSecret)))
                        });
                        break;
                    }
                case TwitterBase.SignatureTypes.PLAINTEXT:
                    result = HttpUtility.UrlEncode(string.Format("{0}&{1}", consumerSecret, tokenSecret));
                    break;
                case TwitterBase.SignatureTypes.RSASHA1:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("Unknown signature type", "signatureType");
            }
            return result;
        }

        public virtual string GenerateTimeStamp()
        {
            return Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds).ToString();
        }

        public virtual string GenerateNonce()
        {
            return this.random.Next(123400, 9999999).ToString();
        }

        private string ComputeHash(HashAlgorithm hashAlgorithm, string data)
        {
            if (hashAlgorithm == null)
            {
                throw new ArgumentNullException("hashAlgorithm");
            }
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException("data");
            }
            byte[] bytes = Encoding.ASCII.GetBytes(data);
            byte[] inArray = hashAlgorithm.ComputeHash(bytes);
            return Convert.ToBase64String(inArray);
        }

        private List<TwitterBase.QueryParameter> GetQueryParameters(string parameters)
        {
            if (parameters.StartsWith("?"))
            {
                parameters = parameters.Remove(0, 1);
            }
            List<TwitterBase.QueryParameter> list = new List<TwitterBase.QueryParameter>();
            if (!string.IsNullOrEmpty(parameters))
            {
                string[] array = parameters.Split(new char[]
                {
                    '&'
                });
                string[] array2 = array;
                for (int i = 0; i < array2.Length; i++)
                {
                    string text = array2[i];
                    if (!string.IsNullOrEmpty(text) && !text.StartsWith("oauth_"))
                    {
                        if (text.IndexOf('=') > -1)
                        {
                            string[] array3 = text.Split(new char[]
                            {
                                '='
                            });
                            list.Add(new TwitterBase.QueryParameter(array3[0], array3[1]));
                        }
                        else
                        {
                            list.Add(new TwitterBase.QueryParameter(text, string.Empty));
                        }
                    }
                }
            }
            return list;
        }
    }
}
