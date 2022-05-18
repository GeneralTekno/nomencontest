using System.Runtime.Serialization;

namespace Forturna.Base.Models
{
    [DataContract(Name="SpriteMode")]
    public enum SpriteMode
    {
        [EnumMember]
        Idle,
        [EnumMember]
        WalkToTile,
        [EnumMember]
        Walk,
        [EnumMember]
        Victory,
        [EnumMember]
        Reset
    }


    [DataContract(Name="TurnPhase")]
    public enum TurnPhase
    {
        [EnumMember]
        NotStarted,
        [EnumMember]
        WheelSpin,
        [EnumMember]
        GuessLetter,
        [EnumMember]
        RevealingLetters,
        [EnumMember]
        AnswerGuessed,
        [EnumMember]
        GameDone
    }

}
