using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Forturna.Base.ViewModels
{
    public interface IItemVM : INotifyPropertyChanged
    {
        string Answer { get; }
        string Hint { get; }
        double BaseValue { get; }
        char[] GuessedLetters { get; }
        char[] RevealedLetters { get; }
        char[] RemainingLetters { get; }
        char LatestLetter { get; }

        ObservableCollection<List<TileVM>> Rows { get; }

        bool HasVowels();
        void RevealAnswer();
        uint HasLetter(char letter);
        List<TileVM> GetTilesToFlip(char letter);



    }
}
