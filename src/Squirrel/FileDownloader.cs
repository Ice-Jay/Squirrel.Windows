using System;
using System.Net;
using System.Threading.Tasks;
using Splat;
using System.Diagnostics;

namespace Squirrel
{
    public interface IFileDownloader
    {
        Task DownloadFile(string url, string targetFile, /*Action<int> progress,*/ Action<DownloadProgressData> progressData);
        Task<byte[]> DownloadUrl(string url);
    }

    /// <summary>
    /// 下载进度数据
    /// </summary>
    public class DownloadProgressData
    {
        /// <summary>
        /// 进度值
        /// </summary>
        public float progress = default(float);

        /// <summary>
        /// 已接收的字节数
        /// </summary>
        public long bytesReceived = default(long);

        /// <summary>
        /// 要接收的总字节数
        /// </summary>
        public long totalBytesToReceive = default(long);

        /// <summary>
        /// 兆字节每秒的速度
        /// </summary>
        public double speedMBPerSecond = default(double);
        
        /// <summary>
        /// 获取以秒为单位的剩余时间
        /// </summary>
        public float remainingTime
        {
            get
            {
                return speedMBPerSecond == 0 ? 0 : (float)((totalBytesToReceive - bytesReceived) / 1024d / 1024d / speedMBPerSecond);
            }
        }

        public DownloadProgressData(float progress = default(float), long bytesReceived = default(long), long totalBytesToReceive = default(long), double speedMBPerSecond = default(double))
        {
            this.progress = progress;
            this.bytesReceived = bytesReceived;
            this.totalBytesToReceive = totalBytesToReceive;
            this.speedMBPerSecond = speedMBPerSecond;
        }

        public override string ToString()
        {
            string downloadSpeed;
            if (speedMBPerSecond >= 1)
                downloadSpeed = speedMBPerSecond.ToString("f2") + "MB/s";
            else if (speedMBPerSecond * 1024d >= 1)
                downloadSpeed = (speedMBPerSecond * 1024d).ToString("f2") + "KB/s";
            else
                downloadSpeed = (speedMBPerSecond * 1024d * 1024d).ToString("f2") + "B/s";

            int hour = (int)(remainingTime / 3600);
            int minute = (int)(remainingTime / 60 % 60);
            int second = (int)(remainingTime % 60);

            return string.Format("Progress: {0}, BytesReceived: {1}, TotalBytesToReceive: {2}, DownloadSpeed: {3}, RemainingTime: {4}:{5}:{6}", progress, bytesReceived, totalBytesToReceive, downloadSpeed, hour.ToString("00"), minute.ToString("00"), second.ToString("00"));
        }
    }

    public class FileDownloader : IFileDownloader, IEnableLogger
    {
        private readonly WebClient _providedClient;

        public FileDownloader(WebClient providedClient = null)
        {
            _providedClient = providedClient;
        }

        /// <summary>
        /// 下载文件
        /// <remark>源码中只提供了进度值回调，现扩展成进度数据回调，包括进度值、已接收的字节数、要接收的总字节数、兆字节每秒的速度、以秒为单位的剩余时间</remark>> 
        /// </summary>
        /// <param name="url">下载链接</param>
        /// <param name="targetFile">目标文件名</param>
        /// <param name="progressData">进度数据</param>
        /// <returns></returns>
        public async Task DownloadFile(string url, string targetFile, /*Action<int> progress,*/ Action<DownloadProgressData> progressData)
        {
            using (var wc = _providedClient ?? Utility.CreateWebClient()) {
                var failedUrl = default(string);

                DownloadProgressData data = new DownloadProgressData();
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                //var lastSignalled = DateTime.MinValue;
                wc.DownloadProgressChanged += (sender, args) =>
                {
                    //var now = DateTime.Now;

                    data.progress = (float)args.BytesReceived / args.TotalBytesToReceive * 100;
                    data.bytesReceived = args.BytesReceived;
                    data.totalBytesToReceive = args.TotalBytesToReceive;
                    data.speedMBPerSecond = args.BytesReceived / 1024d / 1024d / sw.Elapsed.TotalSeconds;

                    //为了让下载进度条连续且平滑，不使用时间间隔来调用
                    progressData(data);
                    //if (now - lastSignalled > TimeSpan.FromMilliseconds(1000)) {
                    //    lastSignalled = now;
                    //    //progress(args.ProgressPercentage);
                    //    progressData(data);
                    //}
                };
                

            retry:
                try {
                    this.Log().Info("Downloading file: " + (failedUrl ?? url));

                    await this.WarnIfThrows(
                        async () => {
                            await wc.DownloadFileTaskAsync(failedUrl ?? url, targetFile);
                            //progress(100);
                            progressData(new DownloadProgressData(100, data.totalBytesToReceive, data.totalBytesToReceive, 0));
                        },
                        "Failed downloading URL: " + (failedUrl ?? url));
                } catch (Exception) {
                    // NB: Some super brain-dead services are case-sensitive yet 
                    // corrupt case on upload. I can't even.
                    if (failedUrl != null) throw;

                    failedUrl = url.ToLower();
                    //progress(0);
                    progressData(new DownloadProgressData(0));
                    goto retry;
                }
            }
        }

        public async Task<byte[]> DownloadUrl(string url)
        {
            using (var wc = _providedClient ?? Utility.CreateWebClient()) {
            var failedUrl = default(string);

        retry:
            try {
                this.Log().Info("Downloading url: " + (failedUrl ?? url));

                return await this.WarnIfThrows(() => wc.DownloadDataTaskAsync(failedUrl ?? url),
                    "Failed to download url: " + (failedUrl ?? url));
            } catch (Exception) {
                // NB: Some super brain-dead services are case-sensitive yet 
                // corrupt case on upload. I can't even.
                if (failedUrl != null) throw;

                failedUrl = url.ToLower();
                goto retry;
            }
        }
    }
}
}
