using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using Nomencontest.Base.Models;

namespace Nomencontest.Base.ViewModels
{
    [DataContract]
    public class CategoryVM : PropertyClass, IItemVM
    {
        [DataMember]
        private CategoryModel _model;

        [DataMember]
        public CategoryModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                SetupQuestionOrder();
                RaisePropertyChanged("Model");
                RaisePropertyChanged("Category");
                RaisePropertyChanged("CurrentCategory");
                RaisePropertyChanged("CurrentItemIndex");
            }
        }

        public StringWithIDTransporter GetCategoryTransport()
        {
            return Model != null ? new StringWithIDTransporter {ID = Model.ID, Value = Model.Category} : new StringWithIDTransporter();

        }

        public CategoryVM(CategoryModel model)
        {
            _model = model;
            SetupQuestionOrder();
        }

        private int[] _questionOrder;

        private void SetupQuestionOrder()
        {
            _questionOrder = new int[_model.Items.Length];
            for (int i = 0; i < _model.Items.Length; i++)
            {
                _questionOrder[i] = i;
            }
            _currentItemIndex = 0;
        }

        public string Category { get { return _model.Category; } }
        private int _currentItemIndex;

        public int CurrentItemIndex
        {
            get { return _questionOrder.Length > 0 && _currentItemIndex >= 0 && _currentItemIndex < _questionOrder.Length ? _questionOrder[_currentItemIndex] : -1; }
        }

        public string CurrentItem
        {
            get { return CurrentItemIndex >= 0 ? _model.Items[CurrentItemIndex] : string.Empty; }
        }
        
        public int NextItem()
        {
            if (_currentItemIndex < _questionOrder.Length)
            {
                _currentItemIndex++;

            }
            else { _currentItemIndex = -1;}

            RaisePropertyChanged("CurrentItemIndex");
            RaisePropertyChanged("CurrentCategory");
            return CurrentItemIndex;
        }

        public bool ShuffleItems()
        {
            Random rng = new Random();
            int n = _questionOrder.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                int value = _questionOrder[k];
                _questionOrder[k] = _questionOrder[n];
                _questionOrder[n] = value;
            }
            return true;
        }
    }
}
