﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SoftwareQualityPrediction.Utils;

namespace SoftwareQualityPrediction.ViewModels
{
    public class SelectItemsViewModel : BaseViewModel, IColleague
    {
        public IMediator Mediator { get; set; }

        public SelectItemsViewModel()
        {
            _unselectedItems = new ObservableCollection<string>();
            _selectedItems = new ObservableCollection<string>();
        }

        public ICommand AddCommand =>
            _addCommand ?? (_addCommand = new CommandHandler(AddItem, () => true));

        public ICommand RemoveCommand =>
            _removeCommand ?? (_removeCommand = new CommandHandler(RemoveItem, () => true));

        public ObservableCollection<string> UnselectedItems
        {
            get { return _unselectedItems; }
            set
            {
                _unselectedItems = value;
                OnPropertyChanged(nameof(UnselectedItems));
            }
        }

        public ObservableCollection<string> SelectedItems
        {
            get { return _selectedItems; }
            set
            {
                _selectedItems = value;
                OnPropertyChanged(nameof(SelectedItems));
            }
        }

        public string ItemToAdd
        {
            get { return _itemToAdd; }
            set
            {
                _itemToAdd = value;
                OnPropertyChanged(nameof(ItemToAdd));
                Send(null);
            }
        }

        public string ItemToRemove
        {
            get { return _itemToRemove; }
            set
            {
                _itemToRemove = value;
                OnPropertyChanged(nameof(ItemToRemove));
                Send(null);
            }
        }

        #region Mediator Implementation

        public void Send(object message)
        {
            Mediator?.Send(message, this);
        }

        public void Receive(object message)
        {
            var columns = (List<string>) message;
            UnselectedItems = new ObservableCollection<string>(columns);
            SelectedItems.Clear();
        }

        #endregion

        private void AddItem()
        {
            if (ItemToAdd != null)
            {
                SelectedItems.Add(ItemToAdd);
                UnselectedItems.Remove(ItemToAdd);
                OnPropertyChanged(nameof(SelectedItems));
                OnPropertyChanged(nameof(UnselectedItems));
            }
        }

        private void RemoveItem()
        {
            if (ItemToRemove != null)
            {
                UnselectedItems.Add(ItemToRemove);
                SelectedItems.Remove(ItemToRemove);
                OnPropertyChanged(nameof(SelectedItems));
                OnPropertyChanged(nameof(UnselectedItems));
            }
        }

        private ObservableCollection<string> _unselectedItems;
        private ObservableCollection<string> _selectedItems;
        private string _itemToAdd;
        private string _itemToRemove;
        private ICommand _addCommand;
        private ICommand _removeCommand;
    }
}
