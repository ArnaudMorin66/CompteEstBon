using System;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CompteEstBon;
using Microsoft.VisualStudio.Tools.Applications.Runtime;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;

namespace CebExcel
{
    public partial class Ceb
    {
        public CebTirage Tirage { get; } = new CebTirage();

        private void Feuil1_Startup(object sender, System.EventArgs e)
        {
            InitData();
        }

        private void InitData()
        {
            Plaque_1.Value = Tirage.Plaques[0].Value;
            Plaque_2.Value = Tirage.Plaques[1].Value;
            Plaque_3.Value = Tirage.Plaques[2].Value;
            Plaque_4.Value = Tirage.Plaques[3].Value;
            Plaque_5.Value = Tirage.Plaques[4].Value;
            Plaque_6.Value = Tirage.Plaques[5].Value;
            Recherche.Value = Tirage.Search;
            Clear();
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
            Durée.Value = null;
            dsSolutions.Clear();
            Protect(drawingObjects: true, contents: true, scenarios: true);
        }

        public void Hasard()
        {
            Tirage.Random();
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
            this.Plaque_1.Change +=
                new Microsoft.Office.Interop.Excel.DocEvents_ChangeEventHandler(this.plaque_changed);
            this.Plaque_2.Change +=
                new Microsoft.Office.Interop.Excel.DocEvents_ChangeEventHandler(this.plaque_changed);
            this.Plaque_3.Change +=
                new Microsoft.Office.Interop.Excel.DocEvents_ChangeEventHandler(this.plaque_changed);
            this.Plaque_4.Change +=
                new Microsoft.Office.Interop.Excel.DocEvents_ChangeEventHandler(this.plaque_changed);
            this.Plaque_5.Change +=
                new Microsoft.Office.Interop.Excel.DocEvents_ChangeEventHandler(this.plaque_changed);
            this.Plaque_6.Change +=
                new Microsoft.Office.Interop.Excel.DocEvents_ChangeEventHandler(this.plaque_changed);
            this.Recherche.Change +=
                new Microsoft.Office.Interop.Excel.DocEvents_ChangeEventHandler(this.Recherche_Change);
            this.Startup += new System.EventHandler(this.Feuil1_Startup);
            this.Shutdown += new System.EventHandler(this.Feuil1_Shutdown);
        }

        #endregion

        private void plaque_changed(Excel.Range Target)
        {
            String nm = Target.Name.NameLocal;
            nm = nm.Substring(nm.Length - 1);
            int ix = Convert.ToInt32(nm) - 1;

            Tirage.Plaques[ix].Value = int.TryParse(Target.Text, out int val) ? val : 0;
            Clear();
        }

        private void Recherche_Change(Excel.Range Target)
        {
            Tirage.Search = int.TryParse(Target.Text, out int val) ? val : 0;
            Clear();
        }

        private void btnHasard_Click(object sender, EventArgs e)
        {
            Hasard();
        }

        public void Resoudre()
        {
            if (Tirage.Status !=  CebStatus.Valid)
            {
                return;
            }
            Unprotect();
            Application.EnableEvents = false;
            Application.ScreenUpdating = false;
            var time = DateTime.Now;
            Tirage.Resolve();
            dsSolutions.Clear();
            if (Tirage.Status == CebStatus.CompteEstBon)
            {
                Resultat.Value = "Compte est bon";
                Resultat.Font.Color = Color.Yellow;
                Resultat.Interior.Color = Color.Blue;
            }
            else
            {
                Resultat.Value = $"Compte approché: {Tirage.Found} - Écart: {Tirage.Diff}";
                Resultat.Interior.Color = Color.Green;
                Resultat.Font.Color = Color.White;
            }
            Tirage.Solutions.ForEach(solution =>
            {
                var rw = Solutions.NewRow();
                var ix = 0;
                foreach (var operation in solution.Operations)
                {
                    rw[ix++] = operation;
                }
                Solutions.Rows.Add(rw);
            });
            Solutions.AcceptChanges();
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