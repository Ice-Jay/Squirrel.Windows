using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Splat;

namespace Squirrel
{
    public sealed partial class UpdateManager
    {
        internal class DownloadReleasesImpl : IEnableLogger
        {
            readonly string rootAppDirectory;

            public DownloadReleasesImpl(string rootAppDirectory)
            {
                this.rootAppDirectory = rootAppDirectory;
            }

            public async Task DownloadReleases(string updateUrlOrPath, IEnumerable<ReleaseEntry> releasesToDownload, /*Action<int> progress = null,*/ Action<DownloadProgressData> progressData = null, IFileDownloader urlDownloader = null)
            {
                //progress = progress ?? (_ => { });
                progressData = progressData ?? (_ => { });
                urlDownloader = urlDownloader ?? new FileDownloader();
                var packagesDirectory = Path.Combine(rootAppDirectory, "packages");

                double currentProgress = 0;
                double toIncrement = 100.0 / releasesToDownload.Count();
                long currentBytes = 0;
                long totalBytesToReceive = 0;
                double currentSpeed = 0;
                List<ReleaseEntry> downloadReleases = releasesToDownload.ToList();

                foreach (var item in releasesToDownload)
                {
                    totalBytesToReceive += item.Filesize;
                }

                if (Utility.IsHttpUrl(updateUrlOrPath)) {
                    // From Internet
                    await releasesToDownload.ForEachAsync(async x =>
                    {
                        var targetFile = Path.Combine(packagesDirectory, x.Filename);
                        double component = 0;
                        long componentBytes = 0;
                        double componentSpeed = 0;
                        double lastSpeed = 0;

                        await downloadRelease(updateUrlOrPath, x, urlDownloader, targetFile, p =>
                        {
                            //lock (progress) {
                            //    current -= component;
                            //    component = toIncrement / 100.0 * p;
                            //    progress((int)Math.Round(current += component));
                            //}
                            lock (progressData)
                            {
                                currentProgress -= component;
                                currentBytes -= componentBytes;
                                currentSpeed -= componentSpeed;
                                component = toIncrement / 100.0 * p.progress;
                                componentBytes = p.bytesReceived;
                                componentSpeed = p.speedMBPerSecond;

                                double finalProgress = currentProgress += component;
                                long finalReceive = currentBytes += componentBytes;

                                bool sameFile = currentSpeed == lastSpeed && currentSpeed != 0 && p.progress != 100;
                                lastSpeed = currentSpeed;
                                double totalSpeed = currentSpeed += componentSpeed;

                                if (p.progress == 100 && downloadReleases.Contains(x))
                                    downloadReleases.Remove(x);

                                double finalSpeed = sameFile || downloadReleases.Count == 0 ? p.speedMBPerSecond : totalSpeed / downloadReleases.Count;
                                //Console.WriteLine(finalSpeed.ToString() + "***" + p.speedMBPerSecond.ToString() + "---" + downloadReleases.Count.ToString() + "+++" + x.Filename + "|||" + finalProgress.ToString());
                                progressData(new DownloadProgressData((float)finalProgress, finalReceive, totalBytesToReceive, finalSpeed));
                            }
                        });

                        checksumPackage(x);
                    });
                } else {
                    // From Disk
                    await releasesToDownload.ForEachAsync(x => {
                        var targetFile = Path.Combine(packagesDirectory, x.Filename);

                        File.Copy(
                            Path.Combine(updateUrlOrPath, x.Filename),
                            targetFile,
                            true);

                        //lock (progress) progress((int)Math.Round(current += toIncrement));
                        lock (progressData)
                        {
                            DownloadProgressData p = new DownloadProgressData();
                            p.progress = (float)(currentProgress += toIncrement);
                            progressData(p);
                        }
                        checksumPackage(x);
                    });
                }
            }

            bool isReleaseExplicitlyHttp(ReleaseEntry x)
            {
                return x.BaseUrl != null && 
                    Uri.IsWellFormedUriString(x.BaseUrl, UriKind.Absolute);
            }

            Task downloadRelease(string updateBaseUrl, ReleaseEntry releaseEntry, IFileDownloader urlDownloader, string targetFile, /*Action<int> progress*/ Action<DownloadProgressData> progressData)
            {
                var baseUri = Utility.EnsureTrailingSlash(new Uri(updateBaseUrl));

                var releaseEntryUrl = releaseEntry.BaseUrl + releaseEntry.Filename;
                if (!String.IsNullOrEmpty(releaseEntry.Query)) {
                    releaseEntryUrl += releaseEntry.Query;
                }
                var sourceFileUrl = new Uri(baseUri, releaseEntryUrl).AbsoluteUri;
                File.Delete(targetFile);

                return urlDownloader.DownloadFile(sourceFileUrl, targetFile, progressData);
            }

            Task checksumAllPackages(IEnumerable<ReleaseEntry> releasesDownloaded)
            {
                return releasesDownloaded.ForEachAsync(x => checksumPackage(x));
            }

            void checksumPackage(ReleaseEntry downloadedRelease)
            {
                var targetPackage = new FileInfo(
                    Path.Combine(rootAppDirectory, "packages", downloadedRelease.Filename));

                if (!targetPackage.Exists) {
                    this.Log().Error("File {0} should exist but doesn't", targetPackage.FullName);

                    throw new Exception("Checksummed file doesn't exist: " + targetPackage.FullName);
                }

                if (targetPackage.Length != downloadedRelease.Filesize) {
                    this.Log().Error("File Length should be {0}, is {1}", downloadedRelease.Filesize, targetPackage.Length);
                    targetPackage.Delete();

                    throw new Exception("Checksummed file size doesn't match: " + targetPackage.FullName);
                }

                using (var file = targetPackage.OpenRead()) {
                    var hash = Utility.CalculateStreamSHA1(file);

                    if (!hash.Equals(downloadedRelease.SHA1,StringComparison.OrdinalIgnoreCase)) {
                        this.Log().Error("File SHA1 should be {0}, is {1}", downloadedRelease.SHA1, hash);
                        targetPackage.Delete();
                        throw new Exception("Checksum doesn't match: " + targetPackage.FullName);
                    }
                }
            }
        }
    }
}
