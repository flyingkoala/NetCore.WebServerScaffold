using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository;
using log4net.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Infrastructure.Log
{
    public class LogHelper
    {
        private static ILog log;

        public static ILog Log
        {
            get
            {
                if (log == null)
                {
                    List<IAppender> lstAppender = new List<IAppender>();
                    //log to mongodb
                    //log4net.Repository.Hierarchy.Hierarchy hier = LogManager.GetRepository() as log4net.Repository.Hierarchy.Hierarchy;

                    //string connStr = ConfigurationManager.AppSettings["MongoConnection"];

                    //if(!string.IsNullOrEmpty(connStr))
                    //{
                    //    MongoDBAppender mongoDBAppender = new MongoDBAppender();
                    //    mongoDBAppender.ConnectionString = connStr;
                    //    mongoDBAppender.LogName = "HG_OrderServer_Log";
                    //    lstAppender.Add(mongoDBAppender);
                    //}

                    //log to file
                    RollingFileAppender rollFileAppender = new RollingFileAppender();
                    rollFileAppender.File = "logs/";
                    rollFileAppender.AppendToFile = true;
                    rollFileAppender.DatePattern = "yyyy-MM-dd'.txt'";
                    //< !--变换的形式为日期，这种情况下每天只有一个日志-- >
                    //< !--此时MaxSizeRollBackups和maximumFileSize的节点设置没有意义-- >
                    //< !--< rollingStyle value = "Date" /> -->
                    //< !--变换的形式为日志大小-- >
                    //< !--这种情况下MaxSizeRollBackups和maximumFileSize的节点设置才有意义-- >
                    //< !--< RollingStyle value = "Size" /> -- >
                    rollFileAppender.RollingStyle = RollingFileAppender.RollingMode.Composite;
                    rollFileAppender.MaxSizeRollBackups = -1;//
                    rollFileAppender.MaximumFileSize = "2MB";//单个文件最大值

                    rollFileAppender.StaticLogFileName = false;//名称是否可以更改，为false则可以更改
                    PatternLayout patternLayout = new PatternLayout();
                    StringBuilder jsonLogs = new StringBuilder();
                    jsonLogs.Append("{");
                    jsonLogs.Append($"\"主机IP：\":\"{GetIP()}\",");
                    jsonLogs.Append("\"记录时间：\":\"%date\",");
                    jsonLogs.Append("\"线程ID：\":\"[%thread]\",");
                    jsonLogs.Append("\"日志级别：\":\"%-5level\",");
                    jsonLogs.Append("\"出错类：\":\"%logger property:[%property{NDC}] - \",");
                    jsonLogs.Append("\"错误描述：\":%message");
                    jsonLogs.Append("}%newline");
                    patternLayout.ConversionPattern = jsonLogs.ToString();//"主机IP：" + GetIP() + " 记录时间：%date 线程ID:[%thread] 日志级别：%-5level 出错类：%logger property:[%property{NDC}] - 错误描述：%message%newline";
                    patternLayout.ActivateOptions();
                    rollFileAppender.Layout = patternLayout;
                    rollFileAppender.Encoding = Encoding.UTF8;
                    rollFileAppender.ActivateOptions();
                    rollFileAppender.LockingModel = new FileAppender.MinimalLock();//最小锁定模型以允许多个进程可以写入同一个文件，防止生成的日志会出现多个累加文件；
                    rollFileAppender.PreserveLogFileNameExtension = true;
                    //log to console
                    log4net.Appender.ConsoleAppender consoleAppender = new ConsoleAppender();
                    consoleAppender.Layout = patternLayout;
                    lstAppender.Add(rollFileAppender);
                    lstAppender.Add(consoleAppender);

                    ILoggerRepository repository = LogManager.CreateRepository("NETCoreRepository");
                    log4net.Config.BasicConfigurator.Configure(repository, lstAppender.ToArray());
                    log = LogManager.GetLogger(repository.Name, "Log");
                }
                return log;
            }
        }
        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        /// <returns>若失败则返回回送地址</returns>
        private static string GetIP()
        {
            try
            {
                ////如果客户端使用了代理服务器，则利用HTTP_X_FORWARDED_FOR找到客户端IP地址
                //string userHostAddress = "";
                //if (!string.IsNullOrEmpty(HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] + ""))
                //{
                //    userHostAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString().Split(',')[0].Trim();
                //}

                ////否则直接读取REMOTE_ADDR获取客户端IP地址
                //if (string.IsNullOrEmpty(userHostAddress))
                //{
                //    userHostAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                //}

                ////前两者均失败，则利用Request.UserHostAddress属性获取IP地址，但此时无法确定该IP是客户端IP还是代理IP
                //if (string.IsNullOrEmpty(userHostAddress))
                //{
                //    userHostAddress = HttpContext.Current.Request.UserHostAddress;
                //}

                //bool isIp = System.Text.RegularExpressions.Regex.IsMatch(userHostAddress, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
                ////最后判断获取是否成功，并检查IP地址的格式（检查其格式非常重要）
                //if (!string.IsNullOrEmpty(userHostAddress) && isIp)
                //{
                //    return userHostAddress;
                //}

                return Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault<IPAddress>(a => a.AddressFamily.ToString().Equals("InterNetwork")).ToString();
            }
            catch (Exception ex)
            {
                return "error:" + ex.ToString().Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
            }
        }


        static LogHelper()
        {
            LogLog.InternalDebugging = true;

        }
    }
}
