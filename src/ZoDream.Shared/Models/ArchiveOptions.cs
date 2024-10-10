using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Models
{
    public class ArchiveOptions: IArchiveOptions
    {
        public bool LeaveStreamOpen { get; set; } = true;
        public bool LookForHeader { get; set; }
        public string? Password { get; set; }
        public string? Dictionary { get; set; }

        public ArchiveOptions()
        {
            
        }

        public ArchiveOptions(IArchiveOptions? options)
        {
            if (options is null)
            {
                return;
            }
            LeaveStreamOpen = options.LeaveStreamOpen;
            LookForHeader = options.LookForHeader;
            Dictionary = options.Dictionary;
            Password = options.Password;
        }

        public ArchiveOptions(string password)
        {
            Password = password;
        }

        public ArchiveOptions(string password, string dictionaryFileName)
            : this(password)
        {
            Dictionary = dictionaryFileName;
        }
    }
}
