using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Infrastructure
{
    public static class Lib
    {
        /// <summary>
        /// GUID(32位)
        /// </summary>
        /// <returns></returns>
        public static string GUID()
        {
            //return Guid.NewGuid().ToString().Replace("-", "");
            return Guid.NewGuid().ToString("N");
        }
        /// <summary>
        /// 获取当前时间戳 
        /// </summary>
        /// <returns></returns>
        public static int GenTimeStamp()
        {
            return Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
        }

        /// <summary>
		/// 生成随机数字（默认6位）
		/// </summary>
        /// <param name="Length"></param>
		/// <returns></returns>
		public static string GenerateNumber(int Length = 6)
        {
            Random ran = new Random((int)DateTime.Now.Ticks % int.MaxValue);
            int minValue = Convert.ToInt32(1 * Math.Pow(10, Length - 1));
            int maxValue = Convert.ToInt32(1 * Math.Pow(10, Length));
            string strRandomResult = ran.Next(minValue, maxValue).ToString(maxValue.ToString().Substring(1, Length));

            return strRandomResult;
        }

        /// <summary>
        /// 生成随机码（默认9位，数字和大写字母混合）
        /// </summary>
        /// <param name="Length"></param>
        /// <returns></returns>
        public static string GetPromoCode(int Length = 9)
        {
            /*char[] pattern = new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0',
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j','k' ,'l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J' ,'K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};*/

            char[] pattern = new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0',
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J' ,'K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};

            string code = "";
            System.Threading.Thread.Sleep(1);
            Random ra = new Random(unchecked((int)DateTime.Now.Ticks));
            for (int i = 0; i < Length; i++)
            {
                int raIndex = ra.Next(0, pattern.Length);
                code += pattern[raIndex];
            }
            return code;
        }

        /// <summary>
        /// 获取文件的MD5值
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string GetMD5HashFromFile(string fileName, string content, out string msg)
        {
            msg = "0";
            if (string.IsNullOrEmpty(fileName) && string.IsNullOrEmpty(content))
                return "";
            try
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    byte[] byFile = File.ReadAllBytes(fileName);
                    //FileStream file = new FileStream(fileName, FileMode.Open);
                    if (!string.IsNullOrEmpty(content))
                    {
                        byte[] conVal = System.Text.Encoding.Default.GetBytes(content);
                        //file.Write(conVal, 0, conVal.Length);
                        byFile = byFile.Concat(conVal).ToArray();
                    }
                    System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    byte[] retVal = md5.ComputeHash(byFile);
                    //file.Close();

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < retVal.Length; i++)
                    {
                        sb.Append(retVal[i].ToString("x2"));
                    }
                    return sb.ToString();
                }
                else
                {
                    System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    byte[] conVal = System.Text.Encoding.Default.GetBytes(content);
                    byte[] retVal = md5.ComputeHash(conVal);

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < retVal.Length; i++)
                    {
                        sb.Append(retVal[i].ToString("x2"));
                    }
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                msg = $"GetMD5HashFromFile() fail,error:{ex.ToString()}";
                return "";
            }
        }

        /// <summary>
        /// 获取文件的MD5值及其文件大小
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string GetMD5HashFromFileWithSize(string fileName, string content, out long fileSize, out string msg)
        {
            msg = "0";
            fileSize = 0;
            if (string.IsNullOrEmpty(fileName) && string.IsNullOrEmpty(content))
                return "";
            try
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    byte[] byFile = File.ReadAllBytes(fileName);
                    //FileStream file = new FileStream(fileName, FileMode.Open);
                    fileSize = byFile.Length;
                    if (!string.IsNullOrEmpty(content))
                    {
                        byte[] conVal = System.Text.Encoding.Default.GetBytes(content);
                        //file.Write(conVal, 0, conVal.Length);
                        byFile = byFile.Concat(conVal).ToArray();
                    }
                    System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    byte[] retVal = md5.ComputeHash(byFile);
                    //file.Close();

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < retVal.Length; i++)
                    {
                        sb.Append(retVal[i].ToString("x2"));
                    }
                    return sb.ToString();
                }
                else
                {
                    System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    byte[] conVal = System.Text.Encoding.Default.GetBytes(content);
                    byte[] retVal = md5.ComputeHash(conVal);

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < retVal.Length; i++)
                    {
                        sb.Append(retVal[i].ToString("x2"));
                    }
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                msg = $"GetMD5HashFromFile() fail,error:{ex.ToString()}";
                return "";
            }
        }

        /// <summary>
        /// 获取oss文件和字符串内容的MD5值 md5(ossfilemd5+content)
        /// </summary>
        /// <param name="ossfilemd5"></param>
        /// <param name="content"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string GetMD5HashFromOSS(string ossfilemd5, string content, out string msg)
        {
            msg = "0";
            if (string.IsNullOrEmpty(ossfilemd5) && string.IsNullOrEmpty(content))
                return "";
            try
            {
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] ossVal = System.Text.Encoding.Default.GetBytes(ossfilemd5);//文件的md5
                byte[] conVal = System.Text.Encoding.Default.GetBytes(content);///content
                byte[] retVal = md5.ComputeHash(ossVal.Concat(conVal).ToArray());

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();

            }
            catch (Exception ex)
            {
                msg = $"GetMD5HashFromOSS() fail,error:{ex.ToString()}";
                return "";
            }
        }


        /// <summary>
        /// 判断字符串是否为base64String
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsBase64String(string str)
        {
            HashSet<char> has = new HashSet<char>(){
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P',
            'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f',
            'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v',
            'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '/',
            '='};
            if (string.IsNullOrEmpty(str))
                return false;

            var reg = new Regex("data:image/(.*);base64,");
            str = reg.Replace(str, "");

            if (str.Any(c => !has.Contains(c)))
                return false;

            try
            {
                Convert.FromBase64String(str);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// 转义 mysql 数据库中正则表达式字符串中的特殊字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EscapeMySqlRegularStr(string str)
        {
            HashSet<char> has = new HashSet<char>() {
                '\\','^','$','[',']','.','(',')','*','+','?','|','{','}'
            };
            foreach (char c in has)
            {
                if (str.Contains(c))
                    str = str.Replace(c.ToString(), $"\\{c}");
            }
            return str;
        }

        /// <summary>
        /// 转义json字符串
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string StringToJson(string s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s.ToCharArray()[i];
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\""); break;
                    case '\\':
                        sb.Append("\\\\"); break;
                    case '/':
                        sb.Append("\\/"); break;
                    case '\b':
                        sb.Append("\\b"); break;
                    case '\f':
                        sb.Append("\\f"); break;
                    case '\n':
                        sb.Append("\\n"); break;
                    case '\r':
                        sb.Append("\\r"); break;
                    case '\t':
                        sb.Append("\\t"); break;
                    default:
                        if ((c >= 0 && c <= 31) || c == 127)//在ASCⅡ码中，第0～31号及第127号(共33个)是控制字符或通讯专用字符
                        {

                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }

        #region //IP帮助类

        /// <summary>
        /// 百度api
        /// </summary>
        /// <returns></returns>
        public static string GetBaiduIp(string ip)
        {
            try
            {
                string cs = "";
                string url = "http://api.map.baidu.com/location/ip?ak=GlfVlFKSc6Y7aSr73IHM3lQI&ip=" + ip;
                WebClient client = new WebClient();
                var buffer = client.DownloadData(url);
                string jsonText = Encoding.UTF8.GetString(buffer);
                JObject jo = JObject.Parse(jsonText);
                var txt = jo["content"]["address_detail"]["city"];
                JToken st = txt;
                string str = st.ToString();
                if (str == "")
                {
                    cs = GetCS(ip);
                    return cs;

                }
                int s = str.IndexOf('市');
                string css = str.Substring(0, s);
                bool bl = HasChinese(css);

                if (bl)
                {
                    cs = css;
                }
                else
                {
                    cs = GetCS(ip);
                }

                return cs;
            }
            catch
            {
                return GetIPCitys(ip);
            }

        }

        /// <summary>
        /// 新浪api
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static string GetCS(string ip)
        {
            try
            {
                string url = "http://int.dpool.sina.com.cn/iplookup/iplookup.php?ip=" + ip;
                WebClient MyWebClient = new WebClient();
                MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据
                Byte[] pageData = MyWebClient.DownloadData(url); //从指定网站下载数据
                string stt = Encoding.GetEncoding("GBK").GetString(pageData).Trim();
                return stt.Substring(stt.Length - 2, 2);
            }
            catch
            {
                return "未知";
            }

        }

        /// <summary>
        /// 淘宝api
        /// </summary>
        /// <param name="strIP"></param>
        /// <returns></returns>
        public static string GetIPCitys(string strIP)
        {
            try
            {
                string Url = "http://ip.taobao.com/service/getIpInfo.php?ip=" + strIP + "";

                System.Net.WebRequest wReq = System.Net.WebRequest.Create(Url);
                wReq.Timeout = 2000;
                System.Net.WebResponse wResp = wReq.GetResponse();
                System.IO.Stream respStream = wResp.GetResponseStream();
                using (System.IO.StreamReader reader = new System.IO.StreamReader(respStream))
                {
                    string jsonText = reader.ReadToEnd();
                    JObject ja = (JObject)JsonConvert.DeserializeObject(jsonText);
                    if (ja["code"].ToString() == "0")
                    {
                        string c = ja["data"]["city"].ToString();
                        int ci = c.IndexOf('市');
                        if (ci != -1)
                        {
                            c = c.Remove(ci, 1);
                        }
                        return c;
                    }
                    else
                    {
                        return "未知";
                    }
                }
            }
            catch (Exception)
            {
                return ("未知");
            }
        }


        /// <summary>
        /// 是否含有中文
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasChinese(string str)
        {
            return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }
        #endregion
    }
}
