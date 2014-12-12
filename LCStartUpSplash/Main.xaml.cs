using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LCStartUpSplash
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        string ExecutingDirectory;
        string Programname = "LegendaryClient";
        string ProgramAbr = "LC";

        string dlLink;
        public Main()
        {
            InitializeComponent();
            Thread bgThead = new Thread(() =>
            {
                ExecutingDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    ProgramTitle.Content = Programname;
                    this.Visibility = Visibility.Visible;
                }));
                var CurrentVersion = RetreiveLatestInstall();
                //Cannot read file, abort mission
                if (CurrentVersion == null)
                {
                    MessageBox.Show("Unable to read version file. Please re-install " + Programname + " or try running this program as admin",
                        Programname + " Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Environment.Exit(0);
                }
                else
                {
                    if (CurrentVersion != RetreveLatestVersion())
                    {
                        string client = System.IO.Path.Combine(ExecutingDirectory, "Client");
                        if (Directory.Exists(client))
                            Directory.Delete(client, true);

                        InstallLatest(new Uri(dlLink));
                    }
                    else
                    {
                        LaunchLC();
                    }
                }
            });

            bgThead.Start();
        }
        private void LaunchLC()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                Current.Content = "Launching LegenadryClient...";
            }));
            Thread.Sleep(5000);
            try
            {
                var p = new System.Diagnostics.Process();
                p.StartInfo.FileName = System.IO.Path.Combine(ExecutingDirectory, "Client", "LegendaryClient.exe");
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
                using (WebClient client = new WebClient())
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();

                    string Json = client.DownloadString("http://eddy5641.github.io/LegendaryClient/Releases.Json");
                    RootObject deserializedJSON = serializer.Deserialize<RootObject>(Json);

                    foreach (LCVersion versions in deserializedJSON.LCVersions)
                    {
                        if (versions.isLatest)
                        {
                            dlLink = versions.DownloadLink;
                            return versions.VersionId;
                        }
                    }
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        private void InstallLatest(Uri DLLink)
        {
            string path = System.IO.Path.Combine(ExecutingDirectory, "temp");
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            Directory.CreateDirectory(System.IO.Path.Combine(ExecutingDirectory, "temp"));
            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += client_DownloadProgressChanged;
                client.DownloadFileCompleted += client_DownloadFileCompleted;
                client.DownloadFileAsync(DLLink, System.IO.Path.Combine(ExecutingDirectory, "temp", "Client.zip"));
            }
        }

        void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                ZipFile.ExtractToDirectory(System.IO.Path.Combine(ExecutingDirectory, "temp", "Client.zip"), System.IO.Path.Combine(ExecutingDirectory, "Client"));
                Directory.Delete(System.IO.Path.Combine(ExecutingDirectory, "temp"), true);
                LaunchLC();
            }
            catch
            {
                MessageBox.Show("Unable to extract LC. Try reinstall",
                    Programname + " Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                Current.Content = "Updating LegendaryClient";
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                int half = int.Parse(Math.Truncate(percentage).ToString());
                Progress.Value = (half);
            }));
        }
        private string RetreiveLatestInstall()
        {
            if (File.Exists(System.IO.Path.Combine(ExecutingDirectory, "Client", ProgramAbr + "Version.Version")))
            {
                try
                {
                    var sr = new StreamReader(System.IO.Path.Combine(ExecutingDirectory, "Client", ProgramAbr + "Version.Version"));
                    return sr.ReadToEnd();
                }
                catch
                {
                    return null;
                }
            }
            else
                return null;
        }
    }
    public class LCVersion
    {
        public bool isLatest { get; set; }
        public string VersionId { get; set; }
        public string VersionName { get; set; }
        public string VersionDescription { get; set; }
        public string DownloadLink { get; set; }
        public bool IsPrerelease { get; set; }
        public bool IsBeta { get; set; }
    }

    public class RootObject
    {
        public List<LCVersion> LCVersions { get; set; }
    }
}