using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using System.Threading.Tasks;
using ZoDream.BundleExtractor;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.Archiver.ViewModels
{
    public class RecognizeDialogViewModel : ObservableObject
    {

        private string _message = string.Empty;

        public string Message {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private string _result = string.Empty;

        public string Result {
            get => _result;
            set => SetProperty(ref _result, value);
        }


        public async void Load(BundleScheme scheme, IBundleOptions? options, string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }
            Message = "Recognizing...";
            var data = await scheme.Get<IBundleEngine>(options ?? new BundleOptions()
            {
                Engine = "Unity"
            })
                .RecognizeAsync(new Shared.IO.StorageFileEntry(filePath));
            if (data is null || data.Contains(UnknownCommandArgument.TagName))
            {
                Message = "Unrecognized file format.";
                return;
            }
            Message = "Recognition completed.";
            Result = data.Count > 0 ? data.ToString() : "正常有效文件！";
        }
    }
}
