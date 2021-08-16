namespace CompteEstBon {

#pragma warning disable CA1815 // Override equals and operator equals on value types

    public struct CebData {
#pragma warning restore CA1815 // Override equals and operator equals on value types
        public int Search { get; set; }
        public int[] Plaques { get; set; }
        public CebStatus? Status { get; set; }
        public string Found { get; set; }
        public int? Diff { get; set; }
        public IEnumerable<CebBase> Solutions { get; set; }
    }
}
