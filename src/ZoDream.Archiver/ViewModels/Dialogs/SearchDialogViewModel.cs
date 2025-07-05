using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Text.RegularExpressions;

namespace ZoDream.Archiver.ViewModels
{
    public class SearchDialogViewModel: ObservableObject
    {

        private Regex? _computedRegex;
        private string _pattern = string.Empty;

        public string Pattern {
            get => _pattern;
            set {
                SetProperty(ref _pattern, value);
                _computedRegex = null;
            }
        }


        private bool _useRegex;

        public bool UseRegex {
            get => _useRegex;
            set => SetProperty(ref _useRegex, value);
        }


        public bool IsMatch(string name)
        {
            if (string.IsNullOrWhiteSpace(Pattern))
            {
                return true;
            }
            if (!UseRegex)
            {
                return name.Contains(Pattern);
            }
            try
            {
                _computedRegex ??= new Regex(Pattern);
            }
            catch (Exception)
            {
                Pattern = string.Empty;
                return true;
            }
            return _computedRegex.IsMatch(name);
        }
    }
}
