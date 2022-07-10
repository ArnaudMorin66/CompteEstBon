// Plage Compte est bon

using System.ComponentModel;
using System.Text.Json.Serialization;


namespace CompteEstBon {

    public sealed class CebPlaque : CebBase, INotifyPropertyChanging {

        // public static readonly List<Action<CebPlaque,int,int>> NotifyValueChange = new();

        public static readonly int[] AllPlaques = {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 25, 50, 75, 100,
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 25
        };
        public static readonly IEnumerable<int> AnyPlaques = new SortedSet<int>(AllPlaques);


        // public  event PropertyChangedEventHandler PropertyChanged;

        public CebPlaque(int v = 0, Action<object, PropertyChangingEventArgs> action = null) {
            Value = v;
            Operations.Add(Value.ToString());

            PropertyChanging += (o, e) => {
                action?.Invoke(o, e);
            };


        }


        [JsonIgnore]
        public string Text {
            get => Value.ToString();
            set => Value = int.TryParse(value, out var res) ? res : 0;
        }

        [JsonIgnore]
        public override int Value {
            get => base.Value;
            set {
                if (base.Value == value) return;
                OnPropertyChanging(nameof(Value));
                base.Value = value;
                Operations[0] = value.ToString();

            }
        }
        private void OnPropertyChanging(string propertyName) {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        [JsonIgnore]
        public override bool IsValid => AnyPlaques.Contains(Value);

        public event PropertyChangingEventHandler PropertyChanging;

        public static implicit operator int(CebPlaque p) => p.Value;

        // private void NotifyPropertyChanged([CallerMemberName] string propertyName = "Value") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}