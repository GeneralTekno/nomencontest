using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Nomencontest.Base;
using Nomencontest.Base.Models;
using Nomencontest.Base.ViewModels;
using Nomencontest.Core;
using System.Text.RegularExpressions;

namespace Nomencontest.Core
{
    public class Core : ICore
    {
        private static NomencontestGame _gameInstance;

        private Settings _settings;
        public Settings Settings { get { return _gameInstance.Settings; } }

        private static object locker = new object();

        #region Player registration

        private static Dictionary<uint, ClientInfo> _clients;

        private const string ADMIN_KEY = "2edffbfc-0308-4648-adbc-32d088ecbf8d";
        private const string GUI_KEY = "651432fc-24ff-480e-b671-b9be6bd8a383";
        Random guidGenerator = new Random((int)DateTime.Now.Ticks);

        public Core()
        {
            if (_gameInstance == null) _gameInstance = new NomencontestGame();

            if (_clients == null) _clients = new Dictionary<uint, ClientInfo>();
        }

        private uint GenerateGUID()
        {
            uint guid;
            do
            {
                guid = (uint)guidGenerator.Next();
            }
            while (_clients != null && _clients.ContainsKey(guid));

            return guid;
        }

        private void RemoveExistingRegistration(uint guid)
        {
            if (_clients == null) _clients = new Dictionary<uint, ClientInfo>();
            if (_clients.ContainsKey(guid)) _clients.Remove(guid);
        }
        private bool ValidateClient(DataTransporter client, PermissionLevel level)
        {
            if (_clients != null && _clients.ContainsKey(client.ID))
            {

                if (((ICommunicationObject)(_clients[client.ID].Callback)).State != CommunicationState.Opened)
                {
                    var callback = OperationContext.Current.GetCallbackChannel<ICoreCallback>();
                    if (callback != _clients[client.ID].Callback)
                    {
                        _clients[client.ID].Callback = callback;
                    }
                }

                return _clients[client.ID].PermissionLevel >= level;
            }
            return false;
        }

        public uint RegisterAdministrator(string adminGuid)
        {
            if (adminGuid.Equals(ADMIN_KEY))
            {
                var callback = OperationContext.Current.GetCallbackChannel<ICoreCallback>();
                lock (locker)
                {
                    var guid = GenerateGUID();
                    RemoveExistingRegistration(guid);
                    _clients.Add(guid, new ClientInfo(guid, PermissionLevel.PERMISSIONS_ADMIN, callback));
                    return guid;
                }
            }
            return 0;
        }

        public void UnregisterAdministrator(DataTransporter client)
        {
            if (ValidateClient(client, PermissionLevel.PERMISSIONS_ADMIN))
            {
                RemoveExistingRegistration(client.ID);
            }
        }

        public uint RegisterPlayer(DataTransporter client)
        {
            var callback = OperationContext.Current.GetCallbackChannel<ICoreCallback>();
            lock (locker)
            {
                //Can only register players if game not yet started
                if (_gameInstance.RoundPhase == RoundPhase.GameDone ||
                    (_gameInstance.RoundPhase == RoundPhase.NotStarted && _gameInstance.CurrentRound == 0))
                {
                    var guid = GenerateGUID();
                    RemoveExistingRegistration(guid);

                    var registeredPlayerCount = _clients.Count(x => x.Value.PermissionLevel == PermissionLevel.PERMISSIONS_PLAYER);
                    if (registeredPlayerCount < Settings.MaxPlayers)
                    {
                        var clientInfo = new ClientInfo(guid, PermissionLevel.PERMISSIONS_PLAYER, callback);
                        _clients.Add(guid, clientInfo);
                    }
                    return guid;
                }
            }
            return 0;
        }

        public void UnregisterPlayer(DataTransporter client)
        {
            lock (locker)
            {
                if (ValidateClient(client, PermissionLevel.PERMISSIONS_PLAYER))
                {
                    RemoveExistingRegistration(client.ID);
                }
            }
        }

        public uint RegisterMainGUI(DataTransporter client, string guiGuid)
        {
            if (guiGuid.Equals(GUI_KEY))
            {
                var callback = OperationContext.Current.GetCallbackChannel<ICoreCallback>();
                lock (locker)
                {
                    var guid = GenerateGUID();
                    RemoveExistingRegistration(guid);
                    _clients.Add(guid, new ClientInfo(guid, PermissionLevel.PERMISSIONS_GUI, callback));
                    return guid;
                }
            }
            return 0;
        }

        public void UnregisterMainGUI(DataTransporter client)
        {
            lock (locker)
            {
                RemoveExistingRegistration(client.ID);
            }
        }

        public uint RegisterObserver(DataTransporter client)
        {
            var callback = OperationContext.Current.GetCallbackChannel<ICoreCallback>();
            lock (locker)
            {
                var guid = GenerateGUID();
                RemoveExistingRegistration(guid);
                _clients.Add(guid, new ClientInfo(guid, PermissionLevel.PERMISSIONS_OBSERVER, callback));
                return guid;
            }
            return 0;
        }

        public void UnregisterObserver(DataTransporter client)
        {
            lock (locker)
            {
                RemoveExistingRegistration(client.ID);
            }
        }

        #endregion

        #region Admin Functions

        public void GetGameData(DataTransporter client)
        {
            lock (locker)
            {
                var callback = OperationContext.Current.GetCallbackChannel<ICoreCallback>();

                if (ValidateClient(client, PermissionLevel.PERMISSIONS_OBSERVER))
                {
                    var status = GetGameStatusTransport();
                    callback.OnStatusUpdate(status);
                    if (ValidateClient(client, PermissionLevel.PERMISSIONS_ADMIN))
                    {
                        var items = GetItemsTransport();
                        callback.OnItemsUpdate(items);
                        var settings = GetSettingsTransport();
                        callback.OnSettingsUpdate(settings);
                    }

                }
            }
        }

        public void SetSettings(SetupTransporter settings)
        {
            lock (locker)
            {
                if (ValidateClient(settings, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    _gameInstance.Settings.AnswerValue = settings.Settings.AnswerValue;
                    _gameInstance.Settings.FaceOffGoal = settings.Settings.FaceOffGoal;
                    _gameInstance.Settings.MaxPlayers = settings.Settings.MaxPlayers;
                    _gameInstance.Settings.RoundCount = settings.Settings.RoundCount;
                    _gameInstance.Settings.RoundLength = settings.Settings.RoundLength;
                    _gameInstance.Settings.UseShuffledValues = settings.Settings.UseShuffledValues;
                    _gameInstance.Settings.Save();
                }
            }
        }

        public void AddItemDatabase(DataTransporter client, byte[] itemFile)
        {
            lock (locker)
            {
                if (ValidateClient(client, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    if (itemFile != null && _gameInstance.ItemDatabase.OpenItemFile(itemFile))
                    {
                        _gameInstance.CurrentRound = 0;
                        foreach (var player in _gameInstance.Players)
                        {
                            player.ResetPoints();
                        }
                        _gameInstance.RoundPhase = RoundPhase.GameNotStarted;

                    }

                    UpdateItems();
                    UpdateClientStatus();
                }
                return;
            }
        }

        public void ClearItemDatabase(DataTransporter client)
        {
            lock (locker)
            {
                if (ValidateClient(client, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    _gameInstance.ItemDatabase.ClearItemFile();

                    UpdateClientStatus();
                    UpdateItems();
                }
            }
        }

        public void RemoveItem(DataTransporter client, uint itemID)
        {
            lock (locker)
            {
                if (ValidateClient(client, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    if (_gameInstance.ItemDatabase.HasItems)
                        _gameInstance.ItemDatabase.DeleteItem(itemID);

                    UpdateClientStatus();
                    UpdateItems();
                }
            }
        }

        public void GetItems(DataTransporter client)
        {
            lock (locker)
            {
                if (ValidateClient(client, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    var transporter = GetItemsTransport();
                    var callback = OperationContext.Current.GetCallbackChannel<ICoreCallback>();
                    callback.OnItemsUpdate(transporter);
                }
            }
        }

        public void ShuffleCategoryItems(DataTransporter data, uint itemID)
        {
            lock (locker)
            {
                if (ValidateClient(data, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    _gameInstance.CurrentCategory.ShuffleItems();
                    UpdateClientStatus();
                }
            }
        }

        public void EnableShuffle(DataTransporter data, bool value)
        {
            lock (locker)
            {
                if (ValidateClient(data, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    _gameInstance.Settings.UseShuffledValues = value;
                    UpdateClientStatus();
                }
            }
        }

        public void SetCurrentPlayer(DataTransporter client, uint playerID)
        {
            lock (locker)
            {
                if (ValidateClient(client, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    for (int i=0;i<_gameInstance.Players.Count;i++)
                    {
                        if (_gameInstance.Players[i].Model.ID == playerID)
                        {
                            _gameInstance.CurrentPlayerIndex = i;
                            UpdateClientStatus();
                            break;
                        }
                    }
                }
            }
        }

        public void OverridePlayerScores(DataTransporter client, int player1Score, int player2Score)
        {
            lock (locker)
            {
                if (ValidateClient(client, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    if (_gameInstance.Players.Count == 2)
                    { 
                        _gameInstance.Players[0].Points = player1Score;
                        _gameInstance.Players[1].Points = player2Score;
                    }
                    UpdateClientStatus();
                }
            }
        }

        public void ResetRound(DataTransporter client)
        {
            lock (locker)
            {
                if (ValidateClient(client, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    _gameInstance.IsFinalRoundPlayerGuessing = false;
                    if (_gameInstance.RoundPhase < RoundPhase.IsOver)
                    {
                        foreach (var player in _gameInstance.Players)
                        {
                            player.ResetPoints();
                            player.HasPlayedRound = false;
                        }
                        _gameInstance.CurrentRound--;
                        _gameInstance.RoundPhase = RoundPhase.NotStarted;

                        UpdateClientStatus();
                    }
                } 
            }
        }

        public void ShowRoundCategories(DataTransporter client, uint[] categoryIDs)
        {
            lock (locker)
            {
                if (ValidateClient(client, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    TriggerAudio(SoundCue.StopTheme);
                    if (!_gameInstance.IsFinalRound && _gameInstance.RoundPhase == RoundPhase.NotStarted && categoryIDs.Length > 1)
                    {
                        //Regular round - display the categories

                            _gameInstance.NextItems.Clear();
                        foreach (var id in categoryIDs)
                        {
                            var item = _gameInstance.ItemDatabase.GetItem(id);
                            if (item != null) _gameInstance.NextItems.Add(new CategoryVM(item));
                        }

                        _gameInstance.RoundPhase = RoundPhase.ShowCategories;

                    }
                    UpdateClientStatus();
                }
            }
        }

        public void SelectRoundCategory(DataTransporter client, uint categoryID)
        {
            lock (locker)
            {
                if (ValidateClient(client, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    if (!_gameInstance.IsFinalRound && _gameInstance.RoundPhase == RoundPhase.ShowCategories)
                    {
                        //Regular round - display the categories
                        _gameInstance.RoundPhase = RoundPhase.IsReady;
                        _gameInstance.CurrentCategory = new CategoryVM(_gameInstance.ItemDatabase.GetItem(categoryID));
                    }
                    else if (_gameInstance.IsFinalRound && _gameInstance.RoundPhase == RoundPhase.NotStarted)
                    {
                        //Final round - display the category
                        _gameInstance.RoundPhase = RoundPhase.IsReady;
                        _gameInstance.NextItems.Clear();
                        _gameInstance.CurrentCategory = new CategoryVM(_gameInstance.ItemDatabase.GetItem(categoryID));
                    }
                    TriggerAudio(SoundCue.PickCategory);
                    UpdateClientStatus();
                }
            }
        }

        public void StartGame(DataTransporter clientData)
        {
            lock (locker)
            {
                if (ValidateClient(clientData, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    if (_gameInstance.RoundPhase == RoundPhase.GameNotStarted ||
                        _gameInstance.RoundPhase == RoundPhase.GameDone)
                    {
                        if (_gameInstance.ItemDatabase.UsedItems == _gameInstance.ItemDatabase.Count)
                        {
                            _gameInstance.ItemDatabase.ResetUsedItems();
                        }
                        _gameInstance.CurrentRound = 0;
                        _gameInstance.RoundPhase = RoundPhase.NotStarted;

                        if (_gameInstance.NextItems.Count == 0)
                        {
                            var items = _gameInstance.ItemDatabase.GetNewItems(2, false);
                            foreach (var a in items)
                            {
                                _gameInstance.NextItems.Add(new CategoryVM(a));
                            }
                        }

                        _gameInstance.Players.Clear();

                        int playerNumber = 1;

                        var registeredPlayerClients =
                            _clients.Where(x => x.Value.PermissionLevel == PermissionLevel.PERMISSIONS_PLAYER).ToArray();

                        foreach (var c in registeredPlayerClients)
                        {
                            c.Value.PlayerID = 0;
                        }
                        
                        // If we have player clients registered, hook those up.
                        for (int i = 0; i < Settings.MaxPlayers; i++)
                        {
                            var newPlayer = new PlayerVM(String.Format("Player {0}", i + 1));
                            _gameInstance.Players.Add(newPlayer);
                            newPlayer.ResetPoints();
                            if (i < registeredPlayerClients.Length)
                            {
                                var key = registeredPlayerClients[i].Key;
                                _clients[key].PlayerID = newPlayer.Model.ID;
                            }
                        }
                        _gameInstance.CurrentPlayerIndex = 0;
                        UpdateClientStatus();
                        UpdateItems();
                        TriggerAudio(SoundCue.StartTheme);
                    }
                }
            }
        }

        public void EndGame(DataTransporter clientData)
        {
            lock (locker)
            {
                if (ValidateClient(clientData, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    if (_gameInstance.CurrentRound > 0)
                    {
                        _gameInstance.CurrentRound = Settings.RoundCount + 1;
                        _gameInstance.RoundPhase = RoundPhase.GameDone;

                        foreach (var player in _gameInstance.Players)
                        {
                            player.ResetPoints();
                        }
                    }
                    TriggerAudio(SoundCue.StartTheme);
                    UpdateClientStatus();
                }
            }
        }


        public void StartRound(DataTransporter client)
        {
            lock (locker)
            {
                if (ValidateClient(client, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    TriggerAudio(SoundCue.StopTheme);
                    if (_gameInstance.CurrentCategory != null && _gameInstance.RoundPhase == RoundPhase.IsReady)
                    {
                        _gameInstance.ResetScoreMultiplier();
                        _gameInstance.IsFinalRoundPlayerGuessing = false;
                        if (!_gameInstance.IsFinalRound)
                        {
                            //Regular round
                            _gameInstance.ResetTimer();
                            _gameInstance.OnTimerTick += OnTimerTick;
                            _gameInstance.OnRoundEnded += OnRoundEnded;
                            UpdateClientTimer();
                            _gameInstance.StartTimer();
                        }
                        else
                        {
                            //Final round - display the category
                            _gameInstance.NextItems.Clear();

                        }
                        _gameInstance.CurrentDisplayName = _gameInstance.CurrentCategory.CurrentItem;
                        _usedCharacters.Clear();
                        if (_gameInstance.IsFinalRound) _gameInstance.CurrentDisplayName = GetDefaultFinalRoundName(_gameInstance.CurrentDisplayName);
                        _gameInstance.RoundPhase = RoundPhase.IsRunning;
                        UpdateClientStatus();
                    }
                }
            }
        }

        private void OnTimerTick(object sender, EventArgs eventArgs)
        {
            UpdateClientTimer();
        }

        public void FinalRoundToggleNameVisibility(DataTransporter client, bool showName)
        {
            lock (locker)
            {
                if (ValidateClient(client, PermissionLevel.PERMISSIONS_ADMIN) &&
                    _gameInstance.RoundPhase == RoundPhase.IsRunning)
                {
                    if (_gameInstance.IsFinalRound)
                    {
                        _gameInstance.CurrentDisplayName = showName ? _gameInstance.CurrentCategory.CurrentItem: GetDefaultFinalRoundName(_gameInstance.CurrentCategory.CurrentItem);
                        UpdateClientStatus();
                    }
                }
            }
        }

        public
            void GuessItem(DataTransporter client, bool isCorrect, bool playSoundOnIncorrect)
        {
            lock (locker)
            {
                if (ValidateClient(client, PermissionLevel.PERMISSIONS_ADMIN) && _gameInstance.RoundPhase == RoundPhase.IsRunning)
                {
                    if (!_gameInstance.IsFinalRound)
                    {
                        //Is regular round
                        if (isCorrect)
                        {
                            _gameInstance.CurrentPlayer.Points += _gameInstance.AnswerValue * (_gameInstance.CurrentRound + 1);
                            TriggerAudio(SoundCue.RightGuess);
                        }
                        else if (playSoundOnIncorrect)
                        {
                            TriggerAudio(SoundCue.WrongGuess);
                        }
                    }
                    else if (_gameInstance.IsFinalRound && _gameInstance.IsFinalRoundPlayerGuessing)
                    {
                        _gameInstance.IsFinalRoundPlayerGuessing = false;
                        //Is final round - if we guess right, get points, wrong, other player gets the points. ONLY WORKS if fwe've enabled IsFinalRoundPlayerGuessing
                        if (isCorrect)
                        {
                            _gameInstance.CurrentPlayer.Points += _gameInstance.AnswerValue;
                            TriggerAudio(SoundCue.RightGuess);
                        }
                        else
                        {
                            for (int i = 0; i < _gameInstance.Players.Count; i++)
                            {
                                if (i != _gameInstance.CurrentPlayerIndex)
                                {
                                    _gameInstance.Players[i].Points += _gameInstance.AnswerValue;
                                    
                                   
                                }
                            }
                            TriggerAudio(SoundCue.WrongGuess);
                        }
                        if (_gameInstance.Players.Any(x => x.Points >= Settings.FaceOffGoal))
                        {
                            _gameInstance.RoundPhase = RoundPhase.IsOver;

                            TriggerAudio(SoundCue.Outro);
                        }
                        else
                        {
                            _gameInstance.UpScoreMultiplier();
                        }
                    }
                    _gameInstance.CurrentCategory.NextItem();
                    _gameInstance.CurrentDisplayName = _gameInstance.CurrentCategory.CurrentItem;
                    _usedCharacters.Clear();
                    if (_gameInstance.IsFinalRound) _gameInstance.CurrentDisplayName = GetDefaultFinalRoundName(_gameInstance.CurrentDisplayName);
                    UpdateClientStatus();
                }
            }
        }
        private string GetDefaultFinalRoundName(string text, List<int> usedChars = null)
        {
            var input = text.ToUpper();
            string output = string.Empty;
            bool skipNBSP = true;
            bool skipLongSpace = false;
            for (int i = 0; i < text.Length; i++)
            {
                if (input[i] == ' ')
                {
                    output += skipLongSpace ? " " : "    ";
                    skipNBSP = true;
                    skipLongSpace = false;
                }
                else if (usedChars != null && usedChars.Contains((i)))
                {
                    output += input[i];
                    skipNBSP = true;
                    skipLongSpace = true;
                }
                else
                {
                    if (!skipNBSP) output += System.Convert.ToChar(160);
                    output += '_';
                    skipNBSP = false;
                    skipLongSpace = false;
                }
            }

            return output;
        }

        private List<int> _usedCharacters = new List<int>();
        public void FlipRandomCharacter()
        {
            lock(locker)
            {
                Random random = new Random((int)DateTime.Now.Ticks);
                var item = _gameInstance.CurrentCategory.CurrentItem.ToUpper();
                while (_usedCharacters.Count < item.Length - item.Count(x=> x==' '))
                {
                    var index = random.Next(item.Length);
                    
                    if (!_usedCharacters.Contains(index) && item.ElementAt(index) != ' ')
                    {
                        _usedCharacters.Add(index);
                        break;
                    }
                }
                _gameInstance.CurrentDisplayName = GetDefaultFinalRoundName(_gameInstance.CurrentCategory.CurrentItem, _usedCharacters);
                UpdateClientStatus();
            }
        }

        private void OnRoundEnded(object sender, EventArgs eventArgs)
        {
            lock (locker)
            {
                TriggerAudio(SoundCue.EndRound);
                UpdateClientTimer();
                UpdateClientStatus();
            }
        }
        public void EndRound(DataTransporter client)
        {
            lock (locker)
            {
                if (ValidateClient(client, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    if (_gameInstance.CurrentCategory != null && _gameInstance.RoundPhase == RoundPhase.IsRunning)
                    {
                        _gameInstance.EndTimer();
                    }
                }
            }
        }

        public void NextRound(DataTransporter client)
        {
            lock (locker)
            {
                if (ValidateClient(client, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    if (_gameInstance.CurrentCategory != null)
                    {
                        if (_gameInstance.RoundPhase == RoundPhase.IsOver)
                        {
                            foreach (var entry in _gameInstance.ItemDatabase.ItemEntries)
                            {
                                if (entry.ID == _gameInstance.CurrentCategory.Model.ID)
                                {
                                    entry.AlreadyUsed = true;
                                    break;
                                }
                            }
                            _gameInstance.CurrentDisplayName = String.Empty;
                            if (!_gameInstance.IsFinalRound)
                            {

                                //Regular round
                                _gameInstance.CurrentPlayer.HasPlayedRound = true;
                                if (_gameInstance.Players.Any(x => x.HasPlayedRound == false))
                                {
                                    var nextPlayer = _gameInstance.Players.First(x => x.HasPlayedRound == false);
                                    if (nextPlayer != null)
                                    {
                                        //Go to next player in the round
                                        _gameInstance.CurrentPlayerIndex = _gameInstance.Players.IndexOf(nextPlayer);

                                        //Go to the next item in the group
                                        foreach (var item in _gameInstance.NextItems)
                                        {
                                            var itemInDatabase =_gameInstance.ItemDatabase.ItemEntries.FirstOrDefault(x=> x.ID == item.Model.ID);
                                            if (itemInDatabase == null || itemInDatabase.AlreadyUsed) continue;
                                            _gameInstance.CurrentCategory = item;
                                            break;
                                        }
                                        TriggerAudio(SoundCue.PickCategory);
                                        _gameInstance.RoundPhase = RoundPhase.IsReady;
                                    }
                                }
                                else
                                {
                                    _gameInstance.NextItems.Clear();
                                    //All players have gone; commit scores and get the next round ready
                                    _gameInstance.CurrentRound++;
                                    foreach (var player in _gameInstance.Players)
                                    {
                                        player.HasPlayedRound = false;
                                    }
                                    _gameInstance.CurrentPlayerIndex = _gameInstance.CurrentRound%
                                                                       _gameInstance.Players.Count;

                                    _gameInstance.RoundPhase = RoundPhase.NotStarted;

                                    if (!_gameInstance.IsFinalRound)
                                    {
                                        if (_gameInstance.NextItems.Count == 0)
                                        {
                                            var items = _gameInstance.ItemDatabase.GetNewItems(2, false);
                                            foreach (var a in items)
                                            {
                                                _gameInstance.NextItems.Add(new CategoryVM(a));
                                            }
                                        }
                                    }

                                    TriggerAudio(_gameInstance.IsFinalRound ? SoundCue.Outro : SoundCue.Intro);
                                }

                            }
                            else
                            {
                                //Final round is done - we're done now!
                                TriggerAudio(SoundCue.StartTheme);
                                _gameInstance.RoundPhase = RoundPhase.GameDone;
                            }
                           
                        }

                        UpdateClientStatus();
                        UpdateItems();
                    }
                }
            }
        }

        public void FinalRoundPlayerGuesses(DataTransporter clientData, uint playerID)
        {
            lock (locker)
            {
                if (ValidateClient(clientData, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    PlayerGuesses(playerID);
                }
            }
        }
        //Version of this where we use the id in clientData
        public void FinalRoundPlayerGuessesDirect(DataTransporter clientData)
        {
            lock (locker)
            {
                if (ValidateClient(clientData, PermissionLevel.PERMISSIONS_PLAYER))
                {
                    var client = _clients[clientData.ID];
                    if ( client != null && _gameInstance.Players.Any(x => x.Model.ID == client.PlayerID))
                    {
                        PlayerGuesses(client.PlayerID);
                    }
                }
            }
        }

        private void PlayerGuesses(uint playerID)
        {
            if (_gameInstance.IsFinalRound && _gameInstance.RoundPhase == RoundPhase.IsRunning && _gameInstance.IsFinalRoundPlayerGuessing == false)
            {
                var player = _gameInstance.Players.First(x => x.Model.ID == playerID);
                if (player != null)
                {
                    var index = _gameInstance.Players.IndexOf(player);
                    _gameInstance.CurrentPlayerIndex = index;
                    _gameInstance.IsFinalRoundPlayerGuessing = true;
                    UpdateClientStatus();
                }
            }
        }


        public void PlayMusic(DataTransporter clientData, bool playMusic)
        {
            lock (locker)
            {
                if (ValidateClient(clientData, PermissionLevel.PERMISSIONS_ADMIN))
                {
                    foreach (var i in _clients)
                    {
                        if (i.Value.PermissionLevel == PermissionLevel.PERMISSIONS_GUI)
                        {
                            if (((ICommunicationObject)(i.Value.Callback)).State == CommunicationState.Opened)
                            {
                                try
                                {
                                    i.Value.Callback.OnPlayMusic(playMusic);
                                }
                                catch (Exception e)
                                {

                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region General Accessor functions
        public bool Ping(DataTransporter clientData)
        {
            return ValidateClient(clientData, PermissionLevel.PERMISSIONS_OBSERVER);
        }
        public void GetSettings(DataTransporter client)
        {
            UpdateClientSettings();
        }


        #endregion

        #region Updaters


        private GameStatus GetGameStatusTransport()
        {
            if (_gameInstance.CurrentCategory != null)
            {
                
            }
            var list = new List<StringWithIDTransporter>();
            if (_gameInstance.NextItems != null)
            {
                foreach (var item in _gameInstance.NextItems)
                {
                    list.Add(item.GetCategoryTransport());
                }
            }
            var status = new GameStatus()
            {
                CurrentPlayerIndex = _gameInstance.CurrentPlayerIndex,
                IsFinalRoundPlayerGuessing = _gameInstance.IsFinalRoundPlayerGuessing,
                CurrentCategory = _gameInstance.CurrentCategory.GetCategoryTransport(),
                CurrentName = _gameInstance.CurrentCategory.CurrentItem,
                CurrentRound = _gameInstance.CurrentRound,
                Players = _gameInstance.Players.ToArray(),
                IsFinalRound = _gameInstance.IsFinalRound,
                RoundPhase = _gameInstance.RoundPhase,
                SelectableCategories = list.ToArray(),
                CurrentItemScoreValue = _gameInstance.AnswerValue,
                CurrentDisplayName = _gameInstance.CurrentDisplayName
                
            };
            return status;
        }

        private ItemDatabaseTransporter GetItemsTransport()
        {
            var transporter = new ItemDatabaseTransporter() {ItemDatabase = _gameInstance.ItemDatabase};
            return transporter;;
        }

        private SetupTransporter GetSettingsTransport()
        {
            var transporter = new SetupTransporter() {Settings = _gameInstance.Settings};
            return transporter;
        }
        private ClockTransporter GetTimerTransport()
        {
            var transporter = new ClockTransporter() { CurrentTime = _gameInstance.CurrentTimerValue, StartTime = _gameInstance.Settings.RoundLength};
            return transporter;
        }

        private void UpdateClientTimer()
        {

            var status = GetTimerTransport();
            foreach (var i in _clients)
            {
                if (((ICommunicationObject)(i.Value.Callback)).State == CommunicationState.Opened)
                {
                    try
                    {
                        i.Value.Callback.OnClockUpdate(status);
                    }
                    catch (Exception e)
                    {

                    }
                }

            }
        }


        private void TriggerAudio(SoundCue cue)
        {
            
            foreach (var i in _clients)
            {
                if (((ICommunicationObject)(i.Value.Callback)).State == CommunicationState.Opened)
                {
                    try
                    {
                        i.Value.Callback.OnAudioCue(cue);
                    }
                    catch (Exception e)
                    {

                    }
                }

            }
        }

        private void UpdateClientStatus()
        {

            var status = GetGameStatusTransport();
            foreach (var i in _clients)
            {
                if (((ICommunicationObject)(i.Value.Callback)).State == CommunicationState.Opened)
                {
                    try
                    {
                        i.Value.Callback.OnStatusUpdate(status);
                    }
                    catch (Exception e)
                    {

                    }
                }

            }
        }
        private void UpdateItems()
        {

            var status = GetItemsTransport();
            foreach (var i in _clients)
            {
                if (i.Value.PermissionLevel == PermissionLevel.PERMISSIONS_ADMIN &&
                    ((ICommunicationObject)(i.Value.Callback)).State == CommunicationState.Opened)
                {
                    try
                    {
                        i.Value.Callback.OnItemsUpdate(status);
                    }
                    catch (Exception e)
                    {

                    }
                }

            }
        }
        private void UpdateClientSettings()
        {

            var status = GetSettingsTransport();
            foreach (var i in _clients)
            {
                if (i.Value.PermissionLevel == PermissionLevel.PERMISSIONS_ADMIN &&
                    ((ICommunicationObject)(i.Value.Callback)).State == CommunicationState.Opened)
                {
                    try
                    {
                        i.Value.Callback.OnSettingsUpdate(status);
                    }
                    catch (Exception e)
                    {

                    }
                }

            }
        }
        
        #endregion
    }
}
