#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Math;


#endregion


namespace CompteEstBon
{
    /// <summary>
    ///     Gestion tirage Compte est bon
    /// </summary>
    [System.Runtime.InteropServices.Guid("EC9CF01C-34A0-414C-BF2A-D06C5A61503D")]
    public sealed class CebTirage
    {
        private int _search;

        /// <summary>
        ///     Constructeur Tirage du Compte est bon
        /// </summary>
        public CebTirage()
        {
            Plaques.Clear();
            for (var i = 0; i < 6; i++)
            {
                Plaques.Add(new CebPlaque(this, 0));
            }
            Random();
        }

        public bool Sorted { get; set; } = true;

        /// <summary>
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
        /// </summary>
        public int Count => Solutions.Count;

        /// <summary>
        /// </summary>
        public int Diff { get; private set; } = int.MaxValue;

        /// <summary>
        /// </summary>
        public List<CebPlaque> Plaques { get; } = new List<CebPlaque>();

        /// <summary>
        /// </summary>
        public List<CebBase> Solutions { get; } = new List<CebBase>();

        /// <summary>
        ///     Gets the status.
        /// </summary>
        /// <returns></returns>
        /// <value>The status.</value>
        public CebStatus Status { get; private set; } = CebStatus.Indefini;

        /// <summary>
        ///     Return the find values
        /// </summary>
        public string Found => CebFind.ToString();

        /// <summary>
        ///     Find Value
        /// </summary>
        private CebFind CebFind { get; } = new CebFind();

        /// <summary>
        ///     Valid
        /// </summary>
        public CebStatus Valid()
        {
            Status = SearchValid && PlaquesValid ? CebStatus.Valid : CebStatus.Erreur;
            return Status;
        }

        /// <summary>
        ///     Valid the search value
        /// </summary>
        /// <returns></returns>
        public bool SearchValid => _search > 99 && _search < 1000;

        public bool PlaquesValid => Plaques.All(p => p.IsValid &&
                                                     Plaques.Count(q => q.Value == p.Value) <=
                                                     CebPlaque.ListePlaques.Count(n => n == p.Value));

        /// <summary>
        ///     Select the value and the plaque's list
        /// </summary>
        public void Random()
        {
            // EnableEvents(false);
            var rnd = new Random();
            _search = rnd.Next(100, 1000);

            IList<int> liste = new List<int>(CebPlaque.ListePlaques);
            foreach (var plaque in Plaques)
            {
                var n = rnd.Next(0, liste.Count);
                plaque.Value = liste[n];
                liste.RemoveAt(n);
            }
        }

        public async Task RandomAsync()
        {
            await Task.Run(() => Random());
        }

        public void Clear()
        {
            Solutions.Clear();
            Diff = int.MaxValue;
            CebFind.Reset();
            Valid();
        }

        public async Task ClearAsync()
        {
            await Task.Run(() => Clear());
        }

        /// <summary>
        /// </summary>
        /// <param name="search">Valeur à rechercher</param>
        /// <param name="plq">Liste des plaques</param>
        /// <returns></returns>
        public CebStatus Resolve(int search, params int[] plq)
        {
            if (plq.Length != 6)
                throw new ArgumentException("Nombre de plaques incorrecte");
            _search = search;
            for (var i = 0; i < 6; i++)
                Plaques[i].Value = plq[i];
            return Resolve();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public CebStatus Resolve()
        {
            void Resolve(List<CebBase> liste)
            {
                void AddSolution(CebBase sol)
                {
                    var diff = Abs(_search - sol.Value);

                    if (diff > Diff || Solutions.Contains(sol))
                        return;

                    if (diff < Diff)
                    {
                        Diff = diff;
                        Solutions.Clear();
                        CebFind.Reset();
                    }
                    CebFind.Add(sol.Value);
                    Solutions.Add(sol);
                }

                liste.Sort((p, q) => q.Value.CompareTo(p.Value));

                for (var i = 0; i < liste.Count; i++)
                {
                    AddSolution(liste[i]);
                    for (var j = i + 1; j < liste.Count; j++)
                        foreach (var oper in
                            // ReSharper disable AccessToModifiedClosure
                            CebOperation.ListeOperations.Select(operation => new CebOperation(liste[i], operation, liste[j]))
                                .Where(oper => oper.IsValid))
                            Resolve(
                                liste.Where((t, k) => k != i && k != j).Concat(new CebBase[] { oper }).ToList());
                }
            }

            Clear();
            if (Status != CebStatus.Valid) return Status;
            Resolve(Plaques.Cast<CebBase>().ToList());
            if (Sorted)
                Solutions.Sort((p, q) => p.Rank.CompareTo(q.Rank));
            Status = Diff == 0 ? CebStatus.CompteEstBon : CebStatus.CompteApproche;
            return Status;
        }

        public async Task<CebStatus> ResolveAsync()
        {
            return await Task.Run(() => Resolve());
        }

        public async Task<CebStatus> ResolveAsync(int search, params int[] plq)
        {
            return await Task.Run(() => Resolve(search, plq));
        }
    }
}