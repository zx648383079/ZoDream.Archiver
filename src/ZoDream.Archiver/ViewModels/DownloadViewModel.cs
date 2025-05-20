using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            DeleteCommand = UICommand.Delete(new RelayCommand<DownloadItemViewModel>(TapDelete));
            PlayCommand = UICommand.Play(new RelayCommand<DownloadItemViewModel>(TapPlay));
            ResumeCommand = UICommand.Resume(new RelayCommand<DownloadItemViewModel>(TapResume));
            PauseCommand = UICommand.Pause(new RelayCommand<DownloadItemViewModel>(TapPause));
            StopCommand = UICommand.Stop(new RelayCommand<DownloadItemViewModel>(TapStop));
            DragCommand = new RelayCommand<IEnumerable<IStorageItem>>(TapDrag);
            ViewCommand = UICommand.View(TapView);
            SettingCommand = UICommand.Setting(TapSetting);
            _service = new NetService();
            _scheduler = new LimitedScheduler(_service, 5);
        }

        private readonly AppViewModel _app = App.ViewModel;
        private readonly INetService _service;
        private readonly IRequestScheduler _scheduler;

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
        public ICommand ResumeCommand { get; private set; }
        public ICommand PauseCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand DragCommand { get; private set; }
        public ICommand ViewCommand { get; private set; }
        public ICommand SettingCommand { get; private set; }
        private async void TapAdd(object? _)
        {
            var picker = new RequestDialog();
            var model = picker.ViewModel;
            if (!await _app.OpenFormAsync(picker))
            {
                return;
            }
            foreach (var item in model.Link.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(item) || 
                    !Uri.TryCreate(item, UriKind.Absolute, out var uri))
                {
                    continue;
                }
                Items.Add(new(this, uri)
                {
                    Name = uri.AbsolutePath,
                    Status = RequestStatus.Waiting,
                });
            }
        }
        private void TapDelete(DownloadItemViewModel? arg)
        {
            var items = arg is null ? SelectedItems?.Cast<DownloadItemViewModel>() : [arg];
            if (items is null)
            {
                return;
            }
            foreach (var item in items)
            {
                item.StopCommand.Execute(null);
                Items.Remove(item);
            }
            SelectedItems = null;
        }
        private void TapPlay(DownloadItemViewModel? arg)
        {
            var items = arg is null ? SelectedItems?.Cast<DownloadItemViewModel>() : [arg];
            if (items is null)
            {
                return;
            }
            foreach (var item in items)
            {
                if (item.Status is RequestStatus.Sending or RequestStatus.Receiving)
                {
                    continue;
                }
                _scheduler.Execute(item.CreateRequest());
            }
        }
        private void TapResume(DownloadItemViewModel? arg)
        {
            var items = arg is null ? SelectedItems?.Cast<DownloadItemViewModel>() : [arg];
            if (items is null)
            {
                return;
            }
            foreach (var item in items)
            {
                if (item.Status is not RequestStatus.Paused)
                {
                    continue;
                }
                item.ResumeCommand.Execute(null);
            }
        }

        private void TapPause(DownloadItemViewModel? arg)
        {
            var items = arg is null ? SelectedItems?.Cast<DownloadItemViewModel>() : [arg];
            if (items is null)
            {
                return;
            }
            foreach (var item in items)
            {
                if (item.Status is RequestStatus.Sending or RequestStatus.Receiving)
                {
                    item.PauseCommand.Execute(null);
                }
            }
        }
        private void TapStop(DownloadItemViewModel? arg)
        {
            var items = arg is null ? SelectedItems?.Cast<DownloadItemViewModel>() : [arg];
            if (items is null)
            {
                return;
            }
            foreach (var item in items)
            {
                if (item.Status >= RequestStatus.Finished)
                {
                    continue;
                }
                item.StopCommand.Execute(null);
            }
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

        public void LoadAsync(object parameter)
        {

        }

        public void UnloadAsync()
        {
            TapStop(null);
            _scheduler.Cancel();
            _scheduler.Dispose();
            _service.Dispose();
        }
    }
}
