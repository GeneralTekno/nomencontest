using System.Runtime.Serialization;

namespace Nomencontest.Base.Models
{

    [DataContract(Name= "RoundPhase")]
    public enum RoundPhase
    {
        [EnumMember] //Game isn't started
        GameNotStarted,
        [EnumMember] //Setup for the round
        NotStarted,
        [EnumMember] //Display categories for the players
        ShowCategories,
        [EnumMember] //Category is picked, get ready to start timer
        IsReady,
        [EnumMember] //Timer is rumming, guessing happens here
        IsRunning,
        [EnumMember]//Round is over.
        IsOver,
        [EnumMember]//Game done
        GameDone
    }

    [DataContract(Name = "SoundCue")]
    public enum SoundCue
    {
        [EnumMember]
        StartTheme,
        [EnumMember]
        StopTheme,
        [EnumMember]
        RoundBGM,
        [EnumMember]
        RightGuess,
        [EnumMember]
        WrongGuess,
        [EnumMember]
        EndRound,
        [EnumMember]
        PickCategory,
        [EnumMember]
        Intro,
        [EnumMember]
        Outro
    }


}
