using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Input;
using Windows.Storage;
using ZoDream.Archiver.Controls;
using ZoDream.Archiver.Dialogs;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Net;

namespace ZoDream.Archiver.ViewModels
{
    public class DownloadViewModel : ObservableObject, IBundleContext
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
            _service = _app.Service;
            _scheduler = new LimitedScheduler(_service, 5);
        }

        private readonly AppViewModel _app = App.ViewModel;
        private readonly IEntryService _service;
        private readonly IBundleScheduler _scheduler;

        public IEntryService Service => _service;

        private ObservableCollection<DownloadItemViewModel> _items = [];
        public ObservableCollection<DownloadItemViewModel> Items {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        private IEntryItem[]? _selectedItems;

        public IEntryItem[]? SelectedItems {
            get => _selectedItems;
            set => SetProperty(ref _selectedItems, value);
        }

        private bool _isMultipleSelect;

        public bool IsMultipleSelect {
            get => _isMultipleSelect;
            set => SetProperty(ref _isMultipleSelect, value);
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

        

        private async void TapAdd()
        {
            var picker = new RequestDialog();
            var model = picker.ViewModel;
            if (!await _app.OpenFormAsync(picker))
            {
                return;
            }
            _service.AddIf(model.Executor);
            foreach (var item in model.Link.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(item) || 
                    !Uri.TryCreate(item, UriKind.Absolute, out var uri))
                {
                    continue;
                }
                Items.Add(new(this, uri)
                {
                    Name = NetService.GetFileName(uri),
                    Target = model.OutputFolder,
                    Status = BundleStatus.Waiting,
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
                if (item.Status is BundleStatus.Sending or BundleStatus.Receiving)
                {
                    continue;
                }
                Enqueue(item.CreateRequest());
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
                if (item.Status is not BundleStatus.Paused)
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
                if (item.Status is BundleStatus.Sending or BundleStatus.Receiving)
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
                if (item.Status >= BundleStatus.Completed)
                {
                    continue;
                }
                item.StopCommand.Execute(null);
            }
        }
        private void TapDrag(IEnumerable<IStorageItem>? items)
        {

        }
        private void TapView()
        {

        }

        private async void TapSetting()
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

        public void Enqueue(IBundleRequest request)
        {
            var executor = _service.Get<IBundleExecutor>();
            if (!executor.CanExecute(request))
            {
                return;
            }
            _scheduler.Execute(async () => {
                await executor.ExecuteAsync(request, this);
            });
        }

        public bool TryDequeue([NotNullWhen(true)] out IBundleRequest? request)
        {
            request = null;
            return false;
        }
    }
}
