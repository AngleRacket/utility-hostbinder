using System.ComponentModel;
using System.Runtime.CompilerServices;
using HostBinder.Annotations;

namespace HostBinder.Models
{
    public class TargetAddress : INotifyPropertyChanged
    {
        private int _lineNumber;
        private string _comment;
        private string _address;
        private string[] _tags;
        private string _name;

        public int LineNumber
        {
            get { return _lineNumber; }
            set
            {
                if (value == _lineNumber) return;
                _lineNumber = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public string[] Tags
        {
            get { return _tags; }
            set
            {
                if (value == _tags) return;
                _tags = value;
                OnPropertyChanged();
            }
        }

        public string Address
        {
            get { return _address; }
            set
            {
                if (value == _address) return;
                _address = value;
                OnPropertyChanged();
            }
        }

        public string Comment
        {
            get { return _comment; }
            set
            {
                if (value == _comment) return;
                _comment = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
