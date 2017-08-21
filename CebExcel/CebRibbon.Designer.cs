namespace CebExcel
{
    partial class CebRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public CebRibbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabCeb = this.Factory.CreateRibbonTab();
            this.group1 = this.Factory.CreateRibbonGroup();
            this.btnHasard = this.Factory.CreateRibbonButton();
            this.btnResoudre = this.Factory.CreateRibbonButton();
            this.tabCeb.SuspendLayout();
            this.group1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabCeb
            // 
            this.tabCeb.Groups.Add(this.group1);
            this.tabCeb.Label = "Ceb";
            this.tabCeb.Name = "tabCeb";
            this.tabCeb.Position = this.Factory.RibbonPosition.AfterOfficeId("TabHome");
            // 
            // group1
            // 
            this.group1.Items.Add(this.btnHasard);
            this.group1.Items.Add(this.btnResoudre);
            this.group1.Label = "Commandes";
            this.group1.Name = "group1";
            // 
            // btnHasard
            // 
            this.btnHasard.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnHasard.Image = global::CebExcel.Properties.Resources.Hasard;
            this.btnHasard.Label = "Hasard";
            this.btnHasard.Name = "btnHasard";
            this.btnHasard.ShowImage = true;
            this.btnHasard.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnHasard_Click);
            // 
            // btnResoudre
            // 
            this.btnResoudre.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnResoudre.Image = global::CebExcel.Properties.Resources.ceb;
            this.btnResoudre.Label = "Résoudre";
            this.btnResoudre.Name = "btnResoudre";
            this.btnResoudre.ShowImage = true;
            this.btnResoudre.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnResoudre_Click);
            // 
            // CebRibbon
            // 
            this.Name = "CebRibbon";
            this.RibbonType = "Microsoft.Excel.Workbook";
            this.Tabs.Add(this.tabCeb);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.CebRibbon_Load);
            this.tabCeb.ResumeLayout(false);
            this.tabCeb.PerformLayout();
            this.group1.ResumeLayout(false);
            this.group1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tabCeb;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnHasard;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnResoudre;
    }

    partial class ThisRibbonCollection
    {
        internal CebRibbon CebRibbon
        {
            get { return this.GetRibbon<CebRibbon>(); }
        }
    }
}
