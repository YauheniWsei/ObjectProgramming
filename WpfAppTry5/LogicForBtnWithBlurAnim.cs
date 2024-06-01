using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows;

namespace WpfAppTry5
{
    public class LogicForBtnWithBlurAnim
    {


        public LogicForBtnWithBlurAnim(UIElement gifLoadingElement)
        {
            gifLoading = gifLoadingElement;
        }

        private Timer timer;
        private Random random = new Random();
        public UIElement gifLoading;

        public void BlurEffectAfterClickBtn(List<Grid> gridsToShow, List<Grid> gridsToHide)
        {
            double randomDelay = random.NextDouble() * (4.0 - 2.8) + 2.8; // Случайное время от 2.8 до 4 секунд

            foreach (var grid in gridsToHide)
            {
                BlurEffect blurEffect = new BlurEffect
                {
                    Radius = 0 // Начальный радиус размытия
                };

                DoubleAnimation blurAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 100, // Максимальный радиус размытия
                    Duration = new Duration(TimeSpan.FromSeconds(3))
                };

                grid.Effect = blurEffect;
                blurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);
            }

            gifLoading.Visibility = Visibility.Visible;
            
            TimerCallback callback = new TimerCallback(OnTimerElapsed);
            timer = new Timer(callback, new object[] { gridsToShow, gridsToHide }, (int)(randomDelay * 1000), Timeout.Infinite);
        }

        public void OnTimerElapsed(object state)
        {
            object[] parameters = (object[])state;
            List<Grid> gridsToShow = (List<Grid>)parameters[0];
            List<Grid> gridsToHide = (List<Grid>)parameters[1];

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var grid in gridsToShow)
                {
                    grid.Visibility = Visibility.Visible;
                }

                gifLoading.Visibility = Visibility.Hidden;

                foreach (var grid in gridsToHide)
                {
                    grid.Effect = null;
                    grid.Visibility = Visibility.Hidden;
                }
            }));

            timer.Dispose();
        }

    }
}
