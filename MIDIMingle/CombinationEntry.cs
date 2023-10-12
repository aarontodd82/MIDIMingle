using System;
using System.ComponentModel;

public class CombinationEntry : INotifyPropertyChanged
{
    private bool _key1;
    private bool _key2;
    private bool _key3;

    private bool _radioKey1;
    private bool _radioKey2;
    private bool _radioKey3;

    private int _midiNote;
    private bool? _isAlteration;
    private int? _alterationValue;
    public Guid Id { get; set; } = Guid.NewGuid();
    public string GroupName => Id.ToString();

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

    public bool RadioKey1
    {
        get { return _radioKey1; }
        set
        {
            if (_radioKey1 != value)
            {
                _radioKey1 = value;
                OnPropertyChanged(nameof(RadioKey1));
            }
        }
    }

    public bool RadioKey2
    {
        get { return _radioKey2; }
        set
        {
            if (_radioKey2 != value)
            {
                _radioKey2 = value;
                OnPropertyChanged(nameof(RadioKey2));
            }
        }
    }

    public bool RadioKey3
    {
        get { return _radioKey3; }
        set
        {
            if (_radioKey3 != value)
            {
                _radioKey3 = value;
                OnPropertyChanged(nameof(RadioKey3));
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

    public bool? IsAlteration
    {
        get { return _isAlteration; }
        set
        {
            if (_isAlteration != value)
            {
                _isAlteration = value;
                OnPropertyChanged(nameof(IsAlteration));
            }
        }
    }

    public int? AlterationValue
    {
        get { return _alterationValue; }
        set
        {
            if (_alterationValue != value)
            {
                _alterationValue = value;
                OnPropertyChanged(nameof(AlterationValue));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
