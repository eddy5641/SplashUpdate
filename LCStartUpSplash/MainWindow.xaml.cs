﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LCStartUpSplash
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// This program updates LegendaryClient
    /// </summary>
    public partial class MainWindow : Window
    {
        string ExecutingDirectory;
        public MainWindow()
        {
            InitializeComponent();
            ExecutingDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var CurrentVersion = RetreiveLatestInstall();
            //Cannot read file, abort mission
            if (CurrentVersion == null)
            {
                MessageBox.Show("Unable to read version file. Please re-install LegendaryClient or try running this program as admin",
                    "LegendaryClient Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Environment.Exit(0);
            }
            else
            {

            }
        }
        private void LaunchLC()
        {
            try
            {

            }
            catch
            {

            }
        }
        private void RetreveLatestLCVersion()
        {

        }
        private void InstallLatestLC(string DLLink)
        {

        }
        private string RetreiveLatestInstall()
        {
            if (File.Exists(System.IO.Path.Combine(ExecutingDirectory, "Client", "LCVersion.Version")))
            {
                try
                {
                    var sr = new StreamReader(System.IO.Path.Combine(ExecutingDirectory, "Client", "LCVersion.Version"));
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

        private void Window_LostFocus(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;

            this.Activate();
            this.Topmost = true;
            this.Topmost = false;
            this.Focus();
        }
    }
}