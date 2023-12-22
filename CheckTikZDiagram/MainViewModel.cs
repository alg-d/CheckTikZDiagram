using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CheckTikZDiagram
{
    class MainViewModel : BindableBase
    {
        private readonly MainModel _model = new MainModel();

        public ObservableCollection<CheckResult> CheckResultCollection { get; } = new ObservableCollection<CheckResult>();

        public Config Config => Config.Instance;

        public bool ErrorOnly
        {
            get { return _model.ErrorOnly; }
            set { _model.ErrorOnly = value; }
        }

        private string _readFilePath = "";
        public string ReadFilePath
        {
            get { return _readFilePath; }
            set 
            {
                SetProperty(ref _readFilePath, value);
                RaisePropertyChanged(nameof(NotEmptyPath));
            }
        }

        public bool NotEmptyPath => !ReadFilePath.IsNullOrEmpty();

        private int _readFileLine = 100;
        public int ReadFileLine
        {
            get { return _readFileLine; }
            set { SetProperty(ref _readFileLine, value); }
        }

        private int _progressValue = 0;
        public int Progressvalue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }

        private int _morphismCount = 0;
        public int MorphismCount
        {
            get { return _morphismCount; }
            set { SetProperty(ref _morphismCount, value); }
        }

        private int _errorCount = 0;
        public int ErrorCount
        {
            get { return _errorCount; }
            set { SetProperty(ref _errorCount, value); }
        }



        public MainViewModel()
        {
            _model.OutputEvent += (x) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (x.Message.IsNullOrEmpty())
                    {
                        Progressvalue = x.Line;
                    }
                    else
                    {
                        MorphismCount++;
                        if (x.IsError)
                        {
                            ErrorCount++;
                        }
                        if (x.DisplayFlag)
                        {
                            CheckResultCollection.Add(x);
                        }
                    }
                });
            };
        }

        private async Task ReadFile()
        {
            CheckResultCollection.Clear();
            ErrorCount = 0;
            MorphismCount = 0;

            var start = DateTime.Now;
            await Task.Run(() =>
            {
                var main = File.ReadAllText(ReadFilePath);
                ReadFileLine = main.Count(x => x == '\n');
                _model.MainLoop(main, ReadFileLine / 100 + 1);
            });
            Progressvalue = ReadFileLine;
            var end = DateTime.Now;
            MessageBox.Show("完了しました");
        }


        private DelegateCommand? _openFileCommand;
        public DelegateCommand OpenFileCommand
        {
            get
            {
                if (_openFileCommand == null)
                {
                    _openFileCommand = new DelegateCommand(async () =>
                    {
                        var dialog = new OpenFileDialog
                        {
                            Title = "ファイルを開く",
                            Filter = "TeXファイル(*.tex)|*.tex"
                        };
                        if (dialog.ShowDialog() == true)
                        {
                            ReadFilePath = dialog.FileName;
                            await ReadFile();
                        }
                    });
                }
                return _openFileCommand;
            }
        }

        private DelegateCommand? _saveCommand;
        public DelegateCommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new DelegateCommand(() =>
                    {
                        try
                        {
                            Config.Save();
                            MessageBox.Show("保存しました");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("設定の保存に失敗しました。" + ex.ToString());
                        }
                    });
                }
                return _saveCommand;
            }
        }

        private DelegateCommand? _reloadCommand;
        public DelegateCommand ReloadCommand
        {
            get
            {
                if (_reloadCommand == null)
                {
                    _reloadCommand = new DelegateCommand(async () =>
                    {
                        await ReadFile();
                    });
                }
                return _reloadCommand;
            }
        }
    }
}
