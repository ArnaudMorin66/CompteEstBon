using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.XlsIO;
using System;
using System.IO;

namespace CompteEstBon {
    //

    public static class SfCebOffice {
        public static void RegisterLicense(string licensefile) {

            if (!string.IsNullOrEmpty(licensefile)) {
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licensefile);
            }
        }



        public static void ExportExcel(this CebTirage tirage, Stream stream) {

            using var engine = new ExcelEngine();

            var application = engine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2016;
            var workbook = application.Workbooks.Create(names: new[] { "Compte Est Bon" });
            var ws = workbook.Worksheets[0];
            var styletb = tirage.Status == CebStatus.CompteEstBon ? TableBuiltInStyles.TableStyleMedium7 : TableBuiltInStyles.TableStyleMedium3;

            for (var i = 0; i < 6; i++) {
                ws.Range[1, i + 1].Value2 = $"Plaque {i + 1}";
                ws.Range[2, i + 1].Value2 = tirage.Plaques[i].Value;
            }

            ws.Range[1, 7].Value2 = "Cherche";
            ws.Range[2, 7].Value2 = tirage.Search;

            ws.ListObjects.Create("TabEntree", ws["A1:G2"])
                        .BuiltInTableStyle = styletb;
            var rg = ws["A5"];
            string res;
            if (tirage.Status == CebStatus.CompteEstBon) {
                res = "Compte est bon";
                rg.CellStyle.Font.Color = ExcelKnownColors.White;
                rg.CellStyle.ColorIndex = ExcelKnownColors.Green;
            }
            else {
                res = "Compte approché";
                rg.CellStyle.Font.Color = ExcelKnownColors.White;
                rg.CellStyle.ColorIndex = ExcelKnownColors.Orange;
            }
            res += $": {tirage.Found}, Nombre de solutions: {tirage.Count}, Duree: {tirage.watch.Elapsed.TotalSeconds:N3} ";

            rg.Value2 = res;
            rg.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ws.Range["A5:F5"].Merge();


            var l = 7;
            for (var i = 1; i < 6; i++) {
                ws.Range[l, i].Value2 = $"Operation {i}";

            }

            foreach (var s in tirage.Solutions) {
                ws.ImportArray(s.Operations.ToArray(), ++l, 1, false);
            }
            ws[$"A7:E{l}"].AutofitColumns();
            ws.ListObjects.Create("TabSolutions", ws[$"A7:E{l}"]).BuiltInTableStyle = styletb;

            workbook.SaveAs(stream);
        }


        public static void ExportWord(this CebTirage tirage, Stream stream) {
            var wd = new WordDocument();
            var sect = wd.AddSection() as WSection;
            var dotm = Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Roaming\Microsoft\Templates\Normal.dotm";
            if (File.Exists(dotm)) {
                wd.AttachedTemplate.Path = dotm;
                wd.UpdateStylesOnOpen = true;

            }

            // ReSharper disable once PossibleNullReferenceException
            var tbl = sect.AddTable();


            var sty = tirage.Status == CebStatus.CompteEstBon ? BuiltinTableStyle.MediumGrid1Accent3 : BuiltinTableStyle.MediumGrid1Accent6;
            tbl.ResetCells(2, 7);
            IWParagraph pg;
            for (var i = 0; i < 6; i++) {
                pg = tbl[0, i].AddParagraph();
                pg.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

                pg.AppendText($"Plaque {i + 1}");
                pg = tbl[1, i].AddParagraph();
                pg.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
                pg.AppendText($"{tirage.Plaques[i].Value}");
            }

            pg = tbl[0, 6].AddParagraph();
            pg.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
            pg.AppendText("Chercher");
            pg = tbl[1, 6].AddParagraph();
            pg.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
            pg.AppendText($"{tirage.Search}");
            tbl.ApplyStyle(sty);
            tbl.ApplyStyleForHeaderRow = true;
            tbl.ApplyStyleForFirstColumn = false;
            sect.AddParagraph();
            sect.AddParagraph();
            Syncfusion.Drawing.Color bcolor;
            Syncfusion.Drawing.Color tcolor;

            string res;
            if (tirage.Status == CebStatus.CompteEstBon) {
                res = "Compte est bon";
                bcolor = Syncfusion.Drawing.Color.Green;
                tcolor = Syncfusion.Drawing.Color.White;

            }
            else {
                res = "Compte approché";
                bcolor = Syncfusion.Drawing.Color.Orange;
                tcolor = Syncfusion.Drawing.Color.Black;

            }
            res += $": {tirage.Found}, Nombre de solutions: {tirage.Count}, Duree: {tirage.watch.Elapsed.TotalSeconds:N3} ";
            pg = sect.AddParagraph();
            var tx = pg.AppendText(res);
            tx.CharacterFormat.TextColor = tcolor;
            pg.ParagraphFormat.BackColor = bcolor;
            pg.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            sect.AddParagraph();
            tbl = sect.AddTable();
            tbl.ResetCells(1, 5);
            for (var i = 0; i < 5; i++) {
                pg = tbl[0, i].AddParagraph();
                pg.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
                pg.AppendText($"Opération {i + 1}");
            }

            foreach (var s in tirage.Solutions) {
                var rw = tbl.AddRow();
                foreach (var (op, ix) in s.Operations.WithIndex()) {
                    pg = rw.Cells[ix].AddParagraph();
                    pg.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
                    pg.AppendText(op);
                }
            }
            tbl.ApplyStyle(sty);
            tbl.ApplyStyleForHeaderRow = true;
            tbl.ApplyStyleForFirstColumn = false;
            tbl.Rows[0].IsHeader = true;
            wd.Save(stream, FormatType.Docx);

        }
    }

}
