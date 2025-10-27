using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

namespace QuestOverlayPlugin.Controls;

public class HtmlTextBlock : TextBlock
{
    static HtmlTextBlock()
    {
        HtmlProperty = DependencyProperty.Register("Html", typeof(string), typeof(HtmlTextBlock),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                OnHtmlChanged, CoerceHtml));
    }

    public static readonly DependencyProperty HtmlProperty;

    public string Html
    {
        get => (string)GetValue(HtmlProperty);
        set => SetValue(HtmlProperty, value);
    }

    private static object CoerceHtml(DependencyObject d, object value)
    {
        OnHtmlChanged(d, (string)value);
        return value;
    }

    private static void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        OnHtmlChanged(d, (string)e.NewValue);

    private static void OnHtmlChanged(DependencyObject d, string newHtml)
    {
        HtmlTextBlock textBlock = (HtmlTextBlock)d;
        if (textBlock.CheckFlags(Flags.HtmlContentChanging))
            return;
        textBlock.SetFlags(true, Flags.HtmlContentChanging);
        try
        {
            ParseFormatting(textBlock, newHtml);
        }
        finally
        {
            textBlock.SetFlags(false, Flags.HtmlContentChanging);
        }
    }

    private static void ParseFormatting(HtmlTextBlock textBlock, string html)
    {
        if (!html.Contains("<"))
        {
            textBlock.SetValue(TextProperty, html);
            return;
        }

        html = html.Replace("<i>", "<Italic>").Replace("</i>", "</Italic>");
        html = html.Replace("<b>", "<Bold>").Replace("</b>", "</Bold>");
        html = html.Replace("<u>", "<Underline>").Replace("</u>", "</Underline>");
        html = html.Replace("\n", "<LineBreak/>");

        TextBlock? tempTextBlock = CreateTextBlock(html);
        if (tempTextBlock == null)
        {
            textBlock.SetValue(TextProperty, html);
            return;
        }

        textBlock.Inlines.Clear();
        Inline[] tempInlines = [..tempTextBlock.Inlines];
        foreach (Inline inline in tempInlines)
            textBlock.Inlines.Add(inline);
    }

    private static TextBlock? CreateTextBlock(string innerXaml)
    {
        string xaml =
            $"""<TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">{innerXaml}</TextBlock>""";
        return XamlReader.Parse(xaml) as TextBlock;
    }

    [Flags]
    private enum Flags
    {
        HtmlContentChanging = 1
    }

    private Flags _flags;

    private bool CheckFlags(Flags flags) => (_flags & flags) == flags;

    private void SetFlags(bool value, Flags flags) => _flags = value ? _flags | flags : _flags & ~flags;
}