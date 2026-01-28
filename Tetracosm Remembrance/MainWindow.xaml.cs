using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using WpfAnimatedGif;

namespace Tetracosm_Remembrance
{
    public partial class MainWindow : Window
    {
        static Random Rand = new Random();
        static Timer TargettingTimer = new Timer();
        static Timer MomentumAdjustmentTimer = new Timer();
        static Timer SingTimer = new Timer();
        static ThicknessAnimation MoveAnim = new ThicknessAnimation();
        static ThicknessAnimation SleepEffectAnim = new ThicknessAnimation()
        {
            BeginTime = TimeSpan.FromMilliseconds(230),
            RepeatBehavior = RepeatBehavior.Forever,
            Duration = TimeSpan.FromSeconds(2)
        };
        static ThicknessAnimation SingEffectAnim = new ThicknessAnimation()
        {
            //BeginTime = TimeSpan.FromMilliseconds(230),
            Duration = TimeSpan.FromSeconds(2),
            FillBehavior = FillBehavior.Stop
        };
        static List<Image> Zs = new List<Image>();
        static List<Image> Notes = new List<Image>();
        static int NextNote = 0;
        static double[] Momentum = { 0, 0 };
        static double[] Target = { 0, 0 };

        static int Switcher = 0;
        static int CurrentBehaviour = 2;
        static int PrevTriggerBehaviour = 0;
        static bool FacingLeft = false;
        static bool LockedFacingDirection = false;
        static bool LockedSinging = false;

        public static double Clamp(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public MainWindow()
        {
            InitializeComponent();
            Height = SystemParameters.VirtualScreenHeight - 1;
            Width = SystemParameters.PrimaryScreenWidth;

            for (int i = 0; i < 3; i++)
            {
                Image Z = new Image()
                {
                    Width = 20,
                    Height = 20,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(-20, -20, 0, 0)
                };
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/Z.gif", UriKind.Relative);
                image.EndInit();
                ImageBehavior.SetAnimatedSource(Z, image);
                RenderOptions.SetBitmapScalingMode(Z, BitmapScalingMode.NearestNeighbor);
                ((Grid)Content).Children.Add(Z);
                Zs.Add(Z);
            }

            for (int i = 0; i < 8; i++)
            {
                Image Note = new Image()
                {
                    Width = 20,
                    Height = 20,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(20, 20, 0, 0)
                };
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/Green Note.gif", UriKind.Relative);
                image.EndInit();
                /*ImageBehavior.SetAutoStart(Note, true);
                ImageBehavior.SetRepeatBehavior(Note, RepeatBehavior.Forever);*/
                ImageBehavior.SetAnimatedSource(Note, image);
                RenderOptions.SetBitmapScalingMode(Note, BitmapScalingMode.NearestNeighbor);
                ((Grid)Content).Children.Add(Note);
                Notes.Add(Note);
            }

            MainObject.Visibility = Visibility.Visible;

            TargettingTimer.AutoReset = true;
            TargettingTimer.Interval = 3500;
            TargettingTimer.Elapsed += New_Target;
            TargettingTimer.Start();

            MomentumAdjustmentTimer.AutoReset = true;
            MomentumAdjustmentTimer.Interval = 230;
            MomentumAdjustmentTimer.Elapsed += AdjustMomentum;
            MomentumAdjustmentTimer.Start();

            SingTimer.AutoReset = true;
            SingTimer.Interval = 1200;
            SingTimer.Elapsed += RandomSing;
            SingTimer.Start();

            if (Rand.Next(2) == 1)
            {
                if (Rand.Next(2) == 1)
                {
                    MainObject.Margin = new Thickness(Rand.Next((int)Width), Rand.Next((int)Height, (int)Height + 500), 0, 0);
                }
                else
                {
                    MainObject.Margin = new Thickness(Rand.Next((int)Width), Rand.Next(-600, -400), 0, 0);
                }
            }
            else
            {
                if (Rand.Next(2) == 1)
                {
                    MainObject.Margin = new Thickness(Rand.Next((int)Width, (int)Width + 500), Rand.Next((int)Height), 0, 0);
                }
                else
                {
                    MainObject.Margin = new Thickness(Rand.Next(-600, -400), Rand.Next((int)Height), 0, 0);
                }
            }
            Target[0] = Rand.Next(40, ((int)Width - 100));
            Target[1] = Rand.Next(40, ((int)Height - 100));
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
                if (Momentum[0] != Math.Abs(Momentum[0]) && !LockedFacingDirection)
                {
                    MainObject.RenderTransform = new ScaleTransform() { ScaleX = -1 };
                    FacingLeft = true;
                }
                else if (!LockedFacingDirection)
                {
                    MainObject.RenderTransform = new ScaleTransform() { ScaleX = 1 };
                    FacingLeft = false;
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

                MomentumXDisplay.Content = "XMomentum: " + Math.Round(Momentum[0], 1).ToString();
                MomentumYDisplay.Content = "YMomentum: " + Math.Round(Momentum[1], 1).ToString();
                XPosition.Content = "LeftMargin: " + Math.Round(MainObject.Margin.Left, 1).ToString();
                YPosition.Content = "TopMargin: " + Math.Round(MainObject.Margin.Top, 1).ToString();
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
                TargetX.Content = "TargetX: " + Math.Round(Target[0], 1).ToString();
                TargetY.Content = "TargetY: " + Math.Round(Target[1], 1).ToString();
                Switcher++;
            });
        }

        public void ChangeAnim(int gif)
        {
            if (CurrentBehaviour != PrevTriggerBehaviour || CurrentBehaviour == 1)
            {
                PrevTriggerBehaviour = CurrentBehaviour;
                LockedFacingDirection = false;
                SleepEffectAnim.From = new Thickness(-20, -20, 0, 0);
                SleepEffectAnim.By = new Thickness(0, 0, 0, 0);
                SleepEffectAnim.BeginTime = TimeSpan.FromMilliseconds(0);
                foreach (Image Z in Zs)
                {
                    Z.BeginAnimation(Image.MarginProperty, SleepEffectAnim);
                }
                var image = new BitmapImage();
                image.BeginInit();
                switch (gif)
                {
                    case 0:
                        image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/AriaFly.gif", UriKind.Relative);
                        break;
                    case 1:
                        image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/AriaSing.gif", UriKind.Relative);
                        LockedFacingDirection = true;

                        break;
                    case 2:
                        image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/AriaSleep.gif", UriKind.Relative);
                        LockedFacingDirection = true;
                        if (FacingLeft)
                        {
                            SleepEffectAnim.From = new Thickness(MainObject.Margin.Left - 20, Height - MainObject.Height / 2, 0, 0);
                            SleepEffectAnim.By = new Thickness(-100, -70, 0, 0);
                        }
                        else
                        {
                            SleepEffectAnim.From = new Thickness(MainObject.Margin.Left + MainObject.Width, Height - MainObject.Height / 2, 0, 0);
                            SleepEffectAnim.By = new Thickness(100, -70, 0, 0);
                        }
                        int delay = 230;
                        foreach (Image Z in Zs)
                        {
                            SleepEffectAnim.BeginTime = TimeSpan.FromMilliseconds(delay);
                            delay += 500;
                            Z.BeginAnimation(Image.MarginProperty, SleepEffectAnim);
                        }
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
                        MomentumAdjustmentTimer.Interval = 800;
                        break;
                }
                image.EndInit();
                ImageBehavior.SetAnimatedSource(MainObject, image);
            }
        }

        private void RandomSing(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)delegate ()
            {
                if (MainObject.Margin.Top == Height - 70 && CurrentBehaviour == 2)
                {
                    if (FacingLeft)
                    {
                        SingEffectAnim.From = new Thickness(MainObject.Margin.Left + 20, Height - 60, 0, 0);
                    }
                    else
                    {
                        SingEffectAnim.From = new Thickness(MainObject.Margin.Left + 55, Height - 60, 0, 0);
                    }
                    double left = Rand.Next(-40, 40);
                    SingEffectAnim.By = new Thickness(left, (100 - Math.Abs(left)) * -1, 0, 0);
                    /*SingEffectAnim.From = new Thickness(MainObject.Margin.Left + 55, Height - 120, 0, 0);
                    SingEffectAnim.By = new Thickness(0, 0, 0, 0);*/
                    var image = new BitmapImage();
                    image.BeginInit();
                    var controller = ImageBehavior.GetAnimationController(Notes[NextNote]);
                    switch (Rand.Next(3))
                    {
                        case 0:
                            image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/Green Note.gif", UriKind.Relative);
                            image.BaseUri = BaseUriHelper.GetBaseUri(this);
                            break;
                        case 1:
                            image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/Orange Note.gif", UriKind.Relative);
                            image.BaseUri = BaseUriHelper.GetBaseUri(this);
                            break;
                        case 2:
                            image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/Red Note.gif", UriKind.Relative);
                            image.BaseUri = BaseUriHelper.GetBaseUri(this);
                            break;
                    }
                    image.EndInit();
                    ImageBehavior.SetAnimatedSource(Notes[NextNote], image);
                    Notes[NextNote].BeginAnimation(Image.MarginProperty, SingEffectAnim);
                    NextNote++;
                    if (NextNote>=Notes.Count)
                    {
                        NextNote = 0;
                    }
                    SingTimer.Interval = Rand.Next(300,1500);
                }
            });
        }

        public void PlayNote(int note)
        {
            if (MainObject.Margin.Top == Height - 70)
            {
                if (FacingLeft)
                {
                    SingEffectAnim.From = new Thickness(MainObject.Margin.Left - 55, Height - 60, 0, 0);
                }
                else
                {
                    SingEffectAnim.From = new Thickness(MainObject.Margin.Left - 45, Height - 60, 0, 0);
                }
                double left = Rand.Next(-90, 90);
                SingEffectAnim.By = new Thickness(left, (170 - Math.Abs(left)) * -1, 0, 0);

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
            CurrentBehaviour = Rand.Next(4);
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
