using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nomencontest.Base;
using Nomencontest.Base.Models;

namespace Nomencontest.Core.Wrapper
{
    public class ItemArgs : EventArgs
    {
        public ItemArgs(ItemDatabaseTransporter transporter)
        {
            _items = transporter;
        }

        private readonly ItemDatabaseTransporter _items;
        public ItemDatabaseTransporter Items { get { return _items; } }
    }

    public class StatusArgs : EventArgs
    {
        public StatusArgs(GameStatus status)
        {
            _gameStatus = status;
        }

        private readonly GameStatus _gameStatus;
        public GameStatus GameStatus { get { return _gameStatus; } }
    }

    public class ClockArgs : EventArgs
    {
        public ClockArgs(ClockTransporter transporter)
        {
            _clockTransporter = transporter;
        }

        private readonly ClockTransporter _clockTransporter;

        public ClockTransporter ClockTransporter { get { return _clockTransporter; } }
    }

    public class SettingsArgs : EventArgs
    {
        public SettingsArgs(SetupTransporter transporter)
        {
            _setupTransporter = transporter;
        }

        private readonly SetupTransporter _setupTransporter;

        public SetupTransporter SetupTransporter { get { return _setupTransporter; } } }
    }


    public class AudioArgs : EventArgs
    {
        public AudioArgs(SoundCue transporter)
        {
            _audioCue = transporter;
        }

        private readonly SoundCue _audioCue;

        public SoundCue SoundCue { get { return _audioCue; } }
    }

    public class GameSetupArgs : EventArgs
    {
        public GameSetupArgs(SetupTransporter transporter, GameStatus gameStatus)
        {
            _setupTransporter = transporter;
            _gameStatus = gameStatus;
        }

        private readonly SetupTransporter _setupTransporter;
        private readonly GameStatus _gameStatus;

        public SetupTransporter SetupTransporter { get { return _setupTransporter; } }
        public GameStatus GameStatus { get { return _gameStatus; } }
    }
    public class BoolArgs : EventArgs
    {
        public BoolArgs(bool arg)
        {
            _value = arg;
        }

        private readonly bool _value;

        public bool Value { get { return _value; } }
    }
    public class StringArgs : EventArgs
    {
        public StringArgs(string arg)
        {
            _value = arg;
        }

        private readonly string _value;

        public string Value { get { return _value; }
    }
}
