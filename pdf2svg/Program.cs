using pdf2cad.Core;
using System;
using System.IO;
using TallComponents.PDF;


namespace pdf2svg
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Processing PDF file...");
            var pdfBytes = File.ReadAllBytes("file.pdf");
            var pdfDocument = new Document(new MemoryStream(pdfBytes));
            

            using (FileStream outFile = new FileStream("file.html", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter outStream = new StreamWriter(outFile))
                {
                    var lineCreator = new LineCreator(pdfDocument, outStream);
                    lineCreator.CreateSvg();
                }
            }

            Console.WriteLine("SVG file created successfully as 'file.html'.");
            Console.ReadKey();
        }
    }
}
