using System;
using System.Linq;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;

namespace CompteEstBon.ViewModel {
    public static class ExportData {
        public static void ToWord(this CebTirage tirage) {
            try {
                NativeMethods.GetApplication(out Word.Application word);
                var nomstyle = tirage.Status == CebStatus.CompteEstBon
                    ? "Tableau Grille 4 - Accentuation 6"
                    : "Tableau Liste 3 - Accentuation 2";
                var doc = word.Documents.Add("Normal");
                doc.UpdateStyles();
                var st = doc.Styles.get_Item("Normal");
                st.ParagraphFormat.SpaceAfter = 0;
                var tbStyle = doc.Styles
                    .Cast<Word.Style>().First(p => p.Type == Word.WdStyleType.wdStyleTypeTable &&
                                              p.NameLocal == nomstyle);

                var tb = doc.Tables.Add(doc.Content, 2, 7);
                for (var i = 1; i <= 6; i++) {
                    tb.Cell(1, i).Range.Text = $"Plaque {i}";
                    tb.Cell(2, i).Range.Text = $"{tirage.Plaques[i - 1]}";
                }

                tb.Cell(1, 7).Range.Text = "Recherche";
                tb.Cell(2, 7).Range.Text = $"{tirage.Search}";
                tb.set_Style(tbStyle);
                doc.Bookmarks.Add("Data", tb.Range);

                tb.Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                tb.set_Style(tbStyle);
                tb.ApplyStyleHeadingRows = true;
                tb.ApplyStyleFirstColumn = false;
                tb.ApplyStyleColumnBands = false;
                tb.ApplyStyleRowBands = true;
                doc.Content.InsertParagraphAfter();
                var nopara = doc.Paragraphs.Count;
                doc.Content.InsertParagraphAfter();
                doc.Content.InsertParagraphAfter();
                doc.Content.InsertParagraphAfter();
                tb = doc.Tables.Add(doc.Content.Paragraphs.Last.Range, 1, 5);

                tb.set_Style(tbStyle);
                tb.ApplyStyleHeadingRows = true;
                tb.ApplyStyleFirstColumn = false;
                tb.ApplyStyleColumnBands = false;
                tb.ApplyStyleRowBands = true;
                for (var c = 1; c < 6; c++) tb.Rows[1].Cells[c].Range.Text = $"Opération {c}";

                foreach (var s in tirage.ArrayOfSolutions) {
                    var row = tb.Rows.Add();
                    for (var j = 0; j < s.Length; j++) row.Cells[j + 1].Range.Text = s[j];
                }

                tb.Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                tb.Rows[1].Range.Rows.HeadingFormat = (int)Word.WdConstants.wdToggle;
                doc.Bookmarks.Add("ListeSolutions", tb.Range);

                var para = doc.Paragraphs[nopara];

                para.Range.Font.Bold = 1; // (int)Word.WdConstants.wdToggle;

                if (tirage.Status == CebStatus.CompteEstBon) {
                    para.Range.Text = "Le Compte est bon";
                    para.Range.Font.Color = Word.WdColor.wdColorWhite;
                    para.Shading.BackgroundPatternColor = Word.WdColor.wdColorGreen;
                }
                else {
                    para.Range.Text = $"Compte approché: {tirage.Found}, écart: {tirage.Diff}";
                    para.Range.Font.Color = Word.WdColor.wdColorWhite;
                    para.Shading.BackgroundPatternColor = Word.WdColor.wdColorOrange;
                }

                para.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                doc.Bookmarks.Add("Résultat", para.Range);
                NativeMethods.SetFocusWindow(doc.Windows[1].Hwnd);
            }
            catch (Exception e) {
                MessageBox.Show(e.Message, e.Source);
            }
        }

        public static void ToExcel(this CebTirage tirage) {
            NativeMethods.GetApplication(out Excel.Application excel);
            excel.Visible = true;
            var workBook = excel.Workbooks.Add();
            Excel.Worksheet ws = workBook.Worksheets[1];
            workBook.Windows[1].DisplayGridlines = false;
            for (var i = 1; i <= 6; i++) {
                ws.Cells[1, i + 1].Value = $"Plaque {i}";
                ws.Cells[2, i + 1].Value = tirage.Plaques[i - 1];
            }

            ws.Cells[1, 8].Value = "Recherche";
            ws.Cells[2, 8].Value = tirage.Search;
            var styletb = tirage.Status == CebStatus.CompteEstBon ? "TableStyleMedium7" : "TableStyleMedium3";
            var ls = ws.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, ws.Range["B1"].CurrentRegion, null,
                Excel.XlYesNoGuess.xlYes);
            ls.TableStyle = styletb;
            ls.Range.HorizontalAlignment = Excel.Constants.xlCenter;
            ls.Name = "Data";

            for (var c = 1; c < 6; c++) ws.Cells[6, c + 2].Value = $"Opération {c}";
            var lg = 7;
            foreach (var s in tirage.ArrayOfSolutions) {
                Excel.Range rw = ws.Rows[lg++];
                ws.Range[rw.Cells[3], rw.Cells[s.Length + 2]].Value = s;
            }

            ls = ws.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, ws.Range["C6"].CurrentRegion, null,
                Excel.XlYesNoGuess.xlYes);
            ls.TableStyle = styletb;
            ls.Name = "ListeSolutions";
            ls.Range.HorizontalAlignment = Excel.Constants.xlCenter;
            var rg = ws.Range["C4"];
            rg.Name = "Resultat";
            if (tirage.Status == CebStatus.CompteEstBon) {
                rg.Value = "Compte est Bon";
                rg.Interior.ThemeColor = Excel.XlThemeColor.xlThemeColorAccent6;
                rg.Font.ThemeColor = Excel.XlThemeColor.xlThemeColorDark1;
                rg.Font.Bold = true;
            }
            else {
                rg.Value = $"Compte approché: {tirage.Found}, écart: {tirage.Diff}";
                rg.Interior.ThemeColor = Excel.XlThemeColor.xlThemeColorAccent4;
                rg.Font.ThemeColor = Excel.XlThemeColor.xlThemeColorLight1;
                rg.Font.Bold = true;
            }
            rg.HorizontalAlignment = Excel.Constants.xlCenter;
            ws.Range["C4:G4"].MergeCells = true;
            workBook.Activate();
            NativeMethods.SetFocusWindow(excel.Hwnd);
        }
    }
}