using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace Nomencontest.Clients.UI
{

    public class SoundPlayer
    {
        public static string GUESS_BGM
        {
            get { return AppDomain.CurrentDomain.BaseDirectory + "/Sounds/guessloop.wav"; }
        }
        public static string GUESS_GOOD
        {
            get { return AppDomain.CurrentDomain.BaseDirectory + "/Sounds/rightguess.wav"; }
        }
        public static string GUESS_BAD
        {
            get { return AppDomain.CurrentDomain.BaseDirectory + "/Sounds/badguess.wav"; }
        }
        public static string END_ROUND
        {
            get { return AppDomain.CurrentDomain.BaseDirectory + "/Sounds/endround.wav"; }
        }
        public static string PICK_CATEGORY
        {
            get { return AppDomain.CurrentDomain.BaseDirectory + "/Sounds/categorypick.wav"; }
        }
        public static string THEMESONG
        {
            get { return AppDomain.CurrentDomain.BaseDirectory + "/Sounds/themesong.wav"; }
        }
        public static string THEMSONGOUTRO
        {
            get { return AppDomain.CurrentDomain.BaseDirectory + "/Sounds/audiocueouttro.wav"; }
        }
        public static string THEMESONGINTRO
        {
            get { return AppDomain.CurrentDomain.BaseDirectory + "/Sounds/audiocueintro.wav"; }
        }

        public static void PlayAsyncSound(string path, int delay = 0)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, args) =>
            {
                try
                {
                    var sound = path;
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(sound);
                    player.Play();
                    if (delay > 0) Thread.Sleep(delay);
                }
                catch
                {
                }
            };
            worker.RunWorkerAsync();

        }
        public static void PlaySound(string path)
        {
            try
            {
                var sound = path;
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(sound);
                player.Play();
            }
            catch
            {
            }
        }


        private static MediaPlayer _musicPlayer;

        private static bool _playMusic;

        public static bool PlayMusic
        {
            get { return _playMusic; }
            set
            {
                if (_musicPlayer == null)
                {
                    try
                    {
                        _musicPlayer = new MediaPlayer();
                    }
                    catch
                    {
                    }
                }

                if (_playMusic != value)
                {
                    _playMusic = value;
                    if (_musicPlayer != null)
                    {
                        if (value)
                        {
                            var sound = THEMESONG;
                            _musicPlayer.Open(new Uri(sound));
                            _musicPlayer.Play();
                            _musicPlayer.Volume = 1;
                        }
                        else
                        {
                            var dispatcher = Dispatcher.CurrentDispatcher;
                            BackgroundWorker worker = new BackgroundWorker();
                            worker.DoWork += (sender, args) =>
                            {
                                dispatcher.Invoke((Action)(delegate ()
                                {
                                    _musicPlayer.Volume = 1;
                                    for (double i = 1; i > 0; i -= 0.1)
                                    {
                                        _musicPlayer.Volume = i;

                                        System.Threading.Thread.Sleep(100);
                                    }
                                    _musicPlayer.Volume = 0;
                                    _musicPlayer.Stop();
                                    _musicPlayer.Close();
                                }))
                                        ;
                            };
                            worker.RunWorkerCompleted += (sender, args) =>
                            {

                            };
                            worker.RunWorkerAsync();
                        }
                    }
                }
            }
        }

        private static bool _playGuessBGM = false;
        public static bool PlayGuessBGM
        {
            get { return _playGuessBGM; }
            set
            {
                if (_musicPlayer == null)
                {
                    try
                    {
                        _musicPlayer = new MediaPlayer();
                    }
                    catch
                    {
                    }
                }

                if (_playGuessBGM != value)
                {
                    _playGuessBGM = value;
                    if (_musicPlayer != null)
                    {
                        if (value)
                        {
                            var sound = GUESS_BGM;
                            _musicPlayer.Stop();
                            _musicPlayer.Open(new Uri(sound));
                            _musicPlayer.Play();
                            _musicPlayer.Volume = 1;
                        }
                        else
                        {
                            _musicPlayer.Stop();
                            _musicPlayer.Close();
                        }
                    }
                }
            }
        }
    }
}
