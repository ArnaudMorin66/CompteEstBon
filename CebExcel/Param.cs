using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio.Tools.Applications.Runtime;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;

namespace CebExcel
{
    public partial class Param
    {
        private void Feuil2_Startup(object sender, System.EventArgs e)
        {
        }

        private void Feuil2_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region Code généré par le Concepteur VSTO

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(Feuil2_Startup);
            this.Shutdown += new System.EventHandler(Feuil2_Shutdown);
        }

        #endregion

    }
}
