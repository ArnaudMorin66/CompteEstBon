using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CompteEstBon;

public class CebSearchValue(int v = 0) : INotifyPropertyChanged {
    private int _value = v;


    public event PropertyChangedEventHandler PropertyChanged;

    void OnPropertyChanged([CallerMemberName] string propertyName = "") {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public int Value {
        get => _value;
        set {
            if (_value == value)
                return;
            _value = value;
            OnPropertyChanged();
        }
    }

    public override string ToString() => Value.ToString();
}

