using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation.Collections;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;



namespace ExportCeb {
    public static class ExportData {

        public static string ToExcel(this ValueSet valueSet) {
            string result;
            try {
                var Plaques = valueSet["Plaques"] as IList<int>;
                var Search = valueSet["Search"] as int?;
                var ResultCeb = valueSet["Result"] as string;
                var Status = valueSet["Status"] as int?;
                var Solutions = valueSet["Solutions"] as IList<string>;
                
                InteropClass.GetApplication(out Excel.Application _excel);

                InteropClass.SetFocusWindow(_excel.Hwnd);
                string styletb = Status == 3 ? "TableStyleMedium7" : "TableStyleMedium5";

                var wb = _excel.Workbooks.Add();
                var ws = wb.Worksheets[1];
                wb.Windows[1].DisplayGridlines = false;
                for (var i = 1; i <= 6; i++) {
                    ws.Cells[1, i + 1].Value2 = $"Plaque {i}";
                    // ReSharper disable once PossibleNullReferenceException
                    ws.Cells[2, i + 1].Value2 = Plaques[i - 1];
                }
                ws.Cells[1, 8].Value2 = "Recherche";
                ws.Cells[2, 8].Value2 = Search;
                Excel.ListObject ls = ws.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, ws.Range["B1"].CurrentRegion, null, Excel.XlYesNoGuess.xlYes);
                ls.TableStyle = styletb;
                ws.Range["C4"].Value = ResultCeb;

                for (var c = 1; c < 6; c++) {
                    ws.Cells[6, c + 2].Value = $"Opération {c}";
                }
                var l = 7;

                foreach (var s in Solutions.Select(s => s.Split(','))) {
                    ws.Range[ws.Cells[l, 3], ws.Cells[l, s.Length + 2]].Value2 = s;
                    l++;
                }
                ls = ws.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, ws.Range["C6"].CurrentRegion, null, Excel.XlYesNoGuess.xlYes);

                ls.TableStyle = styletb;
                if (Status == 3) {
                    ws.Range["C4"].Interior.ThemeColor = Excel.XlThemeColor.xlThemeColorAccent6;
                    ws.Range["C4"].Font.ThemeColor = Excel.XlThemeColor.xlThemeColorDark1;
                    ws.Range["C4"].Font.Bold = true;
                } else {
                    ws.Range["C4"].Interior.ThemeColor = Excel.XlThemeColor.xlThemeColorAccent4;
                    ws.Range["C4"].Font.ThemeColor = Excel.XlThemeColor.xlThemeColorLight1;
                    ws.Range["C4"].Font.Bold = true;
                }
                ws.Range["C4:G4"].MergeCells = true;
                ws.Range["C4:G4"].HorizontalAlignment = Excel.Constants.xlCenter;
                wb.Activate();
                result = "SUCCESS";
            } catch (Exception exc) {
                result = exc.Message;
            }
            return result;
        }
        public static string ToWord(this ValueSet valueSet) {
            var result = "";
            try {
                var Plaques = valueSet["Plaques"] as IList<int>;
                var Search = valueSet["Search"] as int?;
                var ResultCeb = valueSet["Result"] as string;
                var Status = valueSet["Status"] as int?;
                var Solutions = valueSet["Solutions"] as IList<string>;
                InteropClass.GetApplication(out Word.Application _word);
                var nomstyle = Status == 3 ? "Tableau Grille 4 - Accentuation 6" : "Tableau Liste 3 - Accentuation 2";

                _word.Visible = true;
                Word.Document doc = _word.Documents.Add("Normal");

                doc.UpdateStyles();
                doc.Content.set_Style("Sans interligne");
                var tbStyle = doc.Styles
                        .Cast<Word.Style>().First(p => p.Type == Word.WdStyleType.wdStyleTypeTable &&
                                                       p.NameLocal == nomstyle);

                var tb = doc.Tables.Add(doc.Range(), 2, 7);
                for (var i = 1; i <= 6; i++) {
                    tb.Cell(1, i).Range.Text = $"Plaque {i}";
                    tb.Cell(2, i).Range.Text = $"{Plaques[i - 1]}";
                }

                tb.Cell(1, 7).Range.Text = "Recherche";
                tb.Cell(2, 7).Range.Text = $"{Search}";
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
                tb = doc.Tables.Add(doc.Range().Characters.Last, 1, 5);
                for (var c = 1; c <= 5; c++) {
                    tb.Cell(1, c).Range.Text = $"Opération {c}";
                }

                foreach (var s in Solutions.Select(s => s.Split(','))) {
                    var row = tb.Rows.Add();
                    for (var j = 0; j < s.Length; j++) {
                        row.Cells[j + 1].Range.Text = s[j];
                    }
                }

                tb.set_Style(tbStyle);
                tb.ApplyStyleHeadingRows = true;
                tb.ApplyStyleFirstColumn = false;
                tb.ApplyStyleColumnBands = false;
                tb.ApplyStyleRowBands = true;
                tb.Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                tb.Rows[1].Range.Rows.HeadingFormat = (int)Word.WdConstants.wdToggle;
                var para = doc.Paragraphs[nopara];

                para.Range.Font.Bold = 1; // (int)Word.WdConstants.wdToggle;

                if (Status == 3) {
                    para.Range.Text = "Le Compte est bon";
                    para.Range.Font.Color = Word.WdColor.wdColorWhite;
                    para.Shading.BackgroundPatternColor = Word.WdColor.wdColorGreen;
                } else {
                    para.Range.Text = ResultCeb;
                    para.Range.Font.Color = Word.WdColor.wdColorWhite;
                    para.Shading.BackgroundPatternColor = Word.WdColor.wdColorOrange;
                }

                para.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                InteropClass.SetFocusWindow(doc.Windows[1].Hwnd);
                result = "SUCCESS";
            } catch (Exception exc) {
                result = exc.Message;
            }
            return result;
        }

    }
}
