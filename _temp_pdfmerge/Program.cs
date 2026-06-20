using iTextSharp.text;
using iTextSharp.text.pdf;

if (args.Length < 3)
{
    Console.Error.WriteLine("Usage: PdfMerge <output> <input1> <input2> [inputN...]");
    return 1;
}

var outputPath = args[0];
var inputPaths = args.Skip(1).ToArray();

foreach (var inputPath in inputPaths)
{
    if (!File.Exists(inputPath))
    {
        Console.Error.WriteLine($"Input file not found: {inputPath}");
        return 2;
    }
}

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
using var document = new Document();
using var copy = new PdfCopy(document, stream);
document.Open();

foreach (var inputPath in inputPaths)
{
    using var reader = new PdfReader(inputPath);
    copy.AddDocument(reader);
}

document.Close();
Console.WriteLine(outputPath);
return 0;
