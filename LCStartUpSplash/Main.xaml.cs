using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace LCStartUpSplash
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main
    {
        string _executingDirectory = "";
        private const string Programname = "LegendaryClient";
        private const string ProgramAbr = "LC";

        string dlLink;
        public Main(MainWindow mWindow)
        {
            InitializeComponent();
            var bgThead = new Thread(() =>
            {
                _executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    ProgramTitle.Content = Programname;
                    Visibility = Visibility.Visible;
                }));
                var currentVersion = RetreiveLatestInstall();
                //Cannot read file, abort mission
                if (currentVersion == null)
                {
                    Dispatcher.BeginInvoke(
                        DispatcherPriority.Input, new ThreadStart(() =>
                            {
                                mWindow.Visibility = Visibility.Hidden;
                            }));
                    MessageBox.Show("Unable to read version file. Please re-install " + Programname + " or try running this program as admin",
                        Programname + " Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Environment.Exit(0);
                }
                else
                {
                    if (currentVersion != RetreveLatestVersion())
                    {
                        if (_executingDirectory != null) 
                        {
                            var client = Path.Combine(_executingDirectory, "Client");
                            if (Directory.Exists(client))
                                Directory.Delete(client, true);
                        }

                        InstallLatest(new Uri(dlLink));
                    }
                    else
                    {
                        LaunchLc();
                    }
                }
            });

            bgThead.Start();
        }
        private void LaunchLc()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                Current.Content = "Launching LegenadryClient...";
            }));
            Thread.Sleep(5000);
            try
            {
                var p = new Process
                {
                    StartInfo =
                    {
                        FileName = Path.Combine(_executingDirectory, "Client", "LegendaryClient.exe")
                    }
                };
                p.Start();
            }
            catch
            {
                MessageBox.Show("Failed to launch LegendaryClient. Please try re-installing LegendaryClient onto your computer.",
                    Programname + " Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            Environment.Exit(0);
        }
        private string RetreveLatestVersion()
        {
            try
            {
                using (var client = new WebClient())
                {
                    var json = client.DownloadString("http://eddy5641.github.io/LegendaryClient/Releases.Json");
                    var deserializedJson = new JavaScriptSerializer().Deserialize<RootObject>(json);

                    foreach (var versions in deserializedJson.LcVersions.Where(versions => versions.IsLatest)) 
                    {
                        dlLink = versions.DownloadLink;
                        return versions.VersionId;
                    }
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        private void InstallLatest(Uri dlUri)
        {
            var path = Path.Combine(_executingDirectory, "temp");
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            Directory.CreateDirectory(Path.Combine(_executingDirectory, "temp"));
            using (var client = new WebClient())
            {
                client.DownloadProgressChanged += ClientDownloadProgressChanged;
                client.DownloadFileCompleted += ClientDownloadFileCompleted;
                client.DownloadFileAsync(dlUri, Path.Combine(_executingDirectory, "temp", "Client.zip"));
            }
        }

        void ClientDownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                ZipFile.ExtractToDirectory(Path.Combine(_executingDirectory, "temp", "Client.zip"), Path.Combine(_executingDirectory, "temp"));
                Directory.Move(Path.Combine(_executingDirectory, "temp", "Client"), Path.Combine(_executingDirectory, "Client"));
                Directory.Delete(Path.Combine(_executingDirectory, "temp"), true);
                LaunchLc();
            }
            catch
            {
                MessageBox.Show("Unable to extract LC. Try reinstall",
                    Programname + " Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            
        }

        void ClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                Current.Content = "Updating LegendaryClient";
                var bytesIn = double.Parse(e.BytesReceived.ToString());
                var totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                var percentage = bytesIn / totalBytes * 100;
                var half = int.Parse(Math.Truncate(percentage).ToString(CultureInfo.InvariantCulture));
                Progress.Value = (half);
            }));
        }
        private string RetreiveLatestInstall()
        {
            if (File.Exists(Path.Combine(_executingDirectory, "Client", ProgramAbr + "Version.Version")))
            {
                try
                {
                    var sr =
                        new StreamReader(Path.Combine(_executingDirectory, "Client", ProgramAbr + "Version.Version"));
                    return sr.ReadToEnd();
                }
                catch
                {
                    return null;
                }
            }
            else if (File.Exists(Path.Combine(_executingDirectory, "Client", "LegendaryClient.exe")))
            {
                try
                {
                    return
                        FileVersionInfo.GetVersionInfo(
                            Path.Combine(_executingDirectory, "Client", "LegendaryClient.exe")).FileVersion;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
    }
    public class LcVersion
    {
        public bool IsLatest { get; set; }
        public string VersionId { get; set; }
        public string VersionName { get; set; }
        public string VersionDescription { get; set; }
        public string DownloadLink { get; set; }
        public bool IsPrerelease { get; set; }
        public bool IsBeta { get; set; }
    }

    public class RootObject
    {
        public List<LcVersion> LcVersions { get; set; }
    }
}