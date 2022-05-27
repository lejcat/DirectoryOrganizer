using DirectoryOrganizer.Services;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryOrganizer
{
    public class MainWindowViewModelBase : ViewModelBase
    {
        public MainWindowViewModelBase()
        {
            DataModel = new MainWindowDataModel();
        }

        protected MainWindowDataModel DataModel { get; set; }

        private RelayCommand _PageLoadedCommand { get; set; }
        public RelayCommand PageLoadedCommand
        {
            get { return _PageLoadedCommand; }
            set
            {
                if (_PageLoadedCommand == value) return;
                _PageLoadedCommand = value;
            }
        }
        private RelayCommand _FindPathCommand { get; set; }
        public RelayCommand FindPathCommand
        {
            get { return _FindPathCommand; }
            set
            {
                if (_FindPathCommand == value) return;
                _FindPathCommand = value;
            }
        }
        private RelayCommand _SaveConfigCommand { get; set; }
        public RelayCommand SaveConfigCommand
        {
            get { return _SaveConfigCommand; }
            set
            {
                if (_SaveConfigCommand == value) return;
                _SaveConfigCommand = value;
            }
        }
        private RelayCommand _RunCommand { get; set; }
        public RelayCommand RunCommand
        {
            get { return _RunCommand; }
            set
            {
                if (_RunCommand == value) return;
                _RunCommand = value;
            }
        }
        private RelayCommand _RunCheckCommand { get; set; }
        public RelayCommand RunCheckCommand
        {
            get { return _RunCheckCommand; }
            set
            {
                if (_RunCheckCommand == value) return;
                _RunCheckCommand = value;
            }
        }

        private string _PathString { get; set; } = string.Empty;
        public string PathString
        {
            get { return _PathString; }
            set
            {
                if (_PathString == value) return;
                _PathString = value;

                RaisePropertyChanged("PathString");
            }
        }

        private bool _CheckLastPathContents { get; set; } = false;
        public bool CheckLastPathContents
        {
            get { return _CheckLastPathContents; }
            set
            {
                if (_CheckLastPathContents == value) return;
                _CheckLastPathContents = value;

                RaisePropertyChanged("CheckLastPathContents");
            }
        }

        private string _LastPathLessContentsCount { get; set; } = "3";
        public string LastPathLessContentsCount
        {
            get { return _LastPathLessContentsCount; }
            set
            {
                if (_LastPathLessContentsCount == value) return;
                _LastPathLessContentsCount = value;

                RaisePropertyChanged("LastPathLessContentsCount");
            }
        }

        private ObservableCollection<string> _LogLines { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> LogLines
        {
            get { return _LogLines; }
            set
            {
                if (_LogLines == value) return;
                _LogLines = value;

                RaisePropertyChanged("LogLines");
            }
        }

        private bool _IsPageEnabled { get; set; } = true;
        public bool IsPageEnabled
        {
            get { return _IsPageEnabled; }
            set
            {
                if (_IsPageEnabled == value) return;
                _IsPageEnabled = value;

                RaisePropertyChanged("IsPageEnabled");
            }
        }
    }
}
