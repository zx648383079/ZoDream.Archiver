using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Windows.Input;
using Windows.System;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.Controls
{
    internal static class UICommand
    {
        public static ICommand Delete(Action<object?> execute) => Delete(new RelayCommand(execute));
        public static ICommand Delete(ICommand command) => new StandardUICommand(StandardUICommandKind.Delete)
        {
            Command = command
        };
        public static ICommand Play(Action<object?> execute) => Play(new RelayCommand(execute));
        public static ICommand Play(ICommand command) => new StandardUICommand(StandardUICommandKind.Play)
        {
            Command = command
        };
        public static ICommand Pause(Action<object?> execute) => Pause(new RelayCommand(execute));
        public static ICommand Pause(ICommand command) => new StandardUICommand(StandardUICommandKind.Pause)
        {
            Command = command
        };
        public static ICommand Stop(Action<object?> execute) => Stop(new RelayCommand(execute));
        public static ICommand Stop(ICommand command) => new StandardUICommand(StandardUICommandKind.Stop)
        {
            Command = command
        };
        public static ICommand Resume(Action<object?> execute) => Resume(new RelayCommand(execute));
        public static ICommand Resume(ICommand command) => new StandardUICommand(StandardUICommandKind.Redo)
        {
            Command = command
        };
        public static ICommand Save(Action<object?> execute) => Save(new RelayCommand(execute));
        public static ICommand Save(ICommand command) => new StandardUICommand(StandardUICommandKind.Save)
        {
            Command = command
        };
        public static ICommand SelectAll(Action<object?> execute) => SelectAll(new RelayCommand(execute));
        public static ICommand SelectAll(ICommand command) => new StandardUICommand(StandardUICommandKind.SelectAll)
        {
            Command = command
        };
        public static ICommand Backward(Action<object?> execute) => Backward(new RelayCommand(execute));
        public static ICommand Backward(ICommand command) => new StandardUICommand(StandardUICommandKind.Backward)
        {
            Command = command
        };
        public static ICommand Info(Action<object?> execute) => Info(new RelayCommand(execute));
        public static ICommand Info(ICommand command) => new XamlUICommand()
        {
            Label = "Info",
            Description = "View information",
            IconSource = new FontIconSource()
            {
                Glyph = "\uE946"
            },
            KeyboardAccelerators =
            {
                new KeyboardAccelerator()
                {
                    Key = VirtualKey.I,
                    Modifiers = VirtualKeyModifiers.Control
                }
            },
            Command = command
        };

        public static ICommand View(Action<object?> execute) => View(new RelayCommand(execute));
        public static ICommand View(ICommand command) => new XamlUICommand()
        {
            Label = "View",
            Description = "View",
            IconSource = new SymbolIconSource()
            {
                Symbol = Symbol.View
            },
            Command = command
        };
        public static ICommand Add(Action<object?> execute) => Add(new RelayCommand(execute));
        public static ICommand Add(ICommand command) => new XamlUICommand()
        {
            Label = "Add",
            Description = "Add files",
            IconSource = new SymbolIconSource()
            {
                Symbol = Symbol.Add
            },
            KeyboardAccelerators =
            {
                new KeyboardAccelerator()
                {
                    Key = VirtualKey.A,
                    Modifiers = VirtualKeyModifiers.Control
                }
            },
            Command = command
        };
        public static ICommand AddFolder(Action<object?> execute) => AddFolder(new RelayCommand(execute));
        public static ICommand AddFolder(ICommand command) => new XamlUICommand()
        {
            Label = "Add",
            Description = "Add folder",
            IconSource = new SymbolIconSource()
            {
                Symbol = Symbol.OpenLocal
            },
            Command = command
        };
        public static ICommand Find(Action<object?> execute) => Find(new RelayCommand(execute));
        public static ICommand Find(ICommand command) => new XamlUICommand()
        {
            Label = "Find",
            Description = "Find",
            IconSource = new SymbolIconSource()
            {
                Symbol = Symbol.Find
            },
            KeyboardAccelerators =
            {
                new KeyboardAccelerator()
                {
                    Key = VirtualKey.F,
                    Modifiers = VirtualKeyModifiers.Control
                }
            },
            Command = command
        };
        public static ICommand Setting(Action<object?> execute) => Setting(new RelayCommand(execute));
        public static ICommand Setting(ICommand command) => new XamlUICommand()
        {
            Label = "Setting",
            Description = "Setting",
            IconSource = new SymbolIconSource()
            {
                Symbol = Symbol.Setting
            },
            KeyboardAccelerators =
            {
                new KeyboardAccelerator()
                {
                    Key = VirtualKey.O,
                    Modifiers = VirtualKeyModifiers.Control
                }
            },
            Command = command
        };
        public static ICommand Index(Action<object?> execute) => Index(new RelayCommand(execute));
        public static ICommand Index(ICommand command) => new XamlUICommand()
        {
            Label = "资源目录",
            Description = "根据依赖文件生成资源索引",
            IconSource = new SymbolIconSource()
            {
                Symbol = Symbol.Link
            },
            Command = command
        };
        public static ICommand Log(Action<object?> execute) => Log(new RelayCommand(execute));
        public static ICommand Log(ICommand command) => new XamlUICommand()
        {
            Label = "日志文件",
            Description = "查看日志文件",
            IconSource = new SymbolIconSource()
            {
                Symbol = Symbol.CalendarDay
            },
            Command = command
        };

        public static ICommand Code(Action<object?> execute) => Code(new RelayCommand(execute));
        public static ICommand Code(ICommand command) => new XamlUICommand()
        {
            Label = "代码转化",
            Description = "根据相关文件进行解析代码生成",
            IconSource = new FontIconSource()
            {
                Glyph = "\uE943"
            },
            Command = command
        };
    }
}
