namespace CompteEstBon {

    [System.Runtime.InteropServices.Guid("9CA27D73-CD46-41CE-B666-3F589F98D328")]
    public class CebFind {
        public CebFind() => Reset();
        public int Found1 { get; private set; }
        public int Found2 { get; private set; }
        public void Add(int value) {
            if (value == Found1 || value == Found2) return;
            if (value == int.MaxValue)
                Found2 = int.MaxValue;
            else if (value > Found1)
                Found2 = value;
            else {
                Found2 = Found1;
                Found1 = value;
            }
        }
        public override string ToString() => Found2 switch {
            int.MaxValue => Found1.ToString(),
            _ => $"{Found1} et {Found2}"
        };
        public bool IsUnique => Found2 == int.MaxValue;
        public void Reset(int value = int.MaxValue) {
            Found1 = value;
            Found2 = int.MaxValue;
        }
    }
}