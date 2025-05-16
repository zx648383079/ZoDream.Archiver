using System;
using System.Text.RegularExpressions;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class SearchDialogViewModel: BindableBase
    {

        private Regex? _computedRegex;
        private string _pattern = string.Empty;

        public string Pattern {
            get => _pattern;
            set {
                Set(ref _pattern, value);
                _computedRegex = null;
            }
        }


        private bool _useRegex;

        public bool UseRegex {
            get => _useRegex;
            set => Set(ref _useRegex, value);
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
