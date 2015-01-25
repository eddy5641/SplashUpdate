namespace LCStartUpSplash
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// This program updates LegendaryClient
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Main.Content = new Main(this).Content;
        }
    }
}
