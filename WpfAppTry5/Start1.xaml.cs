using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Image = System.Windows.Controls.Image;
using System.Windows.Threading;
using System.Xml;
using Newtonsoft.Json;
using System.IO;
using System.Globalization;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows.Navigation;

namespace WpfAppTry5
{
    /// <summary>
    /// Logika interakcji dla klasy Start1.xaml
    /// </summary>
    public partial class Start1 : Window
    {
        public static List<Achievement> Achievements { get; private set; }
        public Start1()
        {
            InitializeComponent();
            DisplayMachineName();
            SetJoinDate();
            InitializeTimeTracking();
            LoadAchievements();
            LoadAchievementsToUI();
            LoadCompletedLessons();
            UpdateCompletedLessons();
            logicForBtnWithBlurAnim = new LogicForBtnWithBlurAnim(GifLoading);
        }

        public static void InitializeAchievements()
        {
            Achievements = new List<Achievement>
            {
                new Achievement { Title = "First Steps", Description = "Complete your first lesson." },
                new Achievement { Title = "Consistency", Description = "Log in for 7 consecutive days." },
                new Achievement { Title = "Explorer", Description = "Complete lessons in 3 different categories." }
                // Add more achievements as needed
            };

            SaveAchievements(); // Save initial achievements if they do not exist
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        public static void UnlockAchievement(string title)
        {
            
            var achievement = Achievements.FirstOrDefault(a => a.Title == title);

            if (achievement != null)
            {
                achievement.IsUnlocked = true;
                achievement.Title = $"{title}   Done!";
                // Save the unlocked achievements to persistent storage
                SaveAchievements();
            }
        }
        private void UpdateCompletedLessons()
        {
            CompletedLess.Text = $"Completed Lessons: {_counterClicks}";
        }

        private void SaveCompletedLessons()
        {
            string json = JsonConvert.SerializeObject(_counterClicks);
            File.WriteAllText("completedLessons.json", json);
        }

        private void LoadCompletedLessons()
        {
            if (File.Exists("completedLessons.json"))
            {
                string json = File.ReadAllText("completedLessons.json");
                _counterClicks = JsonConvert.DeserializeObject<int>(json);
            }
            else
            {
                _counterClicks = 0;
            }
        }
        private void LessonCompleted()
        {
            _counterClicks++;
            SaveCompletedLessons();
            UpdateCompletedLessons();
            is3CounterClicks();
        }

        private int _counterClicks { get; set; }
        private void is3CounterClicks()
        {
            if (_counterClicks == 3)
            {
                UnlockAchievement("Explorer");
            }
        }

        private static void SaveAchievements()
        {
            string json = JsonConvert.SerializeObject(Achievements, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("achievements.json", json);
        }

        private static void LoadAchievements()
        {
            if (File.Exists("achievements.json"))
            {
                string json = File.ReadAllText("achievements.json");
                Achievements = JsonConvert.DeserializeObject<List<Achievement>>(json);
            }
            else
            {
                InitializeAchievements();
            }
        }

        private void LoadAchievementsToUI()
        {
            AchievementsItemsControl.ItemsSource = Achievements;
        }

        private bool _lesson1AlphabetIsCompleted = false;

        private void DisplayMachineName()
        {
            string machineName = Environment.MachineName;
            MachinName.Text = $"User: {machineName}";
        }


        private void SetJoinDate()
        {
            // Чтение даты присоединения из конфигурации
            var joinDate = ConfigurationManager.AppSettings["JoinDate"];

            // Если даты нет в конфигурации, установить текущую дату
            if (string.IsNullOrEmpty(joinDate))
            {
                joinDate = DateTime.Now.ToString("yyyy-MM-dd");
                // Сохранить дату присоединения в конфигурации
                SaveJoinDate(joinDate);
            }

            // Отобразить дату присоединения в TextBlock
            JoinDateTextBlock.Text = $"Joined: {joinDate}";
        }

        private void SaveJoinDate(string joinDate)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("JoinDate");
            config.AppSettings.Settings.Add("JoinDate", joinDate);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }


        private LogicForBtnWithBlurAnim logicForBtnWithBlurAnim;

        private System.Timers.Timer timer;
        Random random = new Random();

        // Переменные для отслеживания времени

        private DateTime _startTime;
        private TimeSpan _totalTimeSpent;
        private DispatcherTimer _timer;

        private void InitializeTimeTracking()
        {
            // Чтение общего времени из конфигурации
            var totalTimeString = ConfigurationManager.AppSettings["TotalTimeSpent"];
            _totalTimeSpent = string.IsNullOrEmpty(totalTimeString)
                ? TimeSpan.Zero
                : TimeSpan.Parse(totalTimeString);

            // Обновление текстового блока
            UpdateTotalTimeTextBlock();

            // Установка времени старта
            _startTime = DateTime.Now;

            // Инициализация и запуск таймера
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Обновление времени, проведённого в приложении
            _totalTimeSpent += DateTime.Now - _startTime;
            _startTime = DateTime.Now;

            // Обновление текстового блока
            UpdateTotalTimeTextBlock();
        }

        private void UpdateTotalTimeTextBlock()
        {
            TotalTimeTextBlock.Text = $"Time Spent in App: { _totalTimeSpent:hh\\:mm\\:ss}";
        }

        private void SaveTotalTimeSpent()
        {
            // Сохранение общего времени в конфигурации
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("TotalTimeSpent");
            config.AppSettings.Settings.Add("TotalTimeSpent", _totalTimeSpent.ToString());
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            // Сохранение времени при закрытии приложения
            SaveTotalTimeSpent();
        }

        private void ButClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }


        private void ChangeFontSizeOnMouseEnter(TextBlock textBlock, double fontSizeTo)
        {
            if (textBlock != null)
            {
                DoubleAnimation animation = new DoubleAnimation
                {
                    To = fontSizeTo,
                    Duration = new Duration(TimeSpan.FromSeconds(0.2))
                };

                textBlock.BeginAnimation(TextBlock.FontSizeProperty, animation);
            }
        }

        private void OpenUrl(string url)
        {
            System.Diagnostics.Process.Start(url);
        }

        private void ChangePaddingOnMouseEnter(TextBlock textBlock, Thickness paddingTo)
        {
            if (textBlock != null)
            {
                ThicknessAnimation animation = new ThicknessAnimation
                {
                    To = paddingTo,
                    Duration = new Duration(TimeSpan.FromSeconds(0.2))
                };

                textBlock.BeginAnimation(TextBlock.PaddingProperty, animation);
            }
        }

        private void ChangeBtnSizeOnMouseEnter(Button btn, double widthTo, double heightTo)
        {
            if (btn != null)
            {
                DoubleAnimation widthAnimation = new DoubleAnimation
                {
                    To = widthTo,
                    Duration = new Duration(TimeSpan.FromSeconds(0.2))
                };

                DoubleAnimation heightAnimation = new DoubleAnimation
                {
                    To = heightTo,
                    Duration = new Duration(TimeSpan.FromSeconds(0.2))
                };

                btn.BeginAnimation(Button.WidthProperty, widthAnimation);
                btn.BeginAnimation(Button.HeightProperty, heightAnimation);
            }
        }

        private void ChangeImgSizeOnMouseEnter(Image img, double widthTo, double heightTo)
        {
            if (img != null)
            {
                DoubleAnimation widthAnimation = new DoubleAnimation
                {
                    To = widthTo,
                    Duration = new Duration(TimeSpan.FromSeconds(0.2))
                };

                DoubleAnimation heightAnimation = new DoubleAnimation
                {
                    To = heightTo,
                    Duration = new Duration(TimeSpan.FromSeconds(0.2))
                };

                img.BeginAnimation(Image.WidthProperty, widthAnimation);
                img.BeginAnimation(Image.HeightProperty, heightAnimation);
            }
        }

        private void LangLvlBeginner_MouseEnter(object sender, MouseEventArgs e)
        {
            ChangeFontSizeOnMouseEnter(sender as TextBlock, 26);
        }

        private void LangLvlBeginner_MouseLeave(object sender, MouseEventArgs e)
        {
            ChangeFontSizeOnMouseEnter(sender as TextBlock, 22);
        }

        private void LangLvlElementary_MouseEnter(object sender, MouseEventArgs e)
        {
            ChangeFontSizeOnMouseEnter(sender as TextBlock, 26);
        }

        private void LangLvlElementary_MouseLeave(object sender, MouseEventArgs e)
        {
            ChangeFontSizeOnMouseEnter(sender as TextBlock, 22);
        }

        public byte LangLvlForAnimation { get; set; }
        private void LangLvlInter_MouseEnter(object sender, MouseEventArgs e)
        {
            ChangeFontSizeOnMouseEnter(sender as TextBlock, 26);
        }

        private void LangLvlInter_MouseLeave(object sender, MouseEventArgs e)
        {
            ChangeFontSizeOnMouseEnter(sender as TextBlock, 22);
        }

        private void LangLvlUpInter_MouseEnter(object sender, MouseEventArgs e)
        {
            ChangeFontSizeOnMouseEnter(sender as TextBlock, 26);
        }

        private void LangLvlUpInter_MouseLeave(object sender, MouseEventArgs e)
        {
            ChangeFontSizeOnMouseEnter(sender as TextBlock, 22);
        }


        private void NameOfAppTextInHeader_MouseEnter(object sender, MouseEventArgs e)
        {
            ChangePaddingOnMouseEnter(sender as TextBlock, new Thickness(25, 0, 0, 0));
        }

        private void NameOfAppTextInHeader_MouseLeave(object sender, MouseEventArgs e)
        {
            ChangePaddingOnMouseEnter(sender as TextBlock, new Thickness(10, 0, 0, 0));
        }

        private bool isLeftMenuOpen = false;
        private void HideALlRoots()
        {
            StartMainMenu.Visibility = Visibility.Hidden;
            BeginnerMenu.Visibility = Visibility.Hidden;
            ElementaryMenu.Visibility = Visibility.Hidden;
            InterMenu.Visibility = Visibility.Hidden;
            UpInterMenu.Visibility = Visibility.Hidden;
            LeftMenu.Visibility = Visibility.Hidden;
            isLeftMenuOpen = false;
            AwardsGridMenu.Visibility = Visibility.Hidden;
            HelpGridMenu.Visibility = Visibility.Hidden;
            ProfileGridMenu.Visibility = Visibility.Hidden;
            InProgress.Visibility = Visibility.Hidden;
            VideoLessonsGrid.Visibility = Visibility.Hidden;
            VideoLessonsElGrid.Visibility = Visibility.Hidden;
            ReadingGridBeginner.Visibility = Visibility.Hidden;
            ReadingGridEl.Visibility = Visibility.Hidden;
            Lesson_1_Alphabet.Visibility = Visibility.Hidden;
        }

        private void NameOfAppTextInHeader_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HideALlRoots();
            StartMainMenu.Visibility = Visibility.Visible;
        }


        private void LangLvlUpInter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HideALlRoots();
            UpInterMenu.Visibility = Visibility.Visible;
        }

        private void LangLvlInter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HideALlRoots();
            InterMenu.Visibility = Visibility.Visible;
        }

        public void OpenElementaryMenu()
        {
            HideALlRoots();
            if (RadioVideo.IsChecked == true)
            {
                VideoLessonsElGrid.Visibility = Visibility.Visible;
            }
            else if (RadioReading.IsChecked == true)
            {
                ReadingGridEl.Visibility = Visibility.Visible;
            }
            else
            {
                ElementaryMenu.Visibility = Visibility.Visible;
            }

        }
        private void LangLvlElementary_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenElementaryMenu();
        }


        public void OpenBeginnerMenu()
        {

            HideALlRoots();
            if (RadioVideo.IsChecked == true)
            {
                VideoLessonsGrid.Visibility = Visibility.Visible;
            }
            else if (RadioReading.IsChecked == true)
            {
                ReadingGridBeginner.Visibility = Visibility.Visible;
            }
            else
            {
                BeginnerMenu.Visibility = Visibility.Visible;
            }
        }
        private void LangLvlBeginner_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenBeginnerMenu();

        }

        private void ProfileTextInSettings_MouseEnter(object sender, MouseEventArgs e)
        {
            ChangePaddingOnMouseEnter(sender as TextBlock, new Thickness(30, 0, 0, 0));
            rectProfile.Opacity = 1;
        }

        private void ProfileTextInSettings_MouseLeave(object sender, MouseEventArgs e)
        {
            ChangePaddingOnMouseEnter(sender as TextBlock, new Thickness(20, 0, 0, 0));
            rectProfile.Opacity = 0.6;
        }

        private void AchievementsTextInSettings_MouseEnter(object sender, MouseEventArgs e)
        {
            ChangePaddingOnMouseEnter(sender as TextBlock, new Thickness(30, 0, 0, 0));
            rectRewards.Opacity = 1;
        }

        private void AchievementsTextInSettings_MouseLeave(object sender, MouseEventArgs e)
        {
            ChangePaddingOnMouseEnter(sender as TextBlock, new Thickness(20, 0, 0, 0));
            rectRewards.Opacity = 0.6;
        }

        private void HelpTextInSettings_MouseEnter(object sender, MouseEventArgs e)
        {
            ChangePaddingOnMouseEnter(sender as TextBlock, new Thickness(30, 0, 0, 0));
            rectHelp.Opacity = 1;
        }

        private void HelpTextInSettings_MouseLeave(object sender, MouseEventArgs e)
        {
            ChangePaddingOnMouseEnter(sender as TextBlock, new Thickness(20, 0, 0, 0));
            rectHelp.Opacity = 0.6;
        }

        private void OpenLeftMenu_Click(object sender, RoutedEventArgs e)
        {
            isLeftMenuOpen = !isLeftMenuOpen;
            if (isLeftMenuOpen)
            {
                LeftMenu.Visibility = Visibility.Visible;
            }
            else
            {
                LeftMenu.Visibility = Visibility.Hidden;
            }
        }

        private void ButtonInLeftMenuClicks(Grid root)
        {
            HideALlRoots();
            root.Visibility = Visibility.Visible;
        }


        private void ButtonProfile_Click(object sender, RoutedEventArgs e)
        {
            ButtonInLeftMenuClicks(ProfileGridMenu);
        }

        private void ButtonRewards_Click(object sender, RoutedEventArgs e)
        {
            ButtonInLeftMenuClicks(AwardsGridMenu);
        }

        private void AchievementsTextInSettings_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ButtonInLeftMenuClicks(AwardsGridMenu);
        }
        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            ButtonInLeftMenuClicks(HelpGridMenu);
        }

        private void HelpTextInSettings_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ButtonInLeftMenuClicks(HelpGridMenu);
        }

        private void Rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            RectLess1.Opacity = 1;
        }

        private void RectLess1_MouseLeave(object sender, MouseEventArgs e)
        {
            RectLess1.Opacity = 0.6;
        }
        private void RectLess2_MouseEnter(object sender, MouseEventArgs e)
        {
            RectLess2.Opacity = 1;
        }

        private void RectLess2_MouseLeave(object sender, MouseEventArgs e)
        {
            RectLess2.Opacity = 0.6;
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            RectLess1.Opacity = 1;
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            RectLess1.Opacity = 0.6;
        }

        private void TextBlock_MouseEnter_1(object sender, MouseEventArgs e)
        {
            RectLess1.Opacity = 1;
        }

        private void TextBlock_MouseLeave_1(object sender, MouseEventArgs e)
        {
            RectLess1.Opacity = 0.6;
        }

        private void TextBlock_MouseEnter_2(object sender, MouseEventArgs e)
        {
            RectLess1.Opacity = 1;
        }

        private void TextBlock_MouseLeave_2(object sender, MouseEventArgs e)
        {
            RectLess1.Opacity = 0.6;
        }

        private void TextBlock_MouseEnter_3(object sender, MouseEventArgs e)
        {
            RectLess2.Opacity = 1;
        }

        private void TextBlock_MouseLeave_3(object sender, MouseEventArgs e)
        {
            RectLess2.Opacity = 0.6;
        }

        private void TextBlock_MouseEnter_4(object sender, MouseEventArgs e)
        {
            RectLess2.Opacity = 1;
        }

        private void TextBlock_MouseLeave_4(object sender, MouseEventArgs e)
        {
            RectLess2.Opacity = 0.6;
        }

        private void RectLess1El_MouseEnter(object sender, MouseEventArgs e)
        {
            RectLess1El.Opacity = 1;
        }

        private void RectLess1El_MouseLeave(object sender, MouseEventArgs e)
        {
            RectLess1El.Opacity = 0.6;
        }

        private void lessRadioRect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RadioLessons.IsChecked = true;
            
        }

        private void readRadioRect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RadioReading.IsChecked = true;
            
        }

        private void videoRadioRect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RadioVideo.IsChecked = true;
            
        }

        private void rectProfile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ButtonInLeftMenuClicks(ProfileGridMenu);
        }

        private void rectProfile_MouseEnter(object sender, MouseEventArgs e)
        {
            rectProfile.Opacity = 1;
            ProfileTextInSettings_MouseEnter(ProfileTextInSettings, e);
        }

        private void rectProfile_MouseLeave(object sender, MouseEventArgs e)
        {
            rectProfile.Opacity = 0.6;
            ProfileTextInSettings_MouseLeave(ProfileTextInSettings, e);
        }

        private void rectRewards_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ButtonInLeftMenuClicks(AwardsGridMenu);
        }

        private void rectRewards_MouseEnter(object sender, MouseEventArgs e)
        {
            rectRewards.Opacity = 1;
            AchievementsTextInSettings_MouseEnter(AchievementsTextInSettings, e);
        }

        private void rectRewards_MouseLeave(object sender, MouseEventArgs e)
        {
            rectRewards.Opacity = 0.6;
            AchievementsTextInSettings_MouseLeave(AchievementsTextInSettings, e);
        }

        private void rectHelp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ButtonInLeftMenuClicks(HelpGridMenu);
        }

        private void rectHelp_MouseEnter(object sender, MouseEventArgs e)
        {
            rectHelp.Opacity = 1;
            HelpTextInSettings_MouseEnter(HelpTextInSettings, e);
        }

        private void rectHelp_MouseLeave(object sender, MouseEventArgs e)
        {
            rectHelp.Opacity = 0.6;
            HelpTextInSettings_MouseLeave(HelpTextInSettings, e);
        }

        private void RectBegin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ButtonInLeftMenuClicks(BeginnerMenu);
        }

        private void RectBegin_MouseEnter(object sender, MouseEventArgs e)
        {
            LangLvlBeginner_MouseEnter(LangLvlBeginner, e);
        }

        private void RectBegin_MouseLeave(object sender, MouseEventArgs e)
        {
            LangLvlBeginner_MouseLeave(LangLvlBeginner, e);
        }

        private void RectElementary_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenElementaryMenu();
        }

        private void RectElementary_MouseEnter(object sender, MouseEventArgs e)
        {
            LangLvlElementary_MouseEnter(LangLvlElementary, e);
        }

        private void RectElementary_MouseLeave(object sender, MouseEventArgs e)
        {
            LangLvlElementary_MouseLeave(LangLvlElementary, e);
        }

        private void RectInter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ButtonInLeftMenuClicks(InterMenu);
        }

        private void RectInter_MouseEnter(object sender, MouseEventArgs e)
        {
            LangLvlInter_MouseEnter(LangLvlInter, e);
        }

        private void RectInter_MouseLeave(object sender, MouseEventArgs e)
        {
            LangLvlInter_MouseLeave(LangLvlInter, e);
        }

        private void RectUpInter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ButtonInLeftMenuClicks(UpInterMenu);
        }

        private void RectUpInter_MouseEnter(object sender, MouseEventArgs e)
        {
            LangLvlUpInter_MouseEnter(LangLvlUpInter, e);
        }

        private void RectUpInter_MouseLeave(object sender, MouseEventArgs e)
        {
            LangLvlUpInter_MouseLeave(LangLvlUpInter, e);
        }

        private void BtnYoutube1_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.youtube.com/watch?v=zpJMaW8B4kw");
            LessonCompleted();
        }

        private void BtnYoutube2_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.youtube.com/watch?v=LgFzb-zpENs");
            LessonCompleted();
        }

        private void BtnYoutube3_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.youtube.com/watch?v=J0Mwmb0pghA");
            LessonCompleted();
        }

        private void RadioLessons_Click(object sender, RoutedEventArgs e)
        {
            OpenBeginnerMenu();
        }

        private void RadioReading_Click(object sender, RoutedEventArgs e)
        {
            OpenBeginnerMenu();
        }

        private void RadioVideo_Click(object sender, RoutedEventArgs e)
        {
            OpenBeginnerMenu();
        }

        private void BtnYoutube4_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.youtube.com/watch?v=QhhI0dyqbpE");
            LessonCompleted();
        }

        private void BtnYoutube5_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.youtube.com/watch?v=6VVvEIyIYyo");
            LessonCompleted();
        }

        private void BtnYoutube6_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.youtube.com/watch?v=sSdZA2TciKs&ab_channel=BeFluentinRussian");
            LessonCompleted();
        }

        private void BtnYoutube6El_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.youtube.com/watch?v=4sEZc1_zdPI");
            LessonCompleted();
        }

        private void BtnYoutube5El_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.youtube.com/watch?v=B2zRk4PqMSA");
            LessonCompleted();
        }

        private void BtnYoutube4El_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.youtube.com/watch?v=HzibwVOO3Zk");
            LessonCompleted();
        }

        private void BtnYoutube2El_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.youtube.com/watch?v=e9faloUNgKM");
            LessonCompleted();
        }

        private void BtnYoutube3El_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.youtube.com/watch?v=9vXYmnNLqwE");
            LessonCompleted();
        }

        private void BtnYoutube1El_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.youtube.com/watch?v=kR_G8an7OuQ&list=PLpgpVaWoAiTEF8aNQvPnFCLBrtIeF3tqa&index=36");
            LessonCompleted();
        }

        private void TextBlock_MouseEnter_5(object sender, MouseEventArgs e)
        {
            RectLess1El.Opacity = 1;
        }

        private void TextBlock_MouseLeave_5(object sender, MouseEventArgs e)
        {
            RectLess1El.Opacity = 0.6;
        }

        private void TextBlock_MouseEnter_6(object sender, MouseEventArgs e)
        {
            RectLess1El.Opacity = 1;
        }

        private void TextBlock_MouseLeave_6(object sender, MouseEventArgs e)
        {
            RectLess1El.Opacity = 0.6;
        }

        private void medved_btn_text_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.russianforfree.com/text-russian-bear.php");
            LessonCompleted();
        }

        private void gorod_btn_text_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.russianforfree.com/text-in-russian-beginner-coldest-town-on-earth.php");
            LessonCompleted();
        }

        private void ploshad_btn_text_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.russianforfree.com/text-intermediate-the-red-square.php");
            LessonCompleted();
        }

        private void ghost_btn_text_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.russianforfree.com/text-ghost.php");
            LessonCompleted();
        }

        private void cat_btn_text_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.russianforfree.com/text-cats.php");
            LessonCompleted();
        }

        private void footbal_btn_text_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.russianforfree.com/text-football.php");
            LessonCompleted();
        }

        private void ImageWithAlphabetBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.russianforfree.com/lessons-how-to-read-in-russian-01.php");
            LessonCompleted();
        }

        private void ImageWithAlphabetBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            ChangeBtnSizeOnMouseEnter(ImageWithAlphabetBtn, 450, 320);
        }

        private void ImageWithAlphabetBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            ChangeBtnSizeOnMouseEnter(ImageWithAlphabetBtn, 430, 300);
        }

        private void BtnItsDone_MouseEnter(object sender, MouseEventArgs e)
        {
            ChangeFontSizeOnMouseEnter(BtnItsDone, 34);
        }
        private void BtnItsDone_MouseLeave(object sender, MouseEventArgs e)
        {
            ChangeFontSizeOnMouseEnter(BtnItsDone, 30);
        }


        private async void TimerOn3Sec()
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            IsEnabled = true;
        }

        private async void BtnItsDone_MouseDown(object sender, MouseButtonEventArgs e)
        {
            is3CounterClicks();
            _lesson1AlphabetIsCompleted = true;
            IsEnabled = false;
            List<Grid> gridsToShow = new List<Grid> { BeginnerMenu, PanelWithProfileAndOther, LanguagesLvlMenu };
            List<Grid> gridsToHide = new List<Grid> { Lesson_1_Alphabet };
            logicForBtnWithBlurAnim.BlurEffectAfterClickBtn(gridsToShow, gridsToHide);
            TimerOn3Sec();
            UnlockAchievement("First Steps");
            _counterClicks++;
            UpdateCompletedLessons();
        }

        private async void RectLess1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsEnabled = false;
            List<Grid> gridsToHide = new List<Grid> { BeginnerMenu, PanelWithProfileAndOther, LanguagesLvlMenu };
            List<Grid> gridsToShow = new List<Grid> { Lesson_1_Alphabet };
            logicForBtnWithBlurAnim.BlurEffectAfterClickBtn(gridsToShow, gridsToHide);
            TimerOn3Sec();
        }

        private void BtnItsDoneLess2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            is3CounterClicks();
            IsEnabled= false;
            List<Grid> gridsToShow = new List<Grid> { BeginnerMenu, PanelWithProfileAndOther, LanguagesLvlMenu };
            List<Grid> gridsToHide = new List<Grid> { Lesson_2_PersonalPronouns };
            logicForBtnWithBlurAnim.BlurEffectAfterClickBtn(gridsToShow, gridsToHide);
            TimerOn3Sec();
            _counterClicks++;
            UpdateCompletedLessons();
        }

        private void BorderLess2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(_lesson1AlphabetIsCompleted)
            {
            IsEnabled = false;
            List<Grid> gridsToHide = new List<Grid> { BeginnerMenu, PanelWithProfileAndOther, LanguagesLvlMenu };
            List<Grid> gridsToShow = new List<Grid> { Lesson_2_PersonalPronouns };
            logicForBtnWithBlurAnim.BlurEffectAfterClickBtn(gridsToShow, gridsToHide);
            TimerOn3Sec();
                return;
            }
            else { 
                MessageBox.Show("You need to complete the first lesson to unlock this one!");
            }

        }

        private void BtnItsDoneLess2_MouseEnter(object sender, MouseEventArgs e)
        {
            ChangeFontSizeOnMouseEnter(BtnItsDoneLess2, 34);
        }

        private void BtnItsDoneLess2_MouseLeave(object sender, MouseEventArgs e)
        {
            ChangeFontSizeOnMouseEnter(BtnItsDoneLess2, 30);
        }

        private void BtnItsDoneLess1El_MouseDown(object sender, MouseButtonEventArgs e)
        {
            is3CounterClicks();
            IsEnabled = false;
            List<Grid> gridsToShow = new List<Grid> { ElementaryMenu, PanelWithProfileAndOther, LanguagesLvlMenu };
            List<Grid> gridsToHide = new List<Grid> { Lesson_1_CompoundSentences };
            logicForBtnWithBlurAnim.BlurEffectAfterClickBtn(gridsToShow, gridsToHide);
            TimerOn3Sec();
            _counterClicks++;
            UpdateCompletedLessons();
        }

        private void BtnItsDoneLess1El_MouseEnter(object sender, MouseEventArgs e)
        {
            ChangeFontSizeOnMouseEnter(BtnItsDoneLess1El, 34);
        }

        private void BtnItsDoneLess1El_MouseLeave(object sender, MouseEventArgs e)
        {
            ChangeFontSizeOnMouseEnter(BtnItsDoneLess1El, 30);
        }

        private void RectLess1El_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsEnabled = false;
            List<Grid> gridsToHide = new List<Grid> { ElementaryMenu, PanelWithProfileAndOther, LanguagesLvlMenu };
            List<Grid> gridsToShow = new List<Grid> { Lesson_1_CompoundSentences };
            logicForBtnWithBlurAnim.BlurEffectAfterClickBtn(gridsToShow, gridsToHide);
            TimerOn3Sec();

        }

        private void LanguageInMenuPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            ChangePaddingOnMouseEnter(sender as TextBlock, new Thickness(20, 0, 0, 0));
        }

        private void LanguageInMenuPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            ChangePaddingOnMouseEnter(sender as TextBlock, new Thickness(0, 0, 0, 0));
        }

        private void LanguageInMenuPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Today's we have only this languages for learn:\n\n-> " + (sender as TextBlock).Text);
        }

        private void LinkToFacebookBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.facebook.com/profile.php?id=100055569900071");
        }

        private void LinkToTwiterBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://twitter.com");
        }

        private void LinkToGitHubBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://github.com/YauheniWsei");
        }

        private void LinkToTiktokBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.tiktok.com/@jewxex?lang=pl-PL");
        }

        private void FBImage_MouseEnter(object sender, MouseEventArgs e)
        {
            ChangeImgSizeOnMouseEnter(FBImage, 60, 60);
        }

        private void FBImage_MouseLeave(object sender, MouseEventArgs e)
        {
            ChangeImgSizeOnMouseEnter(FBImage, 50, 50);
        }

        private void TRImage_MouseEnter(object sender, MouseEventArgs e)
        {
            ChangeImgSizeOnMouseEnter(TRImage, 60, 60);
        }

        private void TRImage_MouseLeave(object sender, MouseEventArgs e)
        {
            ChangeImgSizeOnMouseEnter(TRImage, 50, 50);
        }

        private void GHImage_MouseEnter(object sender, MouseEventArgs e)
        {
            ChangeImgSizeOnMouseEnter(GHImage, 60, 60);
        }

        private void GHImage_MouseLeave(object sender, MouseEventArgs e)
        {
            ChangeImgSizeOnMouseEnter(GHImage, 50, 50);
        }

        private void TTImage_MouseEnter(object sender, MouseEventArgs e)
        {
            ChangeImgSizeOnMouseEnter(TTImage, 60, 60);
        }

        private void TTImage_MouseLeave(object sender, MouseEventArgs e)
        {
            ChangeImgSizeOnMouseEnter(TTImage, 50, 50);
        }
    }

    public class Achievement
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsUnlocked { get; set; } = false;
    }

    public class BooleanToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Unlocked" : "Locked";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
