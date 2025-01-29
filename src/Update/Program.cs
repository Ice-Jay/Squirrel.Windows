using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using Splat;
using Squirrel.Json;
using NuGet;
using System.Text.RegularExpressions;
using Squirrel.Update.Forms;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using Microsoft.Win32;

namespace Squirrel.Update
{
    enum UpdateAction
    {
        Unset = 0, Install, Uninstall, Download, Update, Releasify, Shortcut,
        Deshortcut, ProcessStart, UpdateSelf, CheckForUpdate
    }

    enum UpdateType
    {
        Normal = 0, Auto, Remind
    }

    class Program : IEnableLogger
    {
        static OptionSet opts;

        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //若程序的启动带有打包、安装、自身更新这三个命令行参数，则执行相应函数，并直接return，否则执行更新逻辑
            if (args.Any(arg => arg.ToLower().Contains("releasify") || arg.ToLower().Contains("install") || arg.ToLower().Contains("updateself")))
            {
                excute(args);
                return;
            }

            //删除在安装新版本过程中退出程序导致的未删除的临时文件夹
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Application.StartupPath, "..", "SquirrelTemp"));
                if (directoryInfo.Exists)
                    Directory.Delete(directoryInfo.FullName, true);
            }
            catch (Exception)
            {

            }

            if (args.Any(arg => arg.Contains("--checkForAutoUpdateSilently") || arg.Contains("--checkForRemindUpdateSilently")))
            {
                excute(new string[] { string.Format("{0}={1}", args[0], GlobalPararm.UpdateUrl) });
                return;
            }

            Application.Run(new MainForm(args));
        }

        /// <summary>
        /// 向本机发送UDP消息
        /// </summary>
        /// <param name="msg">发送的消息</param>
        public static void SendUDPMessage(string message, int port)
        {
            UdpClient client = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                client.Send(bytes, bytes.Length, iPEndPoint);
            }
            catch
            {

            }
            finally
            {
                client.Close();
            }
        }

        public static int excute(string[] args)
        {
            var pg = new Program();

            try
            {
                return pg.main(args);
            }
            catch (Exception ex)
            {
                // NB: Normally this is a terrible idea but we want to make
                // sure Setup.exe above us gets the nonzero error code
                Console.Error.WriteLine(ex);
                return -1;
            }
        }

        int main(string[] args)
        {
            // NB: Trying to delete the app directory while we have Setup.log
            // open will actually crash the uninstaller
            bool isUninstalling = args.Any(x => x.Contains("uninstall"));

            //生成过程日志文件
            //using (var logger = new SetupLogLogger(isUninstalling) { Level = LogLevel.Info })
            //{
            //    Locator.CurrentMutable.Register(() => logger, typeof(Splat.ILogger));

            //    try
            //    {
            //        return executeCommandLine(args);
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Write("Unhandled exception: " + ex, LogLevel.Fatal);

            //        if (!args.Any(arg => arg.Contains("--checkForAutoUpdateSilently") || arg.Contains("--checkForRemindUpdateSilently")))
            //        {
            //            LoadingForm.Instance.SetLoadingMessage("更新失败，请稍后再试");
            //            LoadingForm.Instance.loadingType = LoadingType.Failed;
            //            LoadingForm.Instance.Invalidate();
            //        }
            //        else
            //            Process.GetCurrentProcess().Kill();
            //        throw;
            //    }
            //}

            try
            {
                return executeCommandLine(args);
            }
            catch (Exception)
            {
                if (!args.Any(arg => arg.Contains("--checkForAutoUpdateSilently") || arg.Contains("--checkForRemindUpdateSilently")))
                {
                    LoadingForm.Instance.SetLoadingMessage("更新失败，请稍后再试");
                    LoadingForm.Instance.loadingType = LoadingType.Failed;
                    LoadingForm.Instance.Invalidate();
                }
                else
                    Process.GetCurrentProcess().Kill();
                throw;
            }
        }

        int executeCommandLine(string[] args)
        {
            var animatedGifWindowToken = new CancellationTokenSource();

#if !MONO
            // Uncomment to test Gifs
            /*
            var ps = new ProgressSource();
            int i = 0; var t = new Timer(_ => ps.Raise(i += 10), null, 0, 1000);
            AnimatedGifWindow.ShowWindow(TimeSpan.FromMilliseconds(0), animatedGifWindowToken.Token, ps);
            Thread.Sleep(10 * 60 * 1000);
            */
#endif

            using (Disposable.Create(() => animatedGifWindowToken.Cancel()))
            {
                this.Log().Info("Starting Squirrel Updater: " + String.Join(" ", args));

                if (args.Any(x => x.StartsWith("/squirrel", StringComparison.OrdinalIgnoreCase)))
                {
                    // NB: We're marked as Squirrel-aware, but we don't want to do
                    // anything in response to these events
                    return 0;
                }

                bool silentInstall = false;
                var updateAction = default(UpdateAction);

                string target = default(string);
                string releaseDir = default(string);
                string packagesDir = default(string);
                string bootstrapperExe = default(string);
                string backgroundGif = default(string);
                string signingParameters = default(string);
                string baseUrl = default(string);
                string processStart = default(string);
                string processStartArgs = default(string);
                string setupIcon = default(string);
                string icon = default(string);
                string shortcutArgs = default(string);
                string frameworkVersion = "net45";
                bool shouldWait = false;
                bool noMsi = true/*(Environment.OSVersion.Platform != PlatformID.Win32NT)*/;        // NB: WiX doesn't work under Mono / Wine
                bool noDelta = false;
                bool checkForUpdateSilently = false;
                var updateType = default(UpdateType);

                opts = new OptionSet() {
                    "Usage: Squirrel.exe command [OPTS]",
                    "Manages Squirrel packages",
                    "",
                    "Commands",
                    { "install=", "Install the app whose package is in the specified directory", v => { updateAction = UpdateAction.Install; target = v; } },
                    { "uninstall", "Uninstall the app the same dir as Update.exe", v => updateAction = UpdateAction.Uninstall},
                    { "download=", "Download the releases specified by the URL and write new results to stdout as JSON", v => { updateAction = UpdateAction.Download; target = v; } },
                    { "checkForUpdate=", "Check for one available update and writes new results to stdout as JSON", v => { updateAction = UpdateAction.CheckForUpdate; target = v; updateType = UpdateType.Normal; } },
                    { "checkForAutoUpdateSilently=", "Check for one available update and writes new results to stdout as JSON", v => { updateAction = UpdateAction.CheckForUpdate; target = v; checkForUpdateSilently = true; updateType = UpdateType.Auto; } },
                    { "checkForRemindUpdateSilently=", "Check for one available update and writes new results to stdout as JSON", v => { updateAction = UpdateAction.CheckForUpdate; target = v; checkForUpdateSilently = true; updateType = UpdateType.Remind; } },
                    { "update=", "Update the application to the latest remote version specified by URL", v => { updateAction = UpdateAction.Update; target = v; } },
                    { "releasify=", "Update or generate a releases directory with a given NuGet package", v => { updateAction = UpdateAction.Releasify; target = v; } },
                    { "createShortcut=", "Create a shortcut for the given executable name", v => { updateAction = UpdateAction.Shortcut; target = v; } },
                    { "removeShortcut=", "Remove a shortcut for the given executable name", v => { updateAction = UpdateAction.Deshortcut; target = v; } },
                    { "updateSelf=", "Copy the currently executing Update.exe into the default location", v => { updateAction =  UpdateAction.UpdateSelf; target = v; } },
                    { "processStart=", "Start an executable in the latest version of the app package", v => { updateAction =  UpdateAction.ProcessStart; processStart = v; }, true},
                    { "processStartAndWait=", "Start an executable in the latest version of the app package", v => { updateAction =  UpdateAction.ProcessStart; processStart = v; shouldWait = true; }, true},
                    "",
                    "Options:",
                    { "h|?|help", "Display Help and exit", _ => {} },
                    { "r=|releaseDir=", "Path to a release directory to use with releasify", v => releaseDir = v},
                    { "p=|packagesDir=", "Path to the NuGet Packages directory for C# apps", v => packagesDir = v},
                    { "bootstrapperExe=", "Path to the Setup.exe to use as a template", v => bootstrapperExe = v},
                    { "g=|loadingGif=", "Path to an animated GIF to be displayed during installation", v => backgroundGif = v},
                    { "i=|icon", "Path to an ICO file that will be used for icon shortcuts", v => icon = v},
                    { "setupIcon=", "Path to an ICO file that will be used for the Setup executable's icon", v => setupIcon = v},
                    { "n=|signWithParams=", "Sign the installer via SignTool.exe with the parameters given", v => signingParameters = v},
                    { "s|silent", "Silent install", _ => silentInstall = true},
                    { "b=|baseUrl=", "Provides a base URL to prefix the RELEASES file packages with", v => baseUrl = v, true},
                    { "a=|process-start-args=", "Arguments that will be used when starting executable", v => processStartArgs = v, true},
                    { "l=|shortcut-locations=", "Comma-separated string of shortcut locations, e.g. 'Desktop,StartMenu'", v => shortcutArgs = v},
                    { "no-msi", "Don't generate an MSI package", v => noMsi = true},
                    { "no-delta", "Don't generate delta packages to save time", v => noDelta = true},
                    { "framework-version=", "Set the required .NET framework version, e.g. net461", v => frameworkVersion = v },
                };

                opts.Parse(args);

                // NB: setupIcon and icon are just aliases for compatibility
                // reasons, because of a dumb breaking rename I made in 1.0.1
                setupIcon = setupIcon ?? icon;

                if (updateAction == UpdateAction.Unset)
                {
                    ShowHelp();
                    return -1;
                }

                switch (updateAction)
                {
#if !MONO
                    case UpdateAction.Install:
                        var progressSource = new ProgressSource();
                        if (!silentInstall)
                        {
                            AnimatedGifWindow.ShowWindow(TimeSpan.FromSeconds(4), animatedGifWindowToken.Token, progressSource);
                        }

                        Install(silentInstall, progressSource, Path.GetFullPath(target)).Wait();
                        animatedGifWindowToken.Cancel();
                        break;
                    case UpdateAction.Uninstall:
                        Uninstall().Wait();
                        break;
                    case UpdateAction.Download:
                        Console.WriteLine(Download(target).Result);
                        break;
                    case UpdateAction.Update:
                        Update(target).Wait();
                        break;
                    case UpdateAction.CheckForUpdate:
                        string checkResult = CheckForUpdate(target, null, checkForUpdateSilently, updateType).Result;
                        //若更新方式是自动更新或提示更新，则执行相应的不同逻辑
                        if (checkResult.Contains("--autoUpdate") || checkResult.Contains("--remindUpdate"))
                            Application.Run(new MainForm(checkResult.Split('*')));
                        break;
                    case UpdateAction.UpdateSelf:
                        UpdateSelf().Wait();
                        break;
                    case UpdateAction.Shortcut:
                        Shortcut(target, shortcutArgs, processStartArgs, setupIcon);
                        break;
                    case UpdateAction.Deshortcut:
                        Deshortcut(target, shortcutArgs);
                        break;
                    case UpdateAction.ProcessStart:
                        ProcessStart(processStart, processStartArgs, shouldWait);
                        break;
#endif
                    case UpdateAction.Releasify:
                        Releasify(target, releaseDir, packagesDir, bootstrapperExe, backgroundGif, signingParameters, baseUrl, setupIcon, !noMsi, frameworkVersion, !noDelta);
                        break;
                }
            }

            return 0;
        }

        public async Task Install(bool silentInstall, ProgressSource progressSource, string sourceDirectory = null)
        {
            sourceDirectory = sourceDirectory ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var releasesPath = Path.Combine(sourceDirectory, "RELEASES");

            this.Log().Info("Starting install, writing to {0}", sourceDirectory);

            if (!File.Exists(releasesPath))
            {
                this.Log().Info("RELEASES doesn't exist, creating it at " + releasesPath);
                var nupkgs = (new DirectoryInfo(sourceDirectory)).GetFiles()
                    .Where(x => x.Name.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase))
                    .Select(x => ReleaseEntry.GenerateFromFile(x.FullName));

                ReleaseEntry.WriteReleaseFile(nupkgs, releasesPath);
            }

            var ourAppName = ReleaseEntry.ParseReleaseFile(File.ReadAllText(releasesPath, Encoding.UTF8))
                .Last().PackageName;

            using (var mgr = new UpdateManager(sourceDirectory, ourAppName))
            {
                this.Log().Info("About to install to: " + mgr.RootAppDirectory);
                if (Directory.Exists(mgr.RootAppDirectory))
                {
                    this.Log().Warn("Install path {0} already exists, burning it to the ground", mgr.RootAppDirectory);

                    mgr.KillAllExecutablesBelongingToPackage();
                    await Task.Delay(500);

                    await this.ErrorIfThrows(() => Utility.DeleteDirectory(mgr.RootAppDirectory),
                        "Failed to remove existing directory on full install, is the app still running???");

                    this.ErrorIfThrows(() => Utility.Retry(() => Directory.CreateDirectory(mgr.RootAppDirectory), 3),
                        "Couldn't recreate app directory, perhaps Antivirus is blocking it");
                }

                Directory.CreateDirectory(mgr.RootAppDirectory);

                var updateTarget = Path.Combine(mgr.RootAppDirectory, "Update.exe");
                this.ErrorIfThrows(() => File.Copy(Assembly.GetExecutingAssembly().Location, updateTarget, true),
                    "Failed to copy Update.exe to " + updateTarget);

                await mgr.FullInstall(silentInstall, progressSource.Raise);

                //await this.ErrorIfThrows(() => mgr.CreateUninstallerRegistryEntry(),
                //    "Failed to create uninstaller registry entry");
            }
        }

        public async Task Update(string updateUrl, string appName = null)
        {
            appName = appName ?? getAppNameFromDirectory();

            this.Log().Info("Starting update, downloading from " + updateUrl);

            string _strUpdateResults = "{\"updateResult\":";

            using (var mgr = new UpdateManager(updateUrl, appName))
            {
                bool ignoreDeltaUpdates = false;
                this.Log().Info("About to update to: " + mgr.RootAppDirectory);

                bool _updateResult = true;

            retry:
                try
                {
                    var updateInfo = await mgr.CheckForUpdate(ignoreDeltaUpdates: ignoreDeltaUpdates, progress: x => { });

                    IEnumerable<ReleaseEntry> releasesToApply = updateInfo.ReleasesToApply;
                    ReleaseEntry currentRelease = updateInfo.CurrentlyInstalledVersion;
                    ReleasePackage basePkg = new ReleasePackage(Path.Combine(mgr.RootAppDirectory, "packages", currentRelease.Filename));

                    //若没有当前版本完整包或需要更新的版本在2个及以上并且当前版本的大版本比最新版本的小，则不下载差异包，直接忽略差异包去下载最新版本完整包
                    if (!File.Exists(basePkg.InputPackageFile) || releasesToApply.Count() >= 2 && currentRelease.Version.Version.Major < releasesToApply.ToList()[releasesToApply.Count() - 1].Version.Version.Major)
                        updateInfo = await mgr.CheckForUpdate(true, progress: x => { });

                    if (GlobalPararm.ProductName == "LinkXR")
                    {
                        //关闭正在运行的LinkVR客户端和监听端
                        Process[] linkVR = Process.GetProcessesByName("LinkXR");
                        if (linkVR.Length > 0)
                        {
                            SendUDPMessage("-linkvr-quit", 26664);
                            linkVR[0].WaitForExit();
                            linkVR[0].Close();
                        }

                        Process[] newDaemon = Process.GetProcessesByName("LinkXRDaemon");
                        if (newDaemon.Length > 0)
                        {
                            SendUDPMessage("ExitForUpdate", 26666);
                            newDaemon[0].WaitForExit();
                            newDaemon[0].Close();
                        }

                        //卸载旧版本的Apache服务
                        string servicePath = Path.Combine(Directory.GetCurrentDirectory(), "LinkXRService.exe");
                        if (File.Exists(servicePath))
                        {
                            Process service = new Process();
                            service.StartInfo.FileName = servicePath;
                            service.StartInfo.Arguments = "--uninstallApache";
                            service.StartInfo.Verb = "runas";
                            service.Start();
                            service.WaitForExit();
                            service.Close();
                        }
                    }

                    //await mgr.DownloadReleases(updateInfo.ReleasesToApply, x => Console.WriteLine(33 + x / 3));
                    //await mgr.ApplyReleases(updateInfo, x => Console.WriteLine(66 + x / 3));

                    LoadingForm.Instance.isCompleted = false;
                    Task downloadTask = new Task(LoadingForm.Instance.SetProgressValue, 0);
                    downloadTask.Start();
                    DateTime lastSignalled = DateTime.MinValue;

                    await mgr.DownloadReleases(/*updateInfo.ReleasesToApply*/updateInfo, x =>
                    {
                        if (LoadingForm.Instance.loadingType != LoadingType.DownloadLoading)
                        {
                            LoadingForm.Instance.SetLoadingMessage("正在为您下载最新版本");
                            LoadingForm.Instance.loadingType = LoadingType.DownloadLoading;
                            LoadingForm.Instance.Label_DownloadInfo.Visible = true;
                            LoadingForm.Instance.Invalidate();
                        }

                        if (x.progress == 0 || x.progress != 100)
                        {
                            DateTime now = DateTime.Now;
                            if (now - lastSignalled > TimeSpan.FromMilliseconds(1000))
                            {
                                lastSignalled = now;

                                string downloadSpeed;
                                if (x.speedMBPerSecond >= 1)
                                    downloadSpeed = x.speedMBPerSecond.ToString("f2") + " MB/s";
                                else if (x.speedMBPerSecond * 1024d >= 1)
                                    downloadSpeed = (x.speedMBPerSecond * 1024d).ToString("f2") + " KB/s";
                                else
                                    downloadSpeed = (x.speedMBPerSecond * 1024d * 1024d).ToString("f2") + " B/s";

                                int hour = (int)(x.remainingTime / 3600);
                                int minute = (int)(x.remainingTime / 60 % 60);
                                int second = (int)(x.remainingTime % 60);

                                LoadingForm.Instance.SetDownloadMessage(string.Format("下载速度: {0}   剩余时间: {1}:{2}:{3}", downloadSpeed, hour.ToString("00"), minute.ToString("00"), second.ToString("00")), true);
                            }
                        }
                        else
                        {
                            LoadingForm.Instance.SetDownloadMessage("", false);
                            LoadingForm.Instance.Invalidate();
                        }

                        LoadingForm.Instance.toProgress = x.progress;
                    });

                    LoadingForm.Instance.isCompleted = true;
                    downloadTask.Wait();

                    LoadingForm.Instance.isCompleted = false;
                    Task applyTask = new Task(LoadingForm.Instance.SetProgressValue, 10);
                    applyTask.Start();

                    await mgr.ApplyReleases(updateInfo, x =>
                    {
                        if (LoadingForm.Instance.loadingType != LoadingType.ApplyLoading)
                        {
                            LoadingForm.Instance.SetLoadingMessage("正在为您安装最新版本");
                            LoadingForm.Instance.loadingType = LoadingType.ApplyLoading;
                            LoadingForm.Instance.Invalidate();
                        }
                        LoadingForm.Instance.toProgress = x;
                    });

                    LoadingForm.Instance.isCompleted = true;
                    applyTask.Wait();

                    //更新注册表中卸载项，更新控制面板卸载项中的软件信息
                    Version currentVersion= updateInfo.CurrentlyInstalledVersion.Version.Version;
                    Version latestVersion = updateInfo.FutureReleaseEntry.Version.Version;
                    string currentFullVersion = updateInfo.CurrentlyInstalledVersion.Version.ToString();
                    string latestFullVersion = updateInfo.FutureReleaseEntry.Version.ToString();
                    int latestMajorVersion = latestVersion.Major;
                    int latestMinorVersion = latestVersion.Minor;
                    int estimatedSize = (int)((GetDirectoryLength(Directory.GetParent(Process.GetCurrentProcess().MainModule.FileName).FullName) - GetDirectoryLength(Path.Combine(Directory.GetParent(Process.GetCurrentProcess().MainModule.FileName).FullName, "app-" + currentFullVersion))) / 1024d);
                    string installDate = DateTime.Now.ToString("yyyyMMdd");
                    
                    string keyName = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\{70F40D2C-0C87-4414-8C60-08D2A57A7019}_is1";
                    RegistryKey curUser = Registry.LocalMachine;
                    RegistryKey key = curUser.OpenSubKey(keyName, true);
                    if (key == null)
                    {
                        curUser.CreateSubKey(keyName);
                        key = curUser.OpenSubKey(keyName, true);
                    }

                    key.SetValue("DisplayVersion", latestFullVersion);
                    key.SetValue("EstimatedSize", estimatedSize);
                    key.SetValue("InstallDate", installDate);
                    key.SetValue("MajorVersion", latestMajorVersion);
                    key.SetValue("MinorVersion", latestMinorVersion);
                    key.SetValue("VersionMajor", latestMajorVersion);
                    key.SetValue("VersionMinor", latestMinorVersion);
                    key.Close();

                    //创建桌面快捷方式
                    string shortcutPublicPath = Path.Combine(Environment.GetEnvironmentVariable("Public"), "Desktop", string.Format("{0}.lnk", GlobalPararm.ProductName));
                    string shortcutUserPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), string.Format("{0}.lnk", GlobalPararm.ProductName));
                    if (File.Exists(shortcutUserPath)) File.Delete(shortcutUserPath);
                    string targetPath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), string.Format("{0}.exe", GlobalPararm.ProductName));
                    IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                    IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPublicPath);
                    shortcut.TargetPath = targetPath;
                    shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
                    shortcut.WindowStyle = 1;
                    shortcut.Description = GlobalPararm.ProductName == "LinkXR" ? "LinkXR，让Unity和Unreal内容适配更多VR沉浸式环境" : "";
                    shortcut.IconLocation = targetPath;
                    shortcut.Save();

                    _updateResult = true;
                    LoadingForm.Instance.SetLoadingMessage("更新完成，将为您自动重启");
                    LoadingForm.Instance.Invalidate();
                }
                catch (Exception ex)
                {
                    // false, if catch an exception
                    _updateResult = false;

                    if (ignoreDeltaUpdates)
                    {
                        this.Log().ErrorException("Really couldn't apply updates!", ex);
                        throw;
                    }

                    this.Log().WarnException("Failed to apply updates, falling back to full updates", ex);
                    ignoreDeltaUpdates = true;
                    goto retry;
                }

                var updateTarget = Path.Combine(mgr.RootAppDirectory, "Update.exe");

                //await this.ErrorIfThrows(() =>
                //    mgr.CreateUninstallerRegistryEntry(),
                //    "Failed to create uninstaller registry entry");
                
                // write update results to console, for which app can read from
                _strUpdateResults += (_updateResult) ? "\"true\"}" : "\"false\"}";
                Console.WriteLine(_strUpdateResults);

                if (_updateResult)
                {
                    Thread.Sleep(3000);
                    Application.Exit();
                }
            }
        }

        public async Task UpdateSelf()
        {
            waitForParentToExit();
            var src = Assembly.GetExecutingAssembly().Location;
            var updateDotExeForOurPackage = Path.Combine(Path.GetDirectoryName(src), "..", "Update.exe");

            await Task.Run(() =>
            {
                File.Copy(src, updateDotExeForOurPackage, true);

                try
                {
                    if (GlobalPararm.ProductName == "LinkXR")
                    {
                        //给新版本文件夹权限
                        string servicePath = Path.Combine(Path.GetDirectoryName(src), "LinkXRService.exe");
                        if (File.Exists(servicePath))
                        {
                            Process service = new Process();
                            service.StartInfo.FileName = servicePath;
                            service.StartInfo.Arguments = "--addSecurityControl";
                            service.StartInfo.Verb = "runas";
                            service.Start();
                            service.WaitForExit();
                            service.Close();
                        }
                    }

                    //寻找旧版本文件夹，将旧版本的版本号和当前版本的比较，若小，则删除
                    //注：此处因占用问题删不掉文件夹，只能删除文件夹子目录，因此删除文件夹的操作放在了Launcher端初始化的时候
                    if (Version.TryParse(Directory.GetParent(src).Name.Split('-')[1], out Version currentVersion))
                    {
                        string[] dirs = Directory.GetDirectories(Directory.GetParent(updateDotExeForOurPackage).FullName);
                        List<string> tempList = new List<string>();
                        foreach (var dir in dirs)
                        {
                            DirectoryInfo directoryInfo = new DirectoryInfo(dir);
                            if (directoryInfo.Name.Contains("app-"))
                            {
                                if (Version.TryParse(dir.Split('-')[1], out Version version))
                                {
                                    if (version < currentVersion)
                                        tempList.Add(dir);
                                }
                            }
                        }
                        foreach (var dirStr in tempList)
                        {
                            if (File.GetAttributes(dirStr) == FileAttributes.Directory)
                                Directory.Delete(dirStr, true);
                        }
                    }
                }
                catch
                {

                }

                if (GlobalPararm.ProductName == "LinkXR")
                {
                    string srcLinkVRRes = Path.Combine(Path.GetDirectoryName(src), "LinkXR_Data", "Resources", "unitydefaultresources");
                    string destLinkVRRes = Path.Combine(Path.GetDirectoryName(src), "LinkXR_Data", "Resources", "unity default resources");
                    if (File.Exists(srcLinkVRRes))
                        File.Move(srcLinkVRRes, destLinkVRRes);

                    string srcVRLinkRes = Path.Combine(Path.GetDirectoryName(src), "Program", "VR-Link", "VR-Link_Data", "Resources", "unitydefaultresources");
                    string destVRLinkRes = Path.Combine(Path.GetDirectoryName(src), "Program", "VR-Link", "VR-Link_Data", "Resources", "unity default resources");
                    if (File.Exists(srcVRLinkRes))
                        File.Move(srcVRLinkRes, destVRLinkRes);

                    string srcMRLinkRes = Path.Combine(Path.GetDirectoryName(src), "Program", "MR-Link", "MR-Link_Data", "Resources", "unitydefaultresources");
                    string destMRLinkRes = Path.Combine(Path.GetDirectoryName(src), "Program", "MR-Link", "MR-Link_Data", "Resources", "unity default resources");
                    if (File.Exists(srcMRLinkRes))
                        File.Move(srcMRLinkRes, destMRLinkRes);

                    string srcQuestLinkRes = Path.Combine(Path.GetDirectoryName(src), "Program", "Quest-Link", "Quest-Link_Data", "Resources", "unitydefaultresources");
                    string destQuestLinkRes = Path.Combine(Path.GetDirectoryName(src), "Program", "Quest-Link", "Quest-Link_Data", "Resources", "unity default resources");
                    if (File.Exists(srcQuestLinkRes))
                        File.Move(srcQuestLinkRes, destQuestLinkRes);

                    string srcExampleRes = Path.Combine(Path.GetDirectoryName(src), "Example", "LinkXRExample_Data", "Resources", "unitydefaultresources");
                    string destExampleRes = Path.Combine(Path.GetDirectoryName(src), "Example", "LinkXRExample_Data", "Resources", "unity default resources");
                    if (File.Exists(srcExampleRes))
                        File.Move(srcExampleRes, destExampleRes);
                }

                //因为下方采用runas降级启动LinkVR，会导致LinkVRService虽然是拥有管理员权限的程序，但降级后的LinkVR启动LinkVRService后，仍是降级后的级别，因此此处做针对性地解决
                //Process s = new Process();
                //s.StartInfo.FileName = Path.Combine(Path.GetDirectoryName(src), "..", "LinkVRService.exe");
                //s.StartInfo.Arguments = GlobalPararm.IsServer ? "--addSecurityControll --startApache" : "--addSecurityControll";
                //s.StartInfo.Verb = "runas";
                //s.Start();

                //采用runas降级启动LinkVR，防止出现启动的案例权限高于IQTracker导致有追踪但按键没响应的问题
                //0x20000:标准用户权限，0x40000管理员权限
                //string processFileName = Path.Combine(Path.GetDirectoryName(src), "..", "LinkVR.exe");
                //string processFileNameAndArguments = GlobalPararm.IsServer ? '"' + processFileName + '"' : '"' + $"{processFileName} -batchmode" + '"';
                //Process p = new Process();
                //p.StartInfo.FileName = "runas.exe";
                //p.StartInfo.Arguments = $"/trustlevel:0x20000 {processFileNameAndArguments}";
                //p.StartInfo.CreateNoWindow = true;
                //p.Start();

                //上方采用runas降级启动LinkVR，虽然LinkVR是只有标准用户权限了，但IQTracker仍视它为管理员权限程序，因此无效
                //因此，采用explorer方法代理运行程序，此方法不能携带程序参数，否则不能启动成功
                string processFileName = '"' + Path.Combine(Path.GetDirectoryName(src), "..", string.Format("{0}.exe", GlobalPararm.ProductName)) + '"';
                Process.Start("explorer.exe", processFileName);
            });
        }

        public async Task<string> Download(string updateUrl, string appName = null)
        {
            appName = appName ?? getAppNameFromDirectory();

            this.Log().Info("Fetching update information, downloading from " + updateUrl);
            using (var mgr = new UpdateManager(updateUrl, appName))
            {
                var updateInfo = await mgr.CheckForUpdate(progress: x => Console.WriteLine(x / 3));
                await mgr.DownloadReleases(/*updateInfo.ReleasesToApply*/updateInfo, x => Console.WriteLine(33 + x.progress / 3));

                var releaseNotes = updateInfo.FetchReleaseNotes();

                var sanitizedUpdateInfo = new
                {
                    currentVersion = updateInfo.CurrentlyInstalledVersion.Version.ToString(),
                    futureVersion = updateInfo.FutureReleaseEntry.Version.ToString(),
                    releasesToApply = updateInfo.ReleasesToApply.Select(x => new
                    {
                        version = x.Version.ToString(),
                        releaseNotes = releaseNotes.ContainsKey(x) ? releaseNotes[x] : "",
                    }).ToArray(),
                };

                return SimpleJson.SerializeObject(sanitizedUpdateInfo);
            }
        }
        
        public async Task<string> CheckForUpdate(string updateUrl, string appName = null, bool isSilent = false, UpdateType updateType = UpdateType.Normal)
        {
            //判断网络连接状态，虽然检查更新内部逻辑中有重试机制，并且重试三次后就抛出异常，但某些网络受限环境下，不会抛出异常，会一直卡在检查更新。
            if (!GlobalPararm.InternetGetConnected())
                throw new Exception("Connection timeout");

            appName = appName ?? getAppNameFromDirectory();

            this.Log().Info("Fetching update information, downloading from " + updateUrl);
            
            using (var mgr = new UpdateManager(updateUrl, appName))
            {
                var updateInfo = await mgr.CheckForUpdate(progress: x => Console.WriteLine(x));
                
                var releaseNotes = updateInfo.FetchReleaseNotes();

                var sanitizedUpdateInfo = new
                {
                    currentVersion = updateInfo.CurrentlyInstalledVersion.Version.ToString(),
                    futureVersion = updateInfo.FutureReleaseEntry.Version.ToString(),
                    releasesToApply = updateInfo.ReleasesToApply.Select(x => new
                    {
                        version = x.Version.ToString(),
                        releaseNotes = releaseNotes.ContainsKey(x) ? releaseNotes[x] : "",
                    }).ToArray(),
                };

                string currentVersion = updateInfo.CurrentlyInstalledVersion.Version.ToString();
                string latestVersion = updateInfo.FutureReleaseEntry.Version.ToString();

                //若已经是最新版本，如果是静默检查更新，则直接退出，如果是非静默检查更新，则弹出面板提示
                if (currentVersion == latestVersion)
                {
                    if (!isSilent)
                    {
                        MainForm.Instance.Invoke(new Action(() =>
                        {
                            LoadingForm.Instance.SetLoadingMessage(string.Format("{0}已是最新版本\r\n当前版本：V{1}", GlobalPararm.ProductName, currentVersion));
                            LoadingForm.Instance.Invalidate();
                            LoadingForm.Instance.ShowUpdateLog();
                            LoadingForm.Instance.Invalidate();
                        }));
                    }
                    else
                        Process.GetCurrentProcess().Kill();
                }
                //若不是最新版本，如果是静默检查更新，则根据更新方式返回结果，如果是非静默检查更新，则弹出面板提示
                else
                {
                    if (!isSilent)
                    {
                        MainForm.Instance.Invoke(new Action(() =>
                        {
                            UpdateForm.Instance.SetUpdateMessage(currentVersion, latestVersion);
                            MainForm.Instance.ConfigurePanel(UpdateForm.Instance);
                        }));
                    }
                    else
                    {
                        SimpleJson.SerializeObject(sanitizedUpdateInfo);
                        return updateType == UpdateType.Auto ? "--autoUpdate" : string.Format("--remindUpdate*{0}*{1}", currentVersion, latestVersion);
                    }
                }

                return SimpleJson.SerializeObject(sanitizedUpdateInfo);
            }
        }

        public async Task Uninstall(string appName = null)
        {
            this.Log().Info("Starting uninstall for app: " + appName);

            appName = appName ?? getAppNameFromDirectory();
            using (var mgr = new UpdateManager("", appName))
            {
                await mgr.FullUninstall();
                mgr.RemoveUninstallerRegistryEntry();
            }
        }

        public void Releasify(string package, string targetDir = null, string packagesDir = null, string bootstrapperExe = null, string backgroundGif = null, string signingOpts = null, string baseUrl = null, string setupIcon = null, bool generateMsi = true, string frameworkVersion = null, bool generateDeltas = true)
        {
            ensureConsole();

            if (baseUrl != null)
            {
                if (!Utility.IsHttpUrl(baseUrl))
                {
                    throw new Exception(string.Format("Invalid --baseUrl '{0}'. A base URL must start with http or https and be a valid URI.", baseUrl));
                }

                if (!baseUrl.EndsWith("/"))
                {
                    baseUrl += "/";
                }
            }

            targetDir = targetDir ?? Path.Combine(".", "Releases");
            packagesDir = packagesDir ?? ".";
            bootstrapperExe = bootstrapperExe ?? Path.Combine(".", "Setup.exe");

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            if (!File.Exists(bootstrapperExe))
            {
                bootstrapperExe = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                    "Setup.exe");
            }

            this.Log().Info("Bootstrapper EXE found at:" + bootstrapperExe);

            var di = new DirectoryInfo(targetDir);
            File.Copy(package, Path.Combine(di.FullName, Path.GetFileName(package)), true);

            var allNuGetFiles = di.EnumerateFiles()
                .Where(x => x.Name.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase));

            var toProcess = allNuGetFiles.Where(x => !x.Name.Contains("-delta") && !x.Name.Contains("-full"));
            var processed = new List<string>();

            var releaseFilePath = Path.Combine(di.FullName, "RELEASES");
            var previousReleases = new List<ReleaseEntry>();
            if (File.Exists(releaseFilePath))
            {
                previousReleases.AddRange(ReleaseEntry.ParseReleaseFile(File.ReadAllText(releaseFilePath, Encoding.UTF8)));
            }

            foreach (var file in toProcess)
            {
                this.Log().Info("Creating release package: " + file.FullName);

                var rp = new ReleasePackage(file.FullName);
                rp.CreateReleasePackage(Path.Combine(di.FullName, rp.SuggestedReleaseFileName), packagesDir, contentsPostProcessHook: pkgPath =>
                {
                    new DirectoryInfo(pkgPath).GetAllFilesRecursively()
                        .Where(x => x.Name.ToLowerInvariant().EndsWith(".exe"))
                        .Where(x => !x.Name.ToLowerInvariant().Contains("squirrel.exe"))
                        .Where(x => Utility.IsFileTopLevelInPackage(x.FullName, pkgPath))
                        .Where(x => Utility.ExecutableUsesWin32Subsystem(x.FullName))
                        .ForEachAsync(x => createExecutableStubForExe(x.FullName))
                        .Wait();

                    if (signingOpts == null) return;

                    new DirectoryInfo(pkgPath).GetAllFilesRecursively()
                        .Where(x => Utility.FileIsLikelyPEImage(x.Name))
                        .ForEachAsync(async x =>
                        {
                            if (isPEFileSigned(x.FullName))
                            {
                                this.Log().Info("{0} is already signed, skipping", x.FullName);
                                return;
                            }

                            this.Log().Info("About to sign {0}", x.FullName);
                            await signPEFile(x.FullName, signingOpts);
                        }, 1)
                        .Wait();
                });

                processed.Add(rp.ReleasePackageFile);

                var prev = ReleaseEntry.GetPreviousRelease(previousReleases, rp, targetDir);
                if (prev != null && generateDeltas)
                {
                    var deltaBuilder = new DeltaPackageBuilder(null);

                    var dp = deltaBuilder.CreateDeltaPackage(prev, rp,
                        Path.Combine(di.FullName, rp.SuggestedReleaseFileName.Replace("full", "delta")));
                    processed.Insert(0, dp.InputPackageFile);
                }
            }

            foreach (var file in toProcess) { File.Delete(file.FullName); }

            var newReleaseEntries = processed
                .Select(packageFilename => ReleaseEntry.GenerateFromFile(packageFilename, baseUrl))
                .ToList();
            var distinctPreviousReleases = previousReleases
                .Where(x => !newReleaseEntries.Select(e => e.Version).Contains(x.Version));
            var releaseEntries = distinctPreviousReleases.Concat(newReleaseEntries).ToList();

            ReleaseEntry.WriteReleaseFile(releaseEntries, releaseFilePath);

            var targetSetupExe = Path.Combine(di.FullName, "Setup.exe");
            var newestFullRelease = releaseEntries.MaxBy(x => x.Version).Where(x => !x.IsDelta).First();

            File.Copy(bootstrapperExe, targetSetupExe, true);
            var zipPath = createSetupEmbeddedZip(Path.Combine(di.FullName, newestFullRelease.Filename), di.FullName, backgroundGif, signingOpts).Result;

            var writeZipToSetup = Utility.FindHelperExecutable("WriteZipToSetup.exe");

            try
            {
                var arguments = String.Format("\"{0}\" \"{1}\" \"--set-required-framework\" \"{2}\"", targetSetupExe, zipPath, frameworkVersion);
                var result = Utility.InvokeProcessAsync(writeZipToSetup, arguments, CancellationToken.None).Result;
                if (result.Item1 != 0) throw new Exception("Failed to write Zip to Setup.exe!\n\n" + result.Item2);
            }
            catch (Exception ex)
            {
                this.Log().ErrorException("Failed to update Setup.exe with new Zip file", ex);
            }
            finally
            {
                File.Delete(zipPath);
            }

            Utility.Retry(() =>
                setPEVersionInfoAndIcon(targetSetupExe, new ZipPackage(package), setupIcon).Wait());

            if (signingOpts != null)
            {
                signPEFile(targetSetupExe, signingOpts).Wait();
            }

            if (generateMsi)
            {
                createMsiPackage(targetSetupExe, new ZipPackage(package)).Wait();

                if (signingOpts != null)
                {
                    signPEFile(targetSetupExe.Replace(".exe", ".msi"), signingOpts).Wait();
                }
            }
        }

        public void Shortcut(string exeName, string shortcutArgs, string processStartArgs, string icon)
        {
            if (IgnoreExeShortcut(exeName)) return;
            if (String.IsNullOrWhiteSpace(exeName))
            {
                ShowHelp();
                return;
            }

            var appName = getAppNameFromDirectory();
            var defaultLocations = ShortcutLocation.StartMenu | ShortcutLocation.Desktop;
            var locations = parseShortcutLocations(shortcutArgs);

            using (var mgr = new UpdateManager("", appName))
            {
                mgr.CreateShortcutsForExecutable(exeName, locations ?? defaultLocations, false, processStartArgs, icon);
            }
        }

        public void Deshortcut(string exeName, string shortcutArgs)
        {
            if (String.IsNullOrWhiteSpace(exeName))
            {
                ShowHelp();
                return;
            }

            var appName = getAppNameFromDirectory();
            var defaultLocations = ShortcutLocation.StartMenu | ShortcutLocation.Desktop;
            var locations = parseShortcutLocations(shortcutArgs);

            using (var mgr = new UpdateManager("", appName))
            {
                mgr.RemoveShortcutsForExecutable(exeName, locations ?? defaultLocations);
            }
        }

        public void ProcessStart(string exeName, string arguments, bool shouldWait)
        {
            if (String.IsNullOrWhiteSpace(exeName))
            {
                ShowHelp();
                return;
            }

            // Find the latest installed version's app dir
            var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var releases = ReleaseEntry.ParseReleaseFile(
                File.ReadAllText(Utility.LocalReleaseFileForAppDir(appDir), Encoding.UTF8));

            // NB: We add the hacked up version in here to handle a migration
            // issue, where versions of Squirrel pre PR #450 will not understand
            // prerelease tags, so it will end up writing the release name sans
            // tags. However, the RELEASES file _will_ have them, so we need to look
            // for directories that match both the real version, and the sanitized
            // version, giving priority to the former.
            var latestAppDir = releases
                .OrderByDescending(x => x.Version)
                .SelectMany(x => new[] {
                    Utility.AppDirForRelease(appDir, x),
                    Utility.AppDirForVersion(appDir, new SemanticVersion(x.Version.Version.Major, x.Version.Version.Minor, x.Version.Version.Build, ""))
                })
                .FirstOrDefault(x => Directory.Exists(x));

            // Check for the EXE name they want
            var targetExe = new FileInfo(Path.Combine(latestAppDir, exeName.Replace("%20", " ")));
            this.Log().Info("Want to launch '{0}'", targetExe);

            // Check for path canonicalization attacks
            if (!targetExe.FullName.StartsWith(latestAppDir, StringComparison.Ordinal))
            {
                throw new ArgumentException();
            }

            if (!targetExe.Exists)
            {
                this.Log().Error("File {0} doesn't exist in current release", targetExe);
                throw new ArgumentException();
            }

            if (shouldWait) waitForParentToExit();

            try
            {
                this.Log().Info("About to launch: '{0}': {1}", targetExe.FullName, arguments ?? "");
                Process.Start(new ProcessStartInfo(targetExe.FullName, arguments ?? "") { WorkingDirectory = Path.GetDirectoryName(targetExe.FullName) });
            }
            catch (Exception ex)
            {
                this.Log().ErrorException("Failed to start process", ex);
            }
        }

        public void ShowHelp()
        {
            ensureConsole();
            opts.WriteOptionDescriptions(Console.Out);
        }

        void waitForParentToExit()
        {
            // Grab a handle the parent process
            var parentPid = NativeMethods.GetParentProcessId();
            var handle = default(IntPtr);

            // Wait for our parent to exit
            try
            {
                handle = NativeMethods.OpenProcess(ProcessAccess.Synchronize, false, parentPid);
                if (handle != IntPtr.Zero)
                {
                    this.Log().Info("About to wait for parent PID {0}", parentPid);
                    NativeMethods.WaitForSingleObject(handle, 0xFFFFFFFF /*INFINITE*/);
                }
                else
                {
                    this.Log().Info("Parent PID {0} no longer valid - ignoring", parentPid);
                }
            }
            finally
            {
                if (handle != IntPtr.Zero) NativeMethods.CloseHandle(handle);
            }
        }

        async Task<string> createSetupEmbeddedZip(string fullPackage, string releasesDir, string backgroundGif, string signingOpts)
        {
            string tempPath;

            this.Log().Info("Building embedded zip file for Setup.exe");
            using (Utility.WithTempDirectory(out tempPath, null))
            {
                this.ErrorIfThrows(() =>
                {
                    File.Copy(Assembly.GetEntryAssembly().Location.Replace("-Mono.exe", ".exe"), Path.Combine(tempPath, "Update.exe"));
                    File.Copy(fullPackage, Path.Combine(tempPath, Path.GetFileName(fullPackage)));
                }, "Failed to write package files to temp dir: " + tempPath);

                if (!String.IsNullOrWhiteSpace(backgroundGif))
                {
                    this.ErrorIfThrows(() =>
                    {
                        File.Copy(backgroundGif, Path.Combine(tempPath, "background.gif"));
                    }, "Failed to write animated GIF to temp dir: " + tempPath);
                }

                var releases = new[] { ReleaseEntry.GenerateFromFile(fullPackage) };
                ReleaseEntry.WriteReleaseFile(releases, Path.Combine(tempPath, "RELEASES"));

                var target = Path.GetTempFileName();
                File.Delete(target);

                // Sign Update.exe so that virus scanners don't think we're
                // pulling one over on them
                if (signingOpts != null)
                {
                    var di = new DirectoryInfo(tempPath);

                    var files = di.EnumerateFiles()
                        .Where(x => x.Name.ToLowerInvariant().EndsWith(".exe"))
                        .Select(x => x.FullName);

                    await files.ForEachAsync(x => signPEFile(x, signingOpts));
                }

                this.ErrorIfThrows(() =>
                    ZipFile.CreateFromDirectory(tempPath, target, CompressionLevel.Optimal, false),
                    "Failed to create Zip file from directory: " + tempPath);

                return target;
            }
        }

        static async Task signPEFile(string exePath, string signingOpts)
        {
            // Try to find SignTool.exe
            var exe = @".\signtool.exe";
            if (!File.Exists(exe))
            {
                exe = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "signtool.exe");

                // Run down PATH and hope for the best
                if (!File.Exists(exe)) exe = "signtool.exe";
            }

            var processResult = await Utility.InvokeProcessAsync(exe,
                String.Format("sign {0} \"{1}\"", signingOpts, exePath), CancellationToken.None);

            if (processResult.Item1 != 0)
            {
                var optsWithPasswordHidden = new Regex(@"/p\s+\w+").Replace(signingOpts, "/p ********");
                var msg = String.Format("Failed to sign, command invoked was: '{0} sign {1} {2}'",
                    exe, optsWithPasswordHidden, exePath);

                throw new Exception(msg);
            }
            else
            {
                Console.WriteLine(processResult.Item2);
            }
        }
        bool isPEFileSigned(string path)
        {
#if MONO
            return Path.GetExtension(path).Equals(".exe", StringComparison.OrdinalIgnoreCase);
#else
            try
            {
                return AuthenticodeTools.IsTrusted(path);
            }
            catch (Exception ex)
            {
                this.Log().ErrorException("Failed to determine signing status for " + path, ex);
                return false;
            }
#endif
        }

        async Task createExecutableStubForExe(string fullName)
        {
            if (IgnoreExeShortcut(fullName)) return;

            var exe = Utility.FindHelperExecutable(@"StubExecutable.exe");

            var target = Path.Combine(
                Path.GetDirectoryName(fullName),
                Path.GetFileNameWithoutExtension(fullName) + "_ExecutionStub.exe");

            await Utility.CopyToAsync(exe, target);

            await Utility.InvokeProcessAsync(
                Utility.FindHelperExecutable("WriteZipToSetup.exe"),
                String.Format("--copy-stub-resources \"{0}\" \"{1}\"", fullName, target),
                CancellationToken.None);
        }

        static async Task setPEVersionInfoAndIcon(string exePath, IPackage package, string iconPath = null)
        {
            var realExePath = Path.GetFullPath(exePath);
            var company = String.Join(",", package.Authors);
            var verStrings = new Dictionary<string, string>() {
                { "CompanyName", company },
                { "LegalCopyright", package.Copyright ?? "Copyright © " + DateTime.Now.Year.ToString() + " " + company },
                { "FileDescription", package.Summary ?? package.Description ?? "Installer for " + package.Id },
                { "ProductName", package.Description ?? package.Summary ?? package.Id },
            };

            var args = verStrings.Aggregate(new StringBuilder("\"" + realExePath + "\""), (acc, x) => { acc.AppendFormat(" --set-version-string \"{0}\" \"{1}\"", x.Key, x.Value); return acc; });
            args.AppendFormat(" --set-file-version {0} --set-product-version {0}", package.Version.ToString());
            if (iconPath != null)
            {
                args.AppendFormat(" --set-icon \"{0}\"", Path.GetFullPath(iconPath));
            }

            // Try to find rcedit.exe
            string exe = Utility.FindHelperExecutable("rcedit.exe");

            var processResult = await Utility.InvokeProcessAsync(exe, args.ToString(), CancellationToken.None);

            if (processResult.Item1 != 0)
            {
                var msg = String.Format(
                    "Failed to modify resources, command invoked was: '{0} {1}'\n\nOutput was:\n{2}",
                    exe, args, processResult.Item2);

                throw new Exception(msg);
            }
            else
            {
                Console.WriteLine(processResult.Item2);
            }
        }

        static async Task createMsiPackage(string setupExe, IPackage package)
        {
            var pathToWix = pathToWixTools();
            var setupExeDir = Path.GetDirectoryName(setupExe);
            var company = String.Join(",", package.Authors);

            var templateText = File.ReadAllText(Path.Combine(pathToWix, "template.wxs"));
            var templateData = new Dictionary<string, string> {
                { "Id", package.Id },
                { "Title", package.Title },
                { "Author", company },
                { "Version", Regex.Replace(package.Version.ToString(), @"-.*$", "") },
                { "Summary", package.Summary ?? package.Description ?? package.Id },
            };

            // NB: We need some GUIDs that are based on the package ID, but unique (i.e.
            // "Unique but consistent").
            for (int i = 1; i <= 10; i++)
            {
                templateData[String.Format("IdAsGuid{0}", i)] = Utility.CreateGuidFromHash(String.Format("{0}:{1}", package.Id, i)).ToString();
            }

            var templateResult = CopStache.Render(templateText, templateData);

            var wxsTarget = Path.Combine(setupExeDir, "Setup.wxs");
            File.WriteAllText(wxsTarget, templateResult, Encoding.UTF8);

            var candleParams = String.Format("-nologo -ext WixNetFxExtension -out \"{0}\" \"{1}\"", wxsTarget.Replace(".wxs", ".wixobj"), wxsTarget);
            var processResult = await Utility.InvokeProcessAsync(
                Path.Combine(pathToWix, "candle.exe"), candleParams, CancellationToken.None, setupExeDir);

            if (processResult.Item1 != 0)
            {
                var msg = String.Format(
                    "Failed to compile WiX template, command invoked was: '{0} {1}'\n\nOutput was:\n{2}",
                    "candle.exe", candleParams, processResult.Item2);

                throw new Exception(msg);
            }

            var lightParams = String.Format("-ext WixNetFxExtension -sval -out \"{0}\" \"{1}\"", wxsTarget.Replace(".wxs", ".msi"), wxsTarget.Replace(".wxs", ".wixobj"));
            processResult = await Utility.InvokeProcessAsync(
                Path.Combine(pathToWix, "light.exe"), lightParams, CancellationToken.None, setupExeDir);

            if (processResult.Item1 != 0)
            {
                var msg = String.Format(
                    "Failed to link WiX template, command invoked was: '{0} {1}'\n\nOutput was:\n{2}",
                    "light.exe", lightParams, processResult.Item2);

                throw new Exception(msg);
            }

            var toDelete = new[] {
                wxsTarget,
                wxsTarget.Replace(".wxs", ".wixobj"),
                wxsTarget.Replace(".wxs", ".wixpdb"),
            };

            await Utility.ForEachAsync(toDelete, x => Utility.DeleteFileHarder(x));
        }

        static string pathToWixTools()
        {
            var ourPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Same Directory? (i.e. released)
            if (File.Exists(Path.Combine(ourPath, "candle.exe")))
            {
                return ourPath;
            }

            // Debug Mode (i.e. in vendor)
            var debugPath = Path.Combine(ourPath, "..", "..", "..", "vendor", "wix", "candle.exe");
            if (File.Exists(debugPath))
            {
                return Path.GetFullPath(debugPath);
            }

            throw new Exception("WiX tools can't be found");
        }

        static string getAppNameFromDirectory(string path = null)
        {
            path = path ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return (new DirectoryInfo(path)).Name;
        }

        static ShortcutLocation? parseShortcutLocations(string shortcutArgs)
        {
            var ret = default(ShortcutLocation?);

            if (!String.IsNullOrWhiteSpace(shortcutArgs))
            {
                var args = shortcutArgs.Split(new[] { ',' });

                foreach (var arg in args)
                {
                    var location = (ShortcutLocation)(Enum.Parse(typeof(ShortcutLocation), arg, false));
                    if (ret.HasValue)
                    {
                        ret |= location;
                    }
                    else
                    {
                        ret = location;
                    }
                }
            }

            return ret;
        }

        static int consoleCreated = 0;
        static void ensureConsole()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) return;

            if (Interlocked.CompareExchange(ref consoleCreated, 1, 0) == 1) return;

            if (!NativeMethods.AttachConsole(-1))
            {
                NativeMethods.AllocConsole();
            }

            NativeMethods.GetStdHandle(StandardHandles.STD_ERROR_HANDLE);
            NativeMethods.GetStdHandle(StandardHandles.STD_OUTPUT_HANDLE);
        }

        static bool IgnoreExeShortcut(string exeName)
        {
            if (exeName.Contains("Update.exe")) return true;
            if (exeName.Contains("UnityCrashHandler64.exe")) return true;
            return false;
        }

        /// <summary>
        /// 获取指定文件夹的大小
        /// </summary>
        /// <param name="dirPath">文件夹路径</param>
        /// <returns></returns>
        public static long GetDirectoryLength(string dirPath)
        {
            if (!Directory.Exists(dirPath)) return 0;

            long len = 0;
            DirectoryInfo di = new DirectoryInfo(dirPath);
            foreach (FileInfo fi in di.GetFiles())
            {
                len += fi.Length;
            }

            DirectoryInfo[] dis = di.GetDirectories();
            if (dis.Length > 0)
            {
                for (int i = 0; i < dis.Length; i++)
                {
                    len += GetDirectoryLength(dis[i].FullName);
                }
            }
            return len;
        }
    }

    public class ProgressSource
    {
        public event EventHandler<int> Progress;

        public void Raise(int i)
        {
            if (Progress != null)
                Progress.Invoke(this, i);
        }
    }

    class SetupLogLogger : Splat.ILogger, IDisposable
    {
        TextWriter inner;
        readonly object gate = 42;
        public Splat.LogLevel Level { get; set; }

        public SetupLogLogger(bool saveInTemp)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var dir = saveInTemp ?
                        Path.GetTempPath() :
                        Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                    var file = Path.Combine(dir, String.Format("Update.{0}.log", i).Replace(".0.log", ".log"));
                    var str = File.Open(file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    inner = new StreamWriter(str, Encoding.UTF8, 4096, false);
                    return;
                }
                catch (Exception ex)
                {
                    // Didn't work? Keep going
                    Console.Error.WriteLine("Couldn't open log file, trying new file: " + ex.ToString());
                }
            }

            inner = Console.Error;
        }

        public void Write(string message, LogLevel logLevel)
        {
            if (logLevel < Level)
            {
                return;
            }

            lock (gate) inner.WriteLine("{0}> {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message);
        }

        public void Dispose()
        {
            lock (gate)
            {
                inner.Flush();
                inner.Dispose();
            }
        }
    }
}
