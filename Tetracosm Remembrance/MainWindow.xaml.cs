using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using WpfAnimatedGif;

namespace Tetracosm_Remembrance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static Timer TargettingTimer = new Timer();
        static Timer MomentumAdjustmentTimer = new Timer();
        static ThicknessAnimation MoveAnim = new ThicknessAnimation();
        static int Switcher = 0;
        static bool LookingLeft = false;
        static int CurrentBehaviour = 3;
        static int PrevTriggerBehaviour = 0;
        static Random Rand = new Random();
        static double[] Momentum = { 0, 0 };
        static double[] Target = { 0, 0 };


        public static double Clamp(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public MainWindow()
        {
            InitializeComponent();
            Height = SystemParameters.VirtualScreenHeight - 1;
            Width = SystemParameters.PrimaryScreenWidth;

            TargettingTimer.AutoReset = true;
            TargettingTimer.Interval = 3500;
            TargettingTimer.Elapsed += New_Target;
            TargettingTimer.Start();

            MomentumAdjustmentTimer.AutoReset = true;
            MomentumAdjustmentTimer.Interval = 230;
            MomentumAdjustmentTimer.Elapsed += AdjustMomentum;
            MomentumAdjustmentTimer.Start();

            MainObject.Margin = new Thickness(Rand.Next((int)Width), Rand.Next((int)Height), 0, 0);
            Target[0] = MainObject.Margin.Left;
            Target[1] = MainObject.Margin.Top;
            MainObject.RenderTransformOrigin = new Point(0.5, 0.5);

            MoveAnim.Duration = TimeSpan.FromMilliseconds(MomentumAdjustmentTimer.Interval);
        }

        private void AdjustMomentum(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)delegate ()
            {
                Momentum[0] = Clamp(Momentum[0] + (Target[0] - MainObject.Margin.Left) / 180, -50, 50);
                Momentum[1] = Clamp(Momentum[1] + (Target[1] - MainObject.Margin.Top) / 90, -50, 50);
                MoveAnim.To = null;
                if (MainObject.Margin.Left + Momentum[0] < 0 || MainObject.Margin.Left + Momentum[0] > Width - 40)
                {
                    Momentum[0] = Clamp(Momentum[0] + (Target[0] - MainObject.Margin.Left) / 60, -50, 50);
                }
                if (MainObject.Margin.Top + Momentum[1] < 0 || MainObject.Margin.Top + Momentum[1] > Height - 40)
                {
                    Momentum[1] = Clamp(Momentum[1] + (Target[1] - MainObject.Margin.Top) / 30, -50, 50);
                }
                if (Momentum[0] != Math.Abs(Momentum[0]))
                {
                    MainObject.RenderTransform = new ScaleTransform() { ScaleX = -1 };
                    LookingLeft = true;
                }
                else
                {
                    MainObject.RenderTransform = new ScaleTransform() { ScaleX = 1 };
                    LookingLeft = false;
                }
                MoveAnim.From = new Thickness(MainObject.Margin.Left,
                MainObject.Margin.Top, 0, 0);
                switch (CurrentBehaviour)
                {
                    case 0:
                        MoveAnim.By = new Thickness(Momentum[0], Momentum[1], 0, 0);
                        ChangeAnim(0);
                        break;
                    case 1:
                        if (MainObject.Margin.Top > Height - 160)
                        {
                            Momentum[0] = 0;
                            Momentum[1] = -60;
                            MoveAnim.To = new Thickness(MainObject.Margin.Left, Height - 70, 0, 0);
                            ChangeAnim(Rand.Next(3, 10));
                        }
                        else
                        {
                            MoveAnim.By = new Thickness(Momentum[0], Momentum[1], 0, 0);
                        }
                        break;
                    case 2:
                        if (MainObject.Margin.Top > Height - 160)
                        {
                            Momentum[0] = 0;
                            Momentum[1] = -60;
                            MoveAnim.To = new Thickness(MainObject.Margin.Left, Height - 70, 0, 0);
                            ChangeAnim(1);
                        }
                        else
                        {
                            MoveAnim.By = new Thickness(Momentum[0], Momentum[1], 0, 0);

                        }
                        break;
                    case 3:
                        if (MainObject.Margin.Top > Height - 160)
                        {
                            Momentum[0] = 0;
                            Momentum[1] = -60;
                            MoveAnim.To = new Thickness(MainObject.Margin.Left, Height - 70, 0, 0);
                            ChangeAnim(2);
                        }
                        else
                        {
                            MoveAnim.By = new Thickness(Momentum[0], Momentum[1], 0, 0);
                        }
                        break;
                    default:
                        MoveAnim.By = new Thickness(Momentum[0], Momentum[1], 0, 0);
                        break;
                }
                MainObject.BeginAnimation(Image.MarginProperty, MoveAnim, HandoffBehavior.Compose);

                MomentumXDisplay.Content = "XMomentum: " + Math.Round(Momentum[0],1).ToString();
                MomentumYDisplay.Content = "YMomentum: " + Math.Round(Momentum[1],1).ToString();
                XPosition.Content = "LeftMargin: " + Math.Round(MainObject.Margin.Left,1).ToString();
                YPosition.Content = "TopMargin: " + Math.Round(MainObject.Margin.Top,1).ToString();
                SwitcherDisplay.Content = "Switcher: " + Switcher.ToString();
                CurrentBehaviourDisplay.Content = "Current Behaviour: " + CurrentBehaviour.ToString();
                ScreenWidth.Content = "ScreenWidth: " + Width.ToString();
                ScreenHeight.Content = "ScreenHeight: " + Height.ToString();
            });
        }

        private void New_Target(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)delegate ()
            {
                if (Switcher > Rand.Next(7, 36))
                {
                    CurrentBehaviour = Rand.Next(3);
                    MomentumAdjustmentTimer.Interval = 230;
                    Switcher = 0;
                }
                switch (CurrentBehaviour)
                {

                    case 0:
                        Target[0] = Rand.Next(40, ((int)Width - 100));
                        Target[1] = Rand.Next(40, ((int)Height - 100));
                        break;
                    case 1:
                            Target[0] = Rand.Next(40, ((int)Width - 100));
                            Target[1] = (int)Height;
                        break;
                    case 2:
                            Target[0] = 400;
                            Target[1] = (int)Height;
                        break;
                    case 3:
                        Target[0] = Rand.Next(40, ((int)Width - 100));
                        Target[1] = (int)Height;
                        break;
                    default:
                            Target[0] = Rand.Next(40, ((int)Width - 100));
                            Target[1] = Rand.Next(40, ((int)Height - 100));
                        break;
                }
                TargetX.Content = "TargetX: " + Math.Round(Target[0],1).ToString();
                TargetY.Content = "TargetY: " + Math.Round(Target[1],1).ToString();
                Switcher++;
            });
        }

        public void ChangeAnim(int gif)
        {
            if (CurrentBehaviour != PrevTriggerBehaviour || CurrentBehaviour == 1)
            {
                PrevTriggerBehaviour = CurrentBehaviour;
                var image = new BitmapImage();
                image.BeginInit();
                switch (gif)
                {
                    case 0:
                        image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/AriaFly.gif", UriKind.Relative);
                        break;
                    case 1:
                        image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/AriaSing.gif", UriKind.Relative);
                        break;
                    case 2:
                        image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/AriaSleep.gif", UriKind.Relative);
                        break;
                    case 3:
                        image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/AriaBlink.gif", UriKind.Relative);
                        MomentumAdjustmentTimer.Interval = 200;
                        break;
                    case 4:
                        image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/AriaTailMove.gif", UriKind.Relative);
                        MomentumAdjustmentTimer.Interval = 600;
                        break;
                    default:
                        image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/AriaIdle.gif", UriKind.Relative);
                        MomentumAdjustmentTimer.Interval = 1200;
                        break;
                }
                image.EndInit();
                ImageBehavior.SetAnimatedSource(MainObject, image);
            }
        }

        private void DebugToggle(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.RightCtrl)
            {
                if (DebugInfo.Visibility == Visibility.Visible)
                {
                    DebugInfo.Visibility = Visibility.Collapsed;
                }
                else
                {
                    DebugInfo.Visibility = Visibility.Visible;
                }
            }
        }

        private void MainObject_RightClick(object sender, MouseButtonEventArgs e)
        {
            CurrentBehaviour = Rand.Next(3);
            MomentumAdjustmentTimer.Interval = 230;
            Switcher = 0;
        }

        private void MainObject_Flick(object sender, MouseButtonEventArgs e)
        {
            switch (Rand.Next(4))
            {
                case 0:
                    Momentum[0] = 50;
                    break;
                case 1:
                    Momentum[1] = 50;
                    break;
                case 2:
                    Momentum[0] = -50;
                    break;
                case 3:
                    Momentum[1] = -50;
                    break;
            }
        }
    }
}
