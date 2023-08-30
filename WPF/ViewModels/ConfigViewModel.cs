using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PreferenceManagerLibrary.Manager;
using PreferenceManagerLibrary.Preferences;
using PreferenceManagerLibrary.Preferences.Base;

namespace WPF.ViewModels
{
    public class ConfigViewModel : ObservableObject
    {
        public event EventHandler OnWindowCloseRequest;
        public IEnumerable<PreferenceBase> Preferences => preferenceManager.Preferences;

        private readonly EventWaitHandle @event;
        private readonly PreferenceManager preferenceManager;

        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand DefaultCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        public ConfigViewModel(PreferenceManager preferenceManager)
        {
            @event = new EventWaitHandle(false, EventResetMode.ManualReset);
            this.preferenceManager = preferenceManager;
            this.preferenceManager.BeginEdit();

            SaveCommand = new RelayCommand(Save, SaveCommandCanExecute);
            DefaultCommand = new RelayCommand(Default);
            CancelCommand = new RelayCommand(Cancel);
        }

        public void WaitForResult()
        {
            @event.WaitOne();
        }

        private void Save()
        {
            preferenceManager.EndEdit();
            preferenceManager.SavePreferences();

            OnWindowCloseRequest?.Invoke(this, EventArgs.Empty);
            @event.Set();
        }
        private void Default()
        {
            preferenceManager.CancelEdit();
            preferenceManager.DefaultPreferences();
            preferenceManager.BeginEdit();
        }
        private void Cancel()
        {
            preferenceManager.CancelEdit();

            OnWindowCloseRequest?.Invoke(this, EventArgs.Empty);
            @event.Set();
        }

        public bool SaveCommandCanExecute()
        {
            return preferenceManager.IsEditableValid;
        }
    }
}