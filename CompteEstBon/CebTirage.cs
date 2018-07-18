#region using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

#endregion using

namespace CompteEstBon
{
    /// <summary>
    /// Gestion tirage Compte est bon
    /// </summary>
    [System.Runtime.InteropServices.Guid("EC9CF01C-34A0-414C-BF2A-D06C5A61503D")]
    public sealed class CebTirage
    {
        private int _search;

        /// <summary>
        /// Constructeur Tirage du Compte est bon
        /// </summary>
        /// <param name="search"></param>
        /// <param name="plaques"></param>

        public CebTirage(int search = -1, params int[] plaques) {
            for (var i = 0; i < 6; i++) {
                _plaques.Add(new CebPlaque(i >= plaques.Length ? 0 : plaques[i], (sender, v) => { if (ActiveEvent) Clear(); }));
            }
            if (search == -1 || plaques.Length < 6)
                Random();
            else
                Search = search;
        }

        public bool ActiveEvent { get; set; }

        public bool Sorted { get; set; } = true;

        /// <summary>
        /// nombre à chercher
        /// </summary>
        public int Search
        {
            get => _search;
            set
            {
                if (value == _search) return;
                _search = value;
                Clear();
            }
        }

        /// <summary>
        /// Nombre de solutions
        /// </summary>
        public int Count => Solutions.Count;

        /// <summary>
        /// Ecart
        /// </summary>
        public int Diff { get; private set; } = int.MaxValue;

        /// <summary>
        /// Liste des plaques
        /// </summary>
        private List<CebPlaque> _plaques = new List<CebPlaque>();

        public ReadOnlyCollection<CebPlaque> Plaques => _plaques.AsReadOnly();

        public void SetPlaques(params int[] plaq)
        {
            ActiveEvent = false;
            for (var i = 0; i < Min(plaq.Length, _plaques.Count); i++)
                _plaques[i].Value = plaq[i];
            Clear();
            ActiveEvent = true;
        }

        /// <summary>
        /// Liste des solutions
        /// </summary>
        public List<CebBase> Solutions { get; } = new List<CebBase>();

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <returns>
        /// </returns>
        /// <value>
        /// The status.
        /// </value>
        public CebStatus Status { get; private set; } = CebStatus.Indefini;

        /// <summary>
        /// Return the find values
        /// </summary>
        public CebFind Found { get; } = new CebFind();

        /// <summary>
        /// Find Value
        /// </summary>
        // private CebFind CebFind { get; } = new CebFind();

        /// <summary>
        /// Valid
        /// </summary>
        public CebStatus Valid()
        {
            Status = SearchValid && PlaquesValid ? CebStatus.Valid : CebStatus.Erreur;
            return Status;
        }

        public CebBase Solution => Solutions.Count == 0 ? null : Solutions[0];

        /// <summary>
        /// Valid the search value
        /// </summary>
        /// <returns>
        /// </returns>
        public bool SearchValid => _search > 99 && _search < 1000;

        public bool PlaquesValid => _plaques.All(p => p.IsValid
                                                     && _plaques.Count(q => q.Value == p.Value) <=
                                                     CebPlaque.ListePlaques.Count(n => n == p.Value)) && _plaques.Count == 6;

        /// <summary>
        /// Select the value and the plaque's list
        /// </summary>
        public void Random()
        {
            // EnableEvents(false);
            ActiveEvent = false;
            var rnd = new Random();
            _search = rnd.Next(100, 1000);

            IList<int> liste = new List<int>(CebPlaque.ListePlaques);
            foreach (var plaque in _plaques)
            {
                var n = rnd.Next(0, liste.Count);
                plaque.Value = liste[n];
                liste.RemoveAt(n);
            }
            ActiveEvent = true;
            Clear();
        }

        public async Task RandomAsync() => await Task.Run(() => Random());

        public void Clear()
        {
            Solutions.Clear();
            Diff = int.MaxValue;
            Found.Reset();
            Valid();
        }

        public async Task ClearAsync() => await Task.Run(() => Clear());

        /// <summary>
        /// resolution du compte
        /// </summary>
        /// <param name="search">
        /// Valeur à rechercher
        /// </param>
        /// <param name="plq">
        /// Liste des plaques
        /// </param>
        /// <returns>
        /// </returns>
        public CebStatus Resolve(int search, params int[] plq)
        {
            if (plq.Length != 6)
                throw new ArgumentException("Nombre de plaques incorrecte");
            _search = search;
            ActiveEvent = false;
            for (var i = 0; i < 6; i++)
                _plaques[i].Value = plq[i];
            ActiveEvent = true;
            return Resolve();
        }

        /// <summary>
        /// resolution
        /// </summary>
        /// <returns>
        /// </returns>
        public CebStatus Resolve()
        {
            void Resolve(List<CebBase> liste)
            {
                void AddSolution(CebBase sol)
                {
                    var diff = Abs(_search - sol.Value);
                    if (diff > Diff)
                        return;

                    if (Solutions.Contains(sol))
                        return;

                    if (diff < Diff)
                    {
                        Diff = diff;
                        Solutions.Clear();
                        Found.Reset();
                    }
                    Found.Add(sol.Value);
                    Solutions.Add(sol);
                }

                liste.Sort((p, q) => q.Value.CompareTo(p.Value));
                
                for (var i = 0; i < liste.Count; i++)
                {
                    AddSolution(liste[i]);
                    for (var j = i + 1; j < liste.Count; j++)
                    {
                        foreach (var oper in
                            // ReSharper disable AccessToModifiedClosure
                            CebOperation.ListeOperations.Select(operation => new CebOperation(liste[i], operation, liste[j]))
                                .Where(oper => oper.IsValid))
                            Resolve(
                                liste.Where((t, k) => k != i && k != j).Concat(new CebBase[] { oper }).ToList());
                    }
                }
            }

            Clear();
            if (Status != CebStatus.Valid) return Status;
            Resolve(_plaques.Cast<CebBase>().ToList());
            if (Sorted)
                Solutions.Sort((p, q) => p.Rank.CompareTo(q.Rank));
            Status = Diff == 0 ? CebStatus.CompteEstBon : CebStatus.CompteApproche;
            return Status;
        }

        public async Task<CebStatus> ResolveAsync() => await Task.Run(() => Resolve());

        public async Task<CebStatus> ResolveAsync(int search, params int[] plq) => await Task.Run(() => Resolve(search, plq));

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.AppendLine("## Tirage du compte est bon ###");
            buffer.AppendLine($"Search : {this.Search}, plaques : [{string.Join(",", _plaques.Select(p => p.ToString()))}]");
            buffer.AppendLine($"Found:  {this.Found}, Status: {Status}, Nb de solutions: {Count}");
            buffer.AppendLine(string.Join(";", Solutions.Select((p) => p.ToString())));
            return buffer.ToString();
        }
        public static (CebStatus status, CebFind found, List<string> Solutions ) Calcul(int search, params int[] plaques) {
            var t = new CebTirage();
            t.Resolve(search, plaques);
            return (t.Status, t.Found, t.Solutions.Select(p => p.ToString()).ToList());
        }
        public static (CebStatus status, CebFind found, List<string> Solutions) Calcul() {
            var t = new CebTirage();
            t.Resolve();
            return (t.Status, t.Found, t.Solutions.Select(p => p.ToString()).ToList());
        }
    }
}