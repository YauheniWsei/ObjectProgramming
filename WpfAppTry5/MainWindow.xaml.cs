using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Timers;
using Timer = System.Threading.Timer;
using System.Windows.Shapes;

namespace WpfAppTry5
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Start1 start1 = new Start1();
        private Timer timer;

        private void ButClick_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                To = 58,
                Duration = new Duration(TimeSpan.FromSeconds(0.2))
            };

            ButClick.BeginAnimation(Button.FontSizeProperty, animation);

        }

        private void ButClick_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                To = 50,
                Duration = new Duration(TimeSpan.FromSeconds(0.2))
            };

            ButClick.BeginAnimation(Button.FontSizeProperty, animation);
        }

        private void ButClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //private void ButClick_Click(object sender, RoutedEventArgs e)
        //{
        //    DoubleAnimation blurAnimation = new DoubleAnimation();
        //    blurAnimation.From = 0;
        //    blurAnimation.To = 100; 
        //    blurAnimation.Duration = new Duration(TimeSpan.FromSeconds(3));

        //    BlurEffect blurEffect = new BlurEffect();
        //    blurEffect.Radius = 0; 

        //    
        //    LogOutPng.Effect = blurEffect;
        //    NameOfApp.Effect = blurEffect;
        //    FootBut.Effect = blurEffect;
        //    ImageGifBack.Effect = blurEffect;

        //    blurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);

        //    GifLoading.Visibility = Visibility.Visible;
        //    GridAfterBut.Visibility = Visibility.Visible;


        //    timer = new Timer(OnTimerElapsed, null, 5000, Timeout.Infinite);
        //}

        private void ButClick_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsEnabled = false;
            DoubleAnimation blurAnimation = new DoubleAnimation();
            blurAnimation.From = 0;
            blurAnimation.To = 100; // Устанавливаем радиус размытия
            blurAnimation.Duration = new Duration(TimeSpan.FromSeconds(3));

            BlurEffect blurEffect = new BlurEffect();
            blurEffect.Radius = 0; // Начальный радиус размытия (может быть 0, если вы хотите начать с неразмытого состояния)

            // Применяем размытие к окну
            LogOutPng.Effect = blurEffect;
            NameOfApp.Effect = blurEffect;
            FootBut.Effect = blurEffect;
            ImageGifBack.Effect = blurEffect;

            blurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);

            GifLoading.Visibility = Visibility.Visible;
            GridAfterBut.Visibility = Visibility.Visible;


            timer = new Timer(OnTimerElapsed, null, 6000, Timeout.Infinite);
        }

        private void OnTimerElapsed(object state)
        {
            Dispatcher.Invoke(() =>
            {
                this.Close();
                start1.Show();
            });
        }

    }
}
