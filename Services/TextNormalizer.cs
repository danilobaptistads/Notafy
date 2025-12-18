using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Notafy.Services;

public static class TextNormalizer
{
public static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        string normalizedText = text;

        normalizedText = PreClean(normalizedText);
        normalizedText = NormalizeVisual(normalizedText);
        normalizedText = FixOcrErrors(normalizedText);
        normalizedText = NormalizeSemantic(normalizedText);
        normalizedText = NormalizeStructure(normalizedText);

        return normalizedText;
    }

    private static string PreClean(string text)
    {
        text = text.Replace("\t", " ");
        text = Regex.Replace(text, @"[^A-Za-z0-9 ,.\-\$\n]", "");
        text = Regex.Replace(text, @" {2,}", " ");

        return text.Trim();
    }

    private static string NormalizeVisual(string text)
    {
        text = text.ToUpperInvariant();
        var textNormalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (char c in textNormalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string FixOcrErrors(string text)
    {
        text = Regex.Replace(text, @"(?<=\d)O(?=\d)", "0");
        text = Regex.Replace(text, @"(?<=\d)I(?=\d)", "1");
        text = Regex.Replace(text, @"R ?S", "R$");
        text = Regex.Replace(text, @"(\d{1,3},\d)O", "$10");

        return text;
    }

    private static string NormalizeSemantic(string text)
    {
        text = text.Replace("QTDE", "QTD");
        text = text.Replace("QUANT", "QTD");
        text = text.Replace("QDE", "QTD");
        text = text.Replace("UND", "UN");
        text = text.Replace("UNID", "UN");
        text = text.Replace("VLR", "VALOR");
        text = text.Replace("VL", "VALOR");

        return text;
    }

    private static string NormalizeStructure(string text)
    {
        var linhas = text.Split('\n')
                        .Select(l => l.Trim())
                        .Where(l => !string.IsNullOrWhiteSpace(l));

        return string.Join("\n", linhas);
    }

}