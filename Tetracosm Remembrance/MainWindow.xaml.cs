using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
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
        static int PrevTriggerAnim = 0;
        static bool FacingLeft = false;
        static bool LockedFacingDirection = false;

        static bool FinishedSong = true;
        static int SongProgress = 0;
        string[] SongData = File.ReadAllText("Songs/default.txt").Split(' ');
        static List<VorbisWaveReader> NoteReaders = new List<VorbisWaveReader>();
        static List<WaveOutEvent> NotePlayers = new List<WaveOutEvent>();
        static int NextPlayer = 0;
        static int NextReader = 0;

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

            for (int i = 0; i < 4; i++)
            {
                NotePlayers.Add(new WaveOutEvent());
            }
            for (int i = 0; i < 4; i++)
            {
                VorbisWaveReader vorbis = new VorbisWaveReader("../../Sound_Effects/aria" + 0 + ".ogg");
                NoteReaders.Add(vorbis);
            }
            for (int i = 0; i < 8; i++)
            {
                Image Note = new Image()
                {
                    Width = 20,
                    Height = 20,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(-20, -20, 0, 0)
                };
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/Green Note.gif", UriKind.Relative);
                image.EndInit();
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
                            ChangeAnim(0);
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
                            ChangeAnim(0);
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
                            ChangeAnim(0);
                        }
                        break;
                    default:
                        MoveAnim.By = new Thickness(Momentum[0], Momentum[1], 0, 0);
                        break;
                }
                MainObject.BeginAnimation(Image.MarginProperty, MoveAnim);

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
                if (Switcher > Rand.Next(7, 36) && FinishedSong)
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
            if (gif != PrevTriggerAnim || gif >= 3)
            {
                PrevTriggerAnim = gif;
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
                        LockedFacingDirection = false;
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
                        LockedFacingDirection = true;
                        break;
                    case 4:
                        image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/AriaTailMove.gif", UriKind.Relative);
                        MomentumAdjustmentTimer.Interval = 600;
                        LockedFacingDirection = true;
                        break;
                    default:
                        image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/AriaIdle.gif", UriKind.Relative);
                        MomentumAdjustmentTimer.Interval = 500;
                        LockedFacingDirection = false;
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
                    if (SongProgress >= SongData.Length)
                    {
                        FinishedSong = true;
                        SongProgress = 0;
                        if (Rand.Next(5) > -1)
                        {
                            SelectSong();
                        }
                    }
                    string[] noteData = SongData[SongProgress].Split(',');
                    PlayNote(int.Parse(noteData[0]), int.Parse(noteData[1]));
                    SongProgress++;
                }
            });
        }

        public void SelectSong()
        {
            if (MainObject.Margin.Top == Height - 70 && CurrentBehaviour == 2)
            {
                var files = Directory.GetFiles("Songs/", "*.txt");
                SongData = File.ReadAllText(files[Rand.Next(files.Length)]).Split(' ');
                FinishedSong = false;
            }
        }
        public void PlayNote(int note, int delayBeforeNextNote)
        {
            if (note < 0)
            {
                note = 0;
            }
            if (note > 5)
            {
                note = 5;
            }
            NoteReaders[NextReader].Dispose();
            NotePlayers[NextPlayer].Stop();
            NotePlayers[NextPlayer].Dispose();
            if (FacingLeft)
            {
                SingEffectAnim.From = new Thickness(MainObject.Margin.Left + 20, Height - 60, 0, 0);
            }
            else
            {
                SingEffectAnim.From = new Thickness(MainObject.Margin.Left + 55, Height - 60, 0, 0);
            }
            double left = Rand.Next(-60, 60);
            SingEffectAnim.By = new Thickness(left, (110 - Math.Abs(left)) * -1, 0, 0);
            var image = new BitmapImage();
            image.BeginInit();
            var controller = ImageBehavior.GetAnimationController(Notes[NextNote]);
            switch (note)
            {
                case 0:
                    image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/Green Note.gif", UriKind.Relative);
                    image.BaseUri = BaseUriHelper.GetBaseUri(this);
                    break;
                case 1:
                    image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/Green Note.gif", UriKind.Relative);
                    image.BaseUri = BaseUriHelper.GetBaseUri(this);
                    break;
                case 2:
                    image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/Orange Note.gif", UriKind.Relative);
                    image.BaseUri = BaseUriHelper.GetBaseUri(this);
                    break;
                case 3:
                    image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/Orange Note.gif", UriKind.Relative);
                    image.BaseUri = BaseUriHelper.GetBaseUri(this);
                    break;
                case 4:
                    image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/Red Note.gif", UriKind.Relative);
                    image.BaseUri = BaseUriHelper.GetBaseUri(this);
                    break;
                case 5:
                    image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/Red Note.gif", UriKind.Relative);
                    image.BaseUri = BaseUriHelper.GetBaseUri(this);
                    break;
                default:
                    image.UriSource = new Uri("/Tetracosm Remembrance;component/Gifs_Images/Green Note.gif", UriKind.Relative);
                    image.BaseUri = BaseUriHelper.GetBaseUri(this);
                    break;
            }
            image.EndInit();
            ImageBehavior.SetAnimatedSource(Notes[NextNote], image);

            NoteReaders[NextReader] = new VorbisWaveReader("../../Sound_Effects/aria" + note + ".ogg");
            NotePlayers[NextPlayer] = new WaveOutEvent();
            NotePlayers[NextPlayer].Init(NoteReaders[NextReader]);
            NotePlayers[NextPlayer].Play();

            Notes[NextNote].BeginAnimation(Image.MarginProperty, SingEffectAnim);
            NextReader++;
            if (NextReader >= NoteReaders.Count)
            {
                NextReader = 0;
            }
            NextPlayer++;
            if (NextPlayer>= NotePlayers.Count)
            {
                NextPlayer = 0;
            }
            NextNote++;
            if (NextNote >= Notes.Count)
            {
                NextNote = 0;
            }
            SingTimer.Interval = delayBeforeNextNote;
            //SingTimer.Interval = Rand.Next(delayBeforeNextNote-50, delayBeforeNextNote+100);
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
