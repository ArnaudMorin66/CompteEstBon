using arnaud.morin.outils;

using static System.Math;


namespace CompteEstBon;

public class CebTirageAsync : CebTirage {
    public override async ValueTask<CebStatus> SolveAsync() {
        _solutions.Clear();
        if (Status == CebStatus.Invalide) return Status;
        Status = CebStatus.EnCours;
        Ecart = int.MaxValue;
        await _solveAsync();
        Status = Ecart == 0 ? CebStatus.CompteEstBon : CebStatus.CompteApproche;
        _solutions.Sort((p, q) => p.Rank.CompareTo(q.Rank));
        OnPropertyChanged(nameof(Solve));
        return Status;
    }


    private async Task _solveAsync() {
        void InsertSolution(CebBase sol) {
            var ecart = Abs(Search - sol.Value);
            if (ecart > Ecart) return;
            if (ecart < Ecart) {
                Ecart = ecart;
                _solutions.Clear();
            } else if (_solutions.Contains(sol)) {
                return;
            }

            _solutions.Add(sol);
        }

        var stack = new Stack<IEnumerable<CebBase>>();
        stack.Push(Plaques);

        while (stack.Count > 0) {
            var currentList = stack.Pop();

            foreach (var (g, i) in currentList.Indexed()) {
                InsertSolution(g);

                foreach (var (d, j) in currentList.Indexed().Where((_, j) => j > i)) {
                    var validOperations = CebOperation.ListeOperations
                        .Select(op => new CebOperation(g, op, d))
                        .Where(o => o.Value != 0);

                    foreach (var oper in validOperations)
                        stack.Push(new[] { oper }.Concat(currentList.Where((_, k) => k != i && k != j)));
                }
                await Task.Yield();
            }

            // Ajouter un point d'attente pour permettre à l'interface utilisateur de rester réactive
            await Task.Yield();
        }
    }
}