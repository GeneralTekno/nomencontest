using System;
using System.Runtime.Serialization;
using Nomencontest.Base.Models;
using Nomencontest.Base.ViewModels;

namespace Nomencontest.Base
{
    [DataContract]
    public class SetupTransporter : DataTransporter
    {
        [DataMember]
        public Settings Settings { get; set; }
    }

    [DataContract]
    public class ItemDatabaseTransporter : DataTransporter
    {
        [DataMember]
        public ItemDatabase ItemDatabase { get; set; }
    }

    [DataContract]
    public class GameStatus : DataTransporter
    {
        [DataMember]
        public int CurrentRound { get; set; }
        [DataMember]
        public bool IsFinalRoundPlayerGuessing { get; set; }
        [DataMember]
        public bool IsFinalRound { get; set; }
        [DataMember]
        public RoundPhase RoundPhase { get; set; }
        [DataMember]
        public PlayerVM[] Players { get; set; }
        [DataMember]
        public int CurrentPlayerIndex { get; set; }
        [DataMember]
        public StringWithIDTransporter CurrentCategory { get; set; }
        [DataMember]
        public StringWithIDTransporter[] SelectableCategories { get; set; }
        [DataMember]
        public string CurrentDisplayName { get; set; }
        [DataMember]
        public string CurrentName { get; set; }
        [DataMember]
        public int CurrentItemScoreValue { get; set; }

        public bool ShowCurrentName
        {
            get { return CurrentDisplayName == CurrentName; }
        }
    }

    [DataContract]
    public class StringWithIDTransporter
    {
        [DataMember]
        public uint ID { get; set; }
        [DataMember] public string Value { get; set; }
    }


    [DataContract]
    [KnownType(typeof(DataTransporter))]
    public class DataTransporter
    {
        [DataMember]
        public uint ID { get; set; }
        [DataMember]
        public long Timestamp { get; set; }

        public DateTime GetTimeStamp() { return new DateTime(Timestamp); }

        public DataTransporter()
        {
            Timestamp = DateTime.UtcNow.Ticks;
        }
    }

    public class ClockTransporter
    {
        [DataMember]
        public int CurrentTime { get; set; }
        [DataMember]
        public int StartTime { get; set; }
    }

    [DataContract(Name="PermissionLevel")]
    public enum PermissionLevel
    {
        [EnumMember]
        PERMISSIONS_OBSERVER = 0x00,
        [EnumMember]
        PERMISSIONS_GUI = 0x1,
        [EnumMember]
        PERMISSIONS_PLAYER = 0x2,
        [EnumMember]
        PERMISSIONS_ADMIN = 0x3
    }
}
