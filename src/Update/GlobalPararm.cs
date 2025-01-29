using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Net;
using System.Net.NetworkInformation;

namespace Squirrel.Update
{
    public static class GlobalPararm
    {
        /// <summary>
        /// 产品名称
        /// </summary>
        public static string ProductName = "LinkXR";

        /// <summary>
        /// 是否是正式发布版本，否则是测试版本，两者更新地址不同
        /// </summary>
        public static bool IsReleaseMode = true;

        /// <summary>
        /// 更新地址
        /// </summary>
        public static string UpdateUrl
        {
            get
            {
                return IsReleaseMode ? "https://gdi-update.obs.cn-east-2.myhuaweicloud.com/LinkXR/Release/Launcher" : "https://gdi-update.obs.cn-east-2.myhuaweicloud.com/LinkXR/Debug/Launcher";
            }
        }

        /// <summary>
        /// 更新说明地址
        /// </summary>
        public static string UpdateLogUrl
        {
            get
            {
                return IsReleaseMode ? "https://gdi-update.obs.cn-east-2.myhuaweicloud.com/LinkXR/Release/UpdateLog.html" : "https://gdi-update.obs.cn-east-2.myhuaweicloud.com/LinkXR/Debug/UpdateLog.html";
            }
        }

        /// <summary>
        /// 网络是否已连接
        /// </summary>
        /// <returns></returns>
        public static bool InternetGetConnected()
        {
            Ping ping = new Ping();
            PingOptions options = new PingOptions();
            options.DontFragment = true;
            string data = string.Empty;
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 500;
            PingReply reply = ping.Send("www.gvrbox.com", timeout, buffer, options);
            return reply.Status == IPStatus.Success;
        }
    }
}
