using System.ComponentModel;

public class CombinationEntry : INotifyPropertyChanged
{
    private bool _key1;
    private bool _key2;
    private bool _key3;
    private int _midiNote;

    public bool Key1
    {
        get { return _key1; }
        set
        {
            if (_key1 != value)
            {
                _key1 = value;
                OnPropertyChanged(nameof(Key1));
            }
        }
    }

    public bool Key2
    {
        get { return _key2; }
        set
        {
            if (_key2 != value)
            {
                _key2 = value;
                OnPropertyChanged(nameof(Key2));
            }
        }
    }

    public bool Key3
    {
        get { return _key3; }
        set
        {
            if (_key3 != value)
            {
                _key3 = value;
                OnPropertyChanged(nameof(Key3));
            }
        }
    }

    public int MidiNote
    {
        get { return _midiNote; }
        set
        {
            if (_midiNote != value)
            {
                _midiNote = value;
                OnPropertyChanged(nameof(MidiNote));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
