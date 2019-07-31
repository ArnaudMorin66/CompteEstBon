using Microsoft.Office.Tools.Ribbon;

namespace CebExcel {
    public partial class CebRibbon {
        private void CebRibbon_Load(object sender, RibbonUIEventArgs e) {

        }

        private void btnHasard_Click(object sender, RibbonControlEventArgs e) {
            Globals.Ceb.Hasard();
        }

        private void btnResoudre_Click(object sender, RibbonControlEventArgs e) {
            Globals.Ceb.Resoudre();
        }
    }
}
