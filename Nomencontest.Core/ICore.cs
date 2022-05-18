using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Nomencontest.Base;
using Nomencontest.Base.Models;

namespace Nomencontest.Core
{
    [ServiceContract(CallbackContract = typeof(ICoreCallback))]
    public interface ICore
    {
        #region User Registration - multiple levels

        //Registering returns an ID for that player/client that's created internally.

        //Admins can do whatever. Admin GUID required for authentication.
        [OperationContract]
        uint RegisterAdministrator(string adminGuid);
        [OperationContract]
        void UnregisterAdministrator(DataTransporter clientData);

        //Players can pick stuff if it's their turn.
        [OperationContract]
        uint RegisterPlayer(DataTransporter clientData);
        [OperationContract]
        void UnregisterPlayer(DataTransporter clientData);

        //If set, sends GUI-specific stuff like when things should be flipped based on what's being rendered onscreen.
        [OperationContract]
        uint RegisterMainGUI(DataTransporter clientData, string adminGuid);
        [OperationContract]
        void UnregisterMainGUI(DataTransporter clientData);

        //Observer just receives data; can't actualy alter anything
        [OperationContract]
        uint RegisterObserver(DataTransporter clientData);
        [OperationContract]
        void UnregisterObserver(DataTransporter clientData);

        [OperationContract(IsOneWay = true)]
        void GetGameData(DataTransporter clientData);
        #endregion

        #region Admin Functions

        [OperationContract(IsOneWay = true)]
        void SetSettings(SetupTransporter settings);

        [OperationContract(IsOneWay = true)]
        void AddItemDatabase(DataTransporter data, byte[] itemFile);
        [OperationContract(IsOneWay = true)]
        void ClearItemDatabase(DataTransporter data);
        [OperationContract(IsOneWay = true)]
        void RemoveItem(DataTransporter data, uint itemID);

        [OperationContract(IsOneWay = true)]
        void GetItems(DataTransporter data);
        
        [OperationContract(IsOneWay = true)]
        void ShuffleCategoryItems(DataTransporter data, uint categoryID);
        [OperationContract(IsOneWay = true)]
        void EnableShuffle(DataTransporter data, bool value);


        [OperationContract(IsOneWay = true)]
        void SetCurrentPlayer(DataTransporter clientData, uint playerID);


        //Reset current round if it's currently running - override
        [OperationContract(IsOneWay = true)]
        void ResetRound(DataTransporter clientData);
        //display category options for the players
        [OperationContract(IsOneWay = true)]
        void ShowRoundCategories(DataTransporter clientData, uint[] categoryIDs);
        //select the category we're using for this round
        [OperationContract(IsOneWay = true)]
        void SelectRoundCategory(DataTransporter clientData, uint categoryID);
        //Hit start - start the counter
        [OperationContract(IsOneWay = true)]
        void StartRound(DataTransporter clientData);
        //Person guesses - yes or no. Either way it continues but a "no" is "pass", "yes" adds points
        //As a note for building the UI - score is displayed AT END of round
        [OperationContract(IsOneWay = true)]
        void GuessItem(DataTransporter clientData, bool isCorrect, bool playSound);
        [OperationContract(IsOneWay = true)]
        void OverridePlayerScores(DataTransporter clientData, int player1Score, int player2Score);

        //Flip a character on the clue so that it's easier to guess
        [OperationContract(IsOneWay = true)]
        void FlipRandomCharacter();

        //Ends the round manually.
        [OperationContract(IsOneWay = true)]
        void EndRound(DataTransporter clientData);
        //Go to the next round. If all players have gone, increment round. If final round, set that mode.
        [OperationContract(IsOneWay = true)]
        void NextRound(DataTransporter clientData);

        //Logic for the final round is gonna be a bit different.
        /*
         Final round plays as 2 teams head-to-head
         They play until 3000 - if they guess right they get it, otherwise other team gets it

        So the behavior of the round guess calls changes.

            In order to call GuessItem we have to call "FinalRoundPlayerGuesses" first.
            Idea being that down the road we have a race condition on this call - it can only be called once.
            GuessItem does nothing in final round UNTIL player is selected

            ShowRoundCategories does NOTHING in final round that said.
             */
        [OperationContract(IsOneWay = true)]
        void FinalRoundToggleNameVisibility(DataTransporter clientData, bool showName);

        [OperationContract(IsOneWay = true)]
        void FinalRoundPlayerGuesses(DataTransporter clientData, uint playerID);

        //This version is for doing from a player client - ID is included in DataTransporter
        [OperationContract(IsOneWay = true)]
        void FinalRoundPlayerGuessesDirect(DataTransporter clientData);


        [OperationContract(IsOneWay = true)]
        void StartGame(DataTransporter clientData);
        [OperationContract(IsOneWay = true)]
        void EndGame(DataTransporter clientData);


        [OperationContract]
        bool Ping(DataTransporter clientData);

        [OperationContract(IsOneWay = true)]
        void PlayMusic(DataTransporter clientData, bool playMusic);

        #endregion

        #region General Accessor functions

        [OperationContract]
        void GetSettings(DataTransporter clientData);

        #endregion


    }




    [ServiceContract]
    public interface ICoreCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnStatusUpdate(GameStatus gameStatus);

        [OperationContract(IsOneWay = true)]
        void OnSettingsUpdate(SetupTransporter gameStatus);

        [OperationContract(IsOneWay = true)]
        void OnItemsUpdate(ItemDatabaseTransporter gameStatus);

        [OperationContract(IsOneWay = true)]
        void OnGameSetup(SetupTransporter gameStatus, GameStatus gameData);

        [OperationContract(IsOneWay = true)]
        void OnClockUpdate(ClockTransporter clockValue);

        [OperationContract(IsOneWay = true)]
        void OnPlayMusic(bool playMusic);

        [OperationContract(IsOneWay = true)]
        void OnAudioCue(SoundCue soundcue);
    }

    [DataContract]
    public class ClientInfo
    {
        [DataMember]
        public uint UserID { get; set; }
        [DataMember]
        public PermissionLevel PermissionLevel { get; set; }
        [DataMember]
        public ICoreCallback Callback { get; set; }

        [DataMember]
        public uint PlayerID { get { return _playerID; } set { _playerID = value; } }

        private uint _playerID = 0;

        public ClientInfo(uint id, PermissionLevel type, ICoreCallback callback)
        {
            UserID = id;
            PermissionLevel = type;
            Callback = callback;
        }
    }
}
