using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using IniParser;
using IniParser.Model;
using Nomencontest.Base.ViewModels;

namespace Nomencontest.Base
{
    
    public class Settings : PropertyClass
    {
        private static Settings _instance;
        private string _path;
        private IniData _data;

        private const string MAIN_SETTINGS = "GENERAL";
        private const string REMOTE_SETTINGS = "REMOTE";

        public int MaxPlayers
        {
            get
            {
                return _data.Sections.ContainsSection(MAIN_SETTINGS) && _data[MAIN_SETTINGS].ContainsKey("MaxPlayers")
                    ? Int32.Parse(_data[MAIN_SETTINGS]["MaxPlayers"])
                    : 2;
            }
            set
            {
                if (EnsureKeyExists(MAIN_SETTINGS, "MaxPlayers"))
                {
                    _data[MAIN_SETTINGS]["MaxPlayers"] = value.ToString();
                    RaisePropertyChanged("MaxPlayers");
                }
            }
        }

        public int RoundLength
        {
            get
            {
                return _data.Sections.ContainsSection(MAIN_SETTINGS) && _data[MAIN_SETTINGS].ContainsKey("RoundLength")
                    ? Int32.Parse(_data[MAIN_SETTINGS]["RoundLength"])
                    : 60;
            }
            set
            {
                if (EnsureKeyExists(MAIN_SETTINGS, "RoundLength"))
                {
                    _data[MAIN_SETTINGS]["RoundLength"] = value.ToString();
                    RaisePropertyChanged("RoundLength");
                }
            }
        }

        public int RoundCount
        {
            get
            {
                return _data.Sections.ContainsSection(MAIN_SETTINGS) && _data[MAIN_SETTINGS].ContainsKey("RoundCount")
                    ? Int32.Parse(_data[MAIN_SETTINGS]["RoundCount"])
                    : 2;
            }
            set
            {
                if (EnsureKeyExists(MAIN_SETTINGS, "RoundCount"))
                {
                    _data[MAIN_SETTINGS]["RoundCount"] = value.ToString();
                    RaisePropertyChanged("RoundCount");
                }
            }
        }


        public bool UseShuffledValues
        {
            get
            {
                return _data.Sections.ContainsSection(MAIN_SETTINGS) && _data[MAIN_SETTINGS].ContainsKey("UseShuffledValues")
                    ? bool.Parse(_data[MAIN_SETTINGS]["UseShuffledValues"])
                    : true;
            }
            set
            {
                if (EnsureKeyExists(MAIN_SETTINGS, "UseShuffledValues"))
                {
                    _data[MAIN_SETTINGS]["UseShuffledValues"] = value.ToString();
                    RaisePropertyChanged("UseShuffledValues");
                }
            }
        }


        public int AnswerValue
        {
            get
            {
                return _data.Sections.ContainsSection(MAIN_SETTINGS) && _data[MAIN_SETTINGS].ContainsKey("AnswerValue")
                    ? Int32.Parse(_data[MAIN_SETTINGS]["AnswerValue"])
                    : 100;
            }
            set
            {
                if (EnsureKeyExists(MAIN_SETTINGS, "AnswerValue"))
                {
                    _data[MAIN_SETTINGS]["AnswerValue"] = value.ToString();
                    RaisePropertyChanged("AnswerValue");
                }
            }
        }

        public int FaceOffGoal
        {
            get
            {
                return _data.Sections.ContainsSection(MAIN_SETTINGS) && _data[MAIN_SETTINGS].ContainsKey("FaceOffGoal")
                    ? Int32.Parse(_data[MAIN_SETTINGS]["FaceOffGoal"])
                    : 3000;
            }
            set
            {
                if (EnsureKeyExists(MAIN_SETTINGS, "FaceOffGoal"))
                {
                    _data[MAIN_SETTINGS]["FaceOffGoal"] = value.ToString();
                    RaisePropertyChanged("FaceOffGoal");
                }
            }
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return "http://" + ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
        public string ServerIPAddress
        {
            get
            {
                return _data.Sections.ContainsSection(REMOTE_SETTINGS) && _data[REMOTE_SETTINGS].ContainsKey("ServerIPAddress")
                    ? (_data[REMOTE_SETTINGS]["ServerIPAddress"])
                    : GetLocalIPAddress();
            }
            set
            {
                if (EnsureKeyExists(REMOTE_SETTINGS, "ServerIPAddress"))
                {
                    _data[REMOTE_SETTINGS]["ServerIPAddress"] = value;
                    RaisePropertyChanged("ServerIPAddress");
                }
            }
        }

        public int RemotePort
        {
            get
            {
                return _data.Sections.ContainsSection(REMOTE_SETTINGS) && _data[REMOTE_SETTINGS].ContainsKey("RemotePort")
                    ? Int32.Parse(_data[REMOTE_SETTINGS]["RemotePort"])
                    : 9001;
            }
            set
            {
                if (EnsureKeyExists(REMOTE_SETTINGS, "RemotePort"))
                {
                    _data[REMOTE_SETTINGS]["RemotePort"] = value.ToString();
                    RaisePropertyChanged("RemotePort");
                }
            }
        }

        public int GamePort
        {
            get
            {
                return _data.Sections.ContainsSection(REMOTE_SETTINGS) && _data[MAIN_SETTINGS].ContainsKey("GamePort")
                    ? Int32.Parse(_data[REMOTE_SETTINGS]["GamePort"])
                    : 9002;
            }
            set
            {
                if (EnsureKeyExists(REMOTE_SETTINGS, "GamePort"))
                {
                    _data[REMOTE_SETTINGS]["GamePort"] = value.ToString();
                    RaisePropertyChanged("GamePort");
                }
            }
        }

        public static string ServerURLPath
        {
            get { return @"/Nomencontest/"; }
        }


        public static string RemoteURLPath
        {
            get { return @"/NomencontestRemote/"; }
        }


        public static string ImageDir
        {
            get { return @"Images/"; }
        }

        public static string SettingsPath
        {
            get { return "settings.ini"; }
        }

        public static Settings GetInstance()
        {
            return _instance;
        }

        public Settings()
        {
            LoadSettings(SettingsPath);

        }
        public Settings(string path)
        {
            LoadSettings(_path);
            
        }

        public void Reload()
        {
            LoadSettings(_path);
        }

        public bool Save()
        {
            try
            {
                var parser = new FileIniDataParser();
                parser.WriteFile("Configuration.ini", _data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void LoadSettings(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    _path = path;
                    var parser = new FileIniDataParser();
                    if (_instance == null) _instance = this;
                    _data = parser.ReadFile(path);

                }
                else _data = new IniData();
                
            }
            catch (Exception e)
            {
            }
        }

        private bool EnsureKeyExists(string category, string key)
        {
            if (_data != null)
            {
                if (!_data.Sections.ContainsSection(category))
                {
                    _data.Sections.AddSection(category);
                }
                if (!_data.Sections[category].ContainsKey(key))
                {
                    _data.Sections[category].AddKey(key);
                }
                return true;
            }
            return false;
        }

    }
}
