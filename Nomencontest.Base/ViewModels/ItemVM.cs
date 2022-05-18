using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using Forturna.Base.Models;

namespace Forturna.Base.ViewModels
{
    [DataContract]
    public class ItemVM : PropertyClass, IItemVM
    {
        [DataMember]
        private ItemModel _model;

        [DataMember]
        public ItemModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                RaisePropertyChanged("Answer");
                RaisePropertyChanged("Hint");
                RaisePropertyChanged("BaseValue");
                RaisePropertyChanged("LettersInAnswer");
                RaisePropertyChanged("Model");                
                SetupRows(true);
            }
        }

        [DataMember]
        public string Answer
        {
            get { return _model != null ? _model.Answer.Replace('\n',' '):string.Empty; }
            set { }
        }

        [DataMember]
        public string Hint
        {
            get { return _model != null ? _model.Hint : string.Empty; }
            set { }
        }

        [DataMember]
        public double BaseValue
        {
            get { return _model != null ? _model.BaseValue : 0; }
            set { }
        }


        public bool HasVowels()
        {
            return _model != null ? ((_model.Answer.Contains('A') &&
                _model.Answer.Contains('E') &&
                _model.Answer.Contains('I') &&
                _model.Answer.Contains('O') &&
                _model.Answer.Contains('U') &&
                _model.Answer.Contains('Y'))) : false;
        }

        [DataMember]
        private List<char> _guessedLetters;

        [DataMember]
        public char[] GuessedLetters
        {
            get { return _guessedLetters == null || _guessedLetters.Count == 0 ? new char[0] : _guessedLetters.ToArray(); }

            set { }
        }

        [DataMember]
        public char[] RevealedLetters
        {
            get
            {
                return _model != null ? _guessedLetters.Where(letter => _model.Answer.Contains(letter)).ToArray() : new char[0];
            }
            set { }
        }

        [DataMember]
        public char[] RemainingLetters
        {
            get
            {
                var remainingLetters = new List<char>();
                if (_model != null)
                {
                    for (char letter = 'A'; letter <= 'Z'; letter++)
                    {
                        if (!_model.Answer.Contains(letter) && !_guessedLetters.Contains(letter))
                        {
                            remainingLetters.Add(letter);
                        }
                    }
                }
                return remainingLetters.ToArray();
            }

            set { }
        }

        public uint HasLetter(char letter)
        {
            if (!_model.Answer.Contains(letter)) return 0;
            uint count = 0;
            foreach (var i in _model.Answer.Where(i => i == letter))
            {
                count++;
            }
            return count;
        }

        [DataMember]
        public char LatestLetter
        {
            get { return _guessedLetters != null && _guessedLetters.Count >0 ? _guessedLetters.Last() : new char(); }
            set { }
        }


        //public uint RevealLetter(char letter)
        //{
        //    if (_guessedLetters.Contains(letter)) return 0;

        //    _guessedLetters.Add(letter);
        //    RaisePropertyChanged("RevealedLetters");
        //    RaisePropertyChanged("RemainingLetters");
        //    RaisePropertyChanged("GuessedLetters");
        //    RaisePropertyChanged("RevealedAnswer");
        //    UpdateRows();
        //    return HasLetter(letter);
        //}

        public List<TileVM> GetTilesToFlip(char letter = '*')
        {
            var tiles = new List<TileVM>();
            if (letter == '*' || HasLetter(letter)>0)
            {
                foreach (var row in _rows)
                {
                    foreach (var tile in row)
                    {
                        if ((letter == '*' ) || (tile.Value == letter && tile.Visibility == Visibility.Collapsed)) tiles.Add(tile);
                    }
                }
            }
            if (!_guessedLetters.Contains(letter)) _guessedLetters.Add(letter);
            tiles.Sort(delegate(TileVM x, TileVM y)
            {
                if (x == null && y == null) return 0;
                else if (x == null) return -1;
                else if (y == null) return 1;
                else
                {
                    var comparer = x.CurrentPosition.X.CompareTo(y.CurrentPosition.X);
                    if (comparer == 0)
                    {
                        comparer = x.CurrentPosition.Y.CompareTo(y.CurrentPosition.Y);
                    }
                    return comparer;
                }
            });

            return tiles;
        }

        private void SetupRows(bool instantLoad = false)
        {
            _rows = new ObservableCollection<List<TileVM>>();
            if (_model == null) return;
            var rows = _model.Answer.Split('\n');

            int rowNumber = 0;
            int colNumber = 0;
            rowNumber = (int)Math.Floor((4-rows.Count())/2f);
                foreach (var rowList in rows.Select(row => row.Select(t => new TileVM(t, rowNumber, colNumber++){HideLetter = instantLoad}).ToList()))
                {
                    _rows.Add(rowList);
                    rowNumber++;
                    colNumber = 0;

                }

                        if (_guessedLetters == null) _guessedLetters = new List<char>();
                        RaisePropertyChanged("Rows");
                        RaisePropertyChanged("RevealedLetters");
                        _guessedLetters.Clear();
                        RaisePropertyChanged("GuessedLetters");
                        RaisePropertyChanged("RemainingLetters");
                        RaisePropertyChanged("RevealedAnswer");
        }

        public void RevealAnswer()
        {
            foreach (var character in _rows.SelectMany(row => row))
            {
                character.FlipTile();
            }
        }

        //private void UpdateRows(bool oneAtATime = false)
        //{
        //    foreach (var character in _rows.SelectMany(row => row))
        //    {
        //        switch (character.Visibility)
        //        {
        //            case Visibility.Collapsed:
        //                if (_guessedLetters.Contains(character.Value)) character.Visibility = Visibility.Hidden;
        //                break;
        //            case Visibility.Hidden:
        //                character.Visibility = Visibility.Visible;
        //                if (oneAtATime) return;
        //                break;
        //        }
        //    }
        //}



        [DataMember]
        private ObservableCollection<List<TileVM>> _rows;

        [DataMember]
        public ObservableCollection<List<TileVM>> Rows
        {
            get { return _rows; }
            set { }
        }

        public ItemVM(ItemModel model)
        {
            _model = model;
            _guessedLetters = new List<char>();
            SetupRows();
            RaisePropertyChanged("Answer");
            RaisePropertyChanged("Hint");
        }
    }
}
