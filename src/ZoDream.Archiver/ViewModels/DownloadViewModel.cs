using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.Storage;
using ZoDream.Archiver.Controls;
using ZoDream.Archiver.Dialogs;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Net;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class DownloadViewModel : BindableBase
    {

        public DownloadViewModel()
        {
            AddCommand = UICommand.Add(TapAdd);
            DeleteCommand = UICommand.Delete(TapDelete);
            PlayCommand = UICommand.Play(TapPlay);
            PauseCommand = UICommand.Pause(TapPause);
            StopCommand = UICommand.Stop(TapStop);
            DragCommand = new RelayCommand<IEnumerable<IStorageItem>>(TapDrag);
            ViewCommand = UICommand.View(TapView);
            SettingCommand = UICommand.Setting(TapSetting);
            Items.Add(new() { Name = "hh.zip" });
            Items.Add(new() { Name = "a.zip", Status = RequestStatus.Receiving, Length = 100, Value=40, Speed = 10 });
            Items.Add(new() { Name = "vh.zip", Status = RequestStatus.Finished });
            Items.Add(new() { Name = "c.zip", Status = RequestStatus.Canceled });
            Items.Add(new() { Name = "c.zip", Status = RequestStatus.Occurred });
        }

        private readonly AppViewModel _app = App.ViewModel;

        private ObservableCollection<DownloadItemViewModel> _items = [];

        public ObservableCollection<DownloadItemViewModel> Items {
            get => _items;
            set => Set(ref _items, value);
        }

        private IEntryItem[]? _selectedItems;

        public IEntryItem[]? SelectedItems {
            get => _selectedItems;
            set => Set(ref _selectedItems, value);
        }

        private bool _isMultipleSelect;

        public bool IsMultipleSelect {
            get => _isMultipleSelect;
            set => Set(ref _isMultipleSelect, value);
        }


        public ICommand AddCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand PlayCommand { get; private set; }
        public ICommand PauseCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand DragCommand { get; private set; }
        public ICommand ViewCommand { get; private set; }
        public ICommand SettingCommand { get; private set; }
        private async void TapAdd(object? _)
        {
            var picker = new RequestDialog();
            if (!await _app.OpenFormAsync(picker))
            {
                return;
            }
        }
        private void TapDelete(object? _)
        {
            if (SelectedItems is null)
            {
                return;
            }
            foreach (var item in SelectedItems)
            {
                Items.Remove(item as DownloadItemViewModel);
            }
            SelectedItems = null;
        }
        private void TapPlay(object? _)
        {

        }
        private void TapPause(object? _)
        {

        }
        private void TapStop(object? _)
        {

        }
        private void TapDrag(IEnumerable<IStorageItem>? items)
        {

        }
        private void TapView(object? _)
        {

        }

        private async void TapSetting(object? _)
        {
            var picker = new SettingDialog();
            var res = await _app.OpenDialogAsync(picker);
            if (res != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                return;
            }
            await picker.ViewModel.SaveAsync();
        }
    }
}
