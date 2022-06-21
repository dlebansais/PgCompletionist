namespace CustomControls
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;

    public class HtmlTextBlock : TextBlock
    {
        #region Constants and types
        private struct TitleFormat
        {
            public double FontSize { get; set; }
            public string NewLines { get; set; }
        }

        private static readonly TitleFormat Level1 = new TitleFormat() { FontSize = 32, NewLines = "\n" };
        private static readonly TitleFormat Level2 = new TitleFormat() { FontSize = 24, NewLines = "\n" };
        private static readonly TitleFormat Normal = new TitleFormat() { FontSize = 16, NewLines = string.Empty };

        private struct Format
        {
            public TitleFormat Title { get; set; }
            public bool IsBold { get; set; }
            public bool IsItalic { get; set; }
            public bool IsUnderline { get; set; }
            public string Indentation { get; set; }
        }
        #endregion

        #region Properties
        public string HtmlFormattedText
        {
            get { return (string)GetValue(HtmlFormattedTextProperty); }
            set { SetValue(HtmlFormattedTextProperty, value); }
        }

        public static readonly DependencyProperty HtmlFormattedTextProperty = DependencyProperty.Register("HtmlFormattedText", typeof(string), typeof(HtmlTextBlock), new UIPropertyMetadata(null, OnHtmlFormattedTextChanged));

        private static void OnHtmlFormattedTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            HtmlTextBlock Ctrl = (HtmlTextBlock)sender;
            Ctrl.OnHtmlFormattedTextChanged(args.NewValue as string);
        }

        private void OnHtmlFormattedTextChanged(string? newValue)
        {
            Inlines.Clear();

            if (newValue is not null)
            {
                int StartIndex = 0;
                bool HasNewLine = true;
                Span FormattedText = ParseText(newValue, ref StartIndex, string.Empty, new Format() { Title = Normal, Indentation = string.Empty }, ref HasNewLine);

                Inlines.Add(FormattedText);
            }
        }
        #endregion

        #region Html parsing
        private static Span ParseText(string text, ref int startIndex, string endTag, Format format, ref bool hasNewLine)
        {
            Span Result = new Span();
            int searchIndex = startIndex;

            while (searchIndex < text.Length)
            {
                int Index = text.IndexOf('<', searchIndex);
                if (Index < 0 || (Index >= 0 && Index + 1 < text.Length && text[Index + 1] == ' '))
                {
                    AddRun(Result.Inlines, text, startIndex, text.Length, format, ref hasNewLine);

                    startIndex = text.Length;
                    searchIndex = startIndex;
                }
                else if (endTag.Length > 0 && Index + endTag.Length <= text.Length && text.Substring(Index, endTag.Length) == endTag)
                {
                    if (startIndex < Index)
                        AddRun(Result.Inlines, text, startIndex, Index, format, ref hasNewLine);

                    startIndex = Index + endTag.Length;
                    return Result;
                }
                else if (Index + 4 <= text.Length && text.Substring(Index, 4) == "<br>")
                {
                    searchIndex = Index + 1;
                }
                else if (Index + 4 <= text.Length && text.Substring(Index, 4) == "<hr>")
                {
                    if (startIndex < Index)
                        AddRun(Result.Inlines, text, startIndex, Index, format, ref hasNewLine);

                    Run Separator = new Run("~~~~");
                    Separator.FontSize = format.Title.FontSize;
                    Result.Inlines.Add(Separator);
                    Result.Inlines.Add(new LineBreak());

                    startIndex = Index + 4;
                    searchIndex = startIndex;
                }
                else if (Index + 5 <= text.Length && text.Substring(Index, 5) == "<span")
                {
                    if (startIndex < Index)
                        AddRun(Result.Inlines, text, startIndex, Index, format, ref hasNewLine);

                    Run Nested = ParseSpan(text, ref Index);
                    Nested.FontSize = format.Title.FontSize;
                    Result.Inlines.Add(Nested);

                    startIndex = Index;
                    searchIndex = startIndex;
                }
                else if (!ParseSpecialText(text, Result, Index, ref startIndex, ref searchIndex, "<h1>", "</h1>", format, new Format() { Title = Level1, IsBold = true, IsItalic = format.IsItalic, IsUnderline = format.IsUnderline, Indentation = format.Indentation }, ref hasNewLine) &&
                         !ParseSpecialText(text, Result, Index, ref startIndex, ref searchIndex, "<h2>", "</h2>", format, new Format() { Title = Level2, IsBold = format.IsBold, IsItalic = format.IsItalic, IsUnderline = true, Indentation = format.Indentation }, ref hasNewLine) &&
                         !ParseSpecialText(text, Result, Index, ref startIndex, ref searchIndex, "<indent=15>", "</indent>", format, new Format() { Title = Normal, IsBold = format.IsBold, IsItalic = format.IsItalic, IsUnderline = format.IsUnderline, Indentation = "               " }, ref hasNewLine) &&
                         !ParseSpecialText(text, Result, Index, ref startIndex, ref searchIndex, "<b>", "</b>", format, new Format() { Title = Normal, IsBold = true, IsItalic = format.IsItalic, IsUnderline = format.IsUnderline, Indentation = format.Indentation }, ref hasNewLine) &&
                         !ParseSpecialText(text, Result, Index, ref startIndex, ref searchIndex, "<i>", "</i>", format, new Format() { Title = Normal, IsBold = format.IsBold, IsItalic = true, IsUnderline = format.IsUnderline, Indentation = format.Indentation }, ref hasNewLine) &&
                         !ParseSpecialText(text, Result, Index, ref startIndex, ref searchIndex, "<em>", "</em>", format, new Format() { Title = Normal, IsBold = format.IsBold, IsItalic = true, IsUnderline = format.IsUnderline, Indentation = format.Indentation }, ref hasNewLine))
                {
                    string TagText = text.Length >= searchIndex + 10 ? text.Substring(searchIndex, 10) : text.Substring(searchIndex, text.Length - searchIndex);
                    Debug.WriteLine($"WARNING Unexpected tag: <{TagText}");

                    searchIndex = Index + 1;
                }
            }

            return Result;
        }

        private static bool ParseSpecialText(string text, Span span, int index, ref int startIndex, ref int searchIndex, string startTag, string endTag, Format oldFormat, Format newFormat, ref bool hasNewLine)
        {
            if (index + startTag.Length <= text.Length && text.Substring(index, startTag.Length) == startTag)
            {
                if (index > startIndex)
                    AddRun(span.Inlines, text, startIndex, index, oldFormat, ref hasNewLine);

                startIndex = index + startTag.Length;
                Span SubText = ParseText(text, ref startIndex, endTag, newFormat, ref hasNewLine);

                if (SubText.Inlines.Count > 0)
                    span.Inlines.Add(SubText);

                searchIndex = startIndex;
                return true;
            }
            else
                return false;
        }

        private static void AddRun(InlineCollection inlines, string text, int startIndex, int endIndex, Format format, ref bool hasNewLine)
        {
            string RunText = text.Substring(startIndex, endIndex - startIndex);

            if (format.Title.NewLines.Length > 0)
            {
                if (!hasNewLine)
                    RunText = "\n" + RunText;

                RunText = RunText + format.Title.NewLines;
                hasNewLine = true;
            }
            else
            {
                if (hasNewLine && RunText.Length > 0 && RunText[0] == '\n')
                    RunText = RunText.Substring(1);

                if (format.Indentation.Length > 0)
                {
                    if (!hasNewLine)
                        RunText = $"\n{format.Indentation}{RunText}";
                    else
                        RunText = $"{format.Indentation}{RunText}";

                    RunText = RunText.Replace("<br> ", $"\n{format.Indentation}");
                }

                int NonSpaceIndex = RunText.Length - 1;
                while (NonSpaceIndex >= 0 && RunText[NonSpaceIndex] == ' ')
                    NonSpaceIndex--;

                hasNewLine = NonSpaceIndex >= 0 && RunText[NonSpaceIndex] == '\n';
            }

            if (RunText.Length > 0)
            {
                Run NewElement = new Run(RunText);

                NewElement.FontSize = format.Title.FontSize;

                if (format.IsBold)
                    NewElement.FontWeight = FontWeights.Bold;

                if (format.IsItalic)
                    NewElement.FontStyle = FontStyles.Italic;

                if (format.IsUnderline)
                    inlines.Add(new Underline(NewElement));
                else
                    inlines.Add(NewElement);
            }
        }

        private static Run ParseSpan(string text, ref int index)
        {
            Run Result = new Run();

            index += 5;

            int StartIndex = index;
            while (StartIndex + 1 < text.Length && text[StartIndex] != '>')
                StartIndex++;

            string SpanClass = text.Substring(index, StartIndex - index).Trim();
            if (SpanClass.Length > 8 && SpanClass.StartsWith("style=\"") && SpanClass.EndsWith("\""))
            {
                string SpanStyle = SpanClass.Substring(7, SpanClass.Length - 8);
                if (SpanStyle.StartsWith("color:"))
                {
                    string SpanColor = SpanStyle.Substring(6).Trim();

                    switch (SpanColor)
                    {
                        case "red":
                            Result.Foreground = Brushes.Red;
                            break;
                        case "lightgreen":
                            Result.Foreground = Brushes.LightGreen;
                            break;
                        case "white":
                            Result.Foreground = Brushes.White;
                            break;
                        default:
                            if (SpanColor.Length == 9 && SpanColor[0] == '#')
                            {
                                byte.TryParse(SpanColor.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte R);
                                byte.TryParse(SpanColor.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte G);
                                byte.TryParse(SpanColor.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte B);
                                byte.TryParse(SpanColor.Substring(7, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte A);
                                Result.Foreground = new SolidColorBrush(Color.FromArgb(A, R, G, B));
                            }

                            break;
                    }
                }
            }

            int EndIndex = StartIndex + 1;
            index = EndIndex;
            while (EndIndex + 7 < text.Length && text.Substring(EndIndex, 7) != "</span>")
            {
                EndIndex++;
                index = EndIndex + 7;
            }

            string Content = text.Substring(StartIndex + 1, EndIndex - StartIndex - 1);
            Result.Text = Content;

            return Result;
        }
        #endregion
    }
}
