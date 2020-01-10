using System.ComponentModel;
using System.Runtime.CompilerServices;
using HostBinder.Annotations;

namespace HostBinder.Models
{
    public class HostItem: INotifyPropertyChanged
    {
        private bool _favorite;
        private string _comment;
        private string _name;
        private string _address;
        private int _lineNumber;

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

        public bool Favorite
        {
            get { return _favorite; }
            set
            {
                if (value.Equals(_favorite)) return;
                _favorite = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
