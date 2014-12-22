using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DiffMatchPatch;

namespace WpfCopyApplication
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MergeWindow : Window
    {
        public MergeWindow(Conflict element)
        {
            string contentSource = File.ReadAllText(element.SourcePath);
            string contentTarget = File.ReadAllText(element.TargetPath);
            //MergeConflicts conflicts = new MergeConflicts();
            diff_match_patch diffMatchPatch = new diff_match_patch();
            //        список объектов класса Diff (список различий и соответствий)
            List<Diff> diffMain = diffMatchPatch.diff_main(contentSource, contentTarget);

            InitializeComponent();
        }

//          possible methods for color substring
//        private void AppendRtfText(string Text, Brush Color)
//        {
//            TextRange range = new TextRange(Source.Document.ContentEnd, Source.Document.ContentEnd);
//            range.Text = Text;
//            range.ApplyPropertyValue(TextElement.ForegroundProperty, Color);
//        }
//        private FormattedText FormatText(string str)
//        {
//            return new FormattedText(str, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 14, Brushes.Red);
//
//        }
    }
}
