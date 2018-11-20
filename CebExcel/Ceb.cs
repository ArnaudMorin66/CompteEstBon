using CompteEstBon;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace CebExcel
{
    public partial class Ceb
    {
        private bool blocked;
        public CebTirage Tirage { get; } = new CebTirage();

        private void Feuil1_Startup(object sender, System.EventArgs e)
        {
            InitData();
        }

        private void InitData()
        {
            blocked = true;
            plaques.Value = Tirage.Plaques.Select(p=>p.Value).ToArray();
            Recherche.Value = Tirage.Search;
           
            Clear();
            blocked = false;
        }

        private void Feuil1_Shutdown(object sender, System.EventArgs e)
        {
        }

        private void Clear()
        {
            Unprotect();
            if (Tirage.Status == CebStatus.Erreur)
            {
                Resultat.Value = "Tirage invalide";
                Resultat.Font.Color = Color.White;
                Resultat.Interior.Color = Color.Red;
            }
            else
            {
                Resultat.Value = null;
                Resultat.Font.Color = Color.Black;
                Resultat.Interior.Color = Color.White;
            }
            NbSolutions.Value = null;
            tbSolutions.DataSource = null;
            Durée.Value = null;
            Protect(drawingObjects: true, contents: true, scenarios: true);
        }

        public async void Hasard()
        {
            await Tirage.RandomAsync();
            InitData();
        }

        #region Code généré par le Concepteur VSTO

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InternalStartup()
        {
            this.btnHasard.Click += new System.EventHandler(this.btnHasard_Click);
            this.btnResoudre.Click += new System.EventHandler(this.btnResoudre_Click);
            this.Recherche.Change += new Microsoft.Office.Interop.Excel.DocEvents_ChangeEventHandler(this.Recherche_Change);
            this.plaques.Change += new Microsoft.Office.Interop.Excel.DocEvents_ChangeEventHandler(this.plaque_changed);
            this.Startup += new System.EventHandler(this.Feuil1_Startup);
            this.Shutdown += new System.EventHandler(this.Feuil1_Shutdown);

        }

        #endregion Code généré par le Concepteur VSTO

        private void plaque_changed(Excel.Range Target)
        {
            if (blocked) return; 
            Tirage.Plaques[Target.Column - plaques.Column].Value = int.TryParse(Target.Text, out int val) ? val : 0;
            Clear();
        }

        private void Recherche_Change(Excel.Range Target)
        {
            if (blocked) return;
            Tirage.Search = int.TryParse(Target.Text, out int val) ? val : 0;
            Clear();
        }

        private void btnHasard_Click(object sender, EventArgs e)
        {
            Hasard();
        }

        public async void Resoudre() {
            if (Tirage.Status != CebStatus.Valid) {
                return;
            }
            Unprotect();
            Application.EnableEvents = false;
            Application.ScreenUpdating = false;
            var time = DateTime.Now;
            tbSolutions.DataBodyRange?.Delete();
            await Tirage.ResolveAsync();

            if (Tirage.Status == CebStatus.CompteEstBon) {
                Resultat.Value = "Compte est bon";
                Resultat.Font.Color = Color.Yellow;
                Resultat.Interior.Color = Color.Blue;
            } else {
                Resultat.Value = $"Compte approché: {Tirage.Found} - Écart: {Tirage.Diff}";
                Resultat.Interior.Color = Color.Green;
                Resultat.Font.Color = Color.White;
            }

            tbSolutions.DataSource = Tirage.ToCebOperationsDetail().ToArray();
            NbSolutions.Value = Tirage.Solutions.Count;
            Durée.Value = (DateTime.Now - time).Milliseconds / 1000.0;
            Application.EnableEvents = true;
            Application.ScreenUpdating = true;
            Protect(drawingObjects: true, contents: true, scenarios: true);
        }

        private void btnResoudre_Click(object sender, EventArgs e)
        {
            Resoudre();
        }
    }
}