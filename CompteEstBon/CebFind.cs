namespace CompteEstBon {
    /// <summary>
    /// 
    /// </summary>
    [System.Runtime.InteropServices.Guid("9CA27D73-CD46-41CE-B666-3F589F98D328")]
    public class CebFind {
        /// <summary>
        /// 
        /// </summary>
        public CebFind() => Reset();
        /// <summary>
        /// 
        /// </summary>
        public int Found1 { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public int Found2 { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Found2 == int.MaxValue ? Found1.ToString() : $"{Found1} et {Found2}";
        /// <summary>
        /// 
        /// </summary>
        public bool IsUnique => Found2 == int.MaxValue;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Reset(int value = int.MaxValue) {
            Found1 = value;
            Found2 = int.MaxValue;
        }
    }
}