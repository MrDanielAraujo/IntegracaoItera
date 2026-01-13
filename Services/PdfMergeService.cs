using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace IntegracaoItera.Services;

public static class PdfMergeService
{
    public static byte[] MergePdfs(byte[] pdfBytes1, byte[] pdfBytes2)
    {
        var pdfFiles = new List<byte[]> { pdfBytes1, pdfBytes2 };

        using var mergedPdf = new PdfDocument();
        foreach (var fileBytes in pdfFiles)
        {
            // Abre o PDF a partir do array de bytes em um MemoryStream
            using var stream = new MemoryStream(fileBytes);
            // Usa PdfReader para abrir o documento de origem no modo Import
            var sourcePdf = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

            // Adiciona todas as páginas do documento de origem ao documento mesclado
            for (int i = 0; i < sourcePdf.PageCount; i++)
            {
                mergedPdf.AddPage(sourcePdf.Pages[i]);
            }
        }

        // Salva o documento mesclado em um novo MemoryStream para retornar como byte[]
        byte[] resultBytes;
        using (var outputStream = new MemoryStream())
        {
            mergedPdf.Save(outputStream, false);
            resultBytes = outputStream.ToArray();
        }

        return resultBytes;
    }
}
