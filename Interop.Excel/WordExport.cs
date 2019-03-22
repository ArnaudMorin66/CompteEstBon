using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Word = Microsoft.Office.Interop.Word;

namespace ExportCeb
{
    public static class WordExport
    {
        public static string ToWord(this ValueSet valueSet)
        {
            var result = "";
            try
            {
                var Plaques = valueSet["Plaques"] as IList<int>;
                var Search = valueSet["Search"] as int?;
                var Result = valueSet["Result"] as string;
                var Status = valueSet["Status"] as int?;
                var Solutions = valueSet["Solutions"] as IList<string>;
                InteropClass.GetApplication(out Word.Application _word );
                var nomstyle = "";
                if (Status == 3)
                {
                    nomstyle = "Tableau Grille 4 - Accentuation 6";
                }
                else
                {
                    nomstyle = "Tableau Liste 3 - Accentuation 2";
                }

                _word.Visible = true;
                Word.Document doc = _word.Documents.Add("Normal");

                doc.UpdateStyles();
                var tbStyle = doc.Styles.Cast<Word.Style>()
                        .Where(p => p.Type == Word.WdStyleType.wdStyleTypeTable &&
                          p.NameLocal == nomstyle).First();

                var tb = doc.Tables.Add(doc.Range(), 2, 7);
                for (var i = 1; i <= 6; i++)
                {
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
                doc.Range().InsertParagraphAfter();

                doc.Range().InsertAfter(Result);
                doc.Range().InsertParagraphAfter();
                var para = doc.Paragraphs.Last.Previous();
                para.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                if (Status == 3)
                {
                    para.Range.Font.Color = Word.WdColor.wdColorWhite;
                    para.Shading.BackgroundPatternColor = Word.WdColor.wdColorGreen;
                }
                else
                {
                    para.Range.Font.Color = Word.WdColor.wdColorBlack;
                    para.Shading.BackgroundPatternColor = Word.WdColor.wdColorOrange;
                }
                para.Range.Font.Bold = (int)Word.WdConstants.wdToggle;
                doc.Range().InsertParagraphAfter();
                doc.Range().Characters.Last.Font.Bold = (int)Word.WdConstants.wdToggle;

                tb = doc.Tables.Add(doc.Range().Characters.Last, 1, 5);
                for (var c = 1; c <= 5; c++)
                {
                    tb.Cell(1, c).Range.Text = $"Opération {c}";
                }

                foreach (var s in Solutions.Select(s => s.Split(',')))
                {
                    Word.Row row = tb.Rows.Add();
                    for (var j = 0; j < s.Length; j++)
                    {
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
                InteropClass.SetFocusWindow(doc.Windows[1].Hwnd);
                result = "SUCCESS";
            }
            catch (Exception exc)
            {
                result = exc.Message;
            }
            return result;
        }
    }
}
