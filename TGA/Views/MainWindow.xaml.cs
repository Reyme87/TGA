using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TGA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ((INotifyCollectionChanged)MessagesListBox.Items).CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object? sender, EventArgs e)
        {
            Border border = (Border)VisualTreeHelper.GetChild(MessagesListBox, 0);
            ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
            scrollViewer.ScrollToBottom();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.From = 0;
            anim.To = 3;
            anim.AutoReverse = true;
            anim.Duration = TimeSpan.FromSeconds(1.5);
            anim.Completed += (s, e) => NotificationBorder.Visibility = Visibility.Hidden;
            anim.EasingFunction = new QuadraticEase();
            NotificationBorder.Visibility = Visibility.Visible;
            NotificationBorder.BeginAnimation(OpacityProperty, anim);
        }
    }
}