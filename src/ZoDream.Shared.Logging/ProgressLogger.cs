namespace ZoDream.Shared.Logging
{
    public class ProgressLogger(ILogger host, bool isMaster) : IProgressLogger
    {
        private long _value;

        public bool IsMaster => isMaster;

        public string Title { get; set; } = string.Empty;
        public long Max { get; set; }
        public long Value 
        { 
            get => _value; 
            set {
                _value = value;
                if (host is EventLogger o)
                {
                    o.Emit(this);
                }
            }
        }

        public static ProgressLogger operator +(ProgressLogger arg, int offset)
        {
            arg.Value += offset;
            return arg;
        }


        public static explicit operator double(ProgressLogger arg)
        {
            if (arg.Max == 0 || arg.Value == 0)
            {
                return 0;
            }
            return (double)arg.Value * 100 / arg.Max;
        }
    }
}