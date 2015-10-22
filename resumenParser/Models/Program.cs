using System;
using System.Collections.Generic;
using System.IO;
using ResumenParser;

namespace ResumenParser
{
    //class MainClass
    //{
    //    public static void Main(string[] args)
    //    {
    //        var fullFilename = @"C:\Google Drive\Code\Parser\resumen20140123.txt";
    //        //var fullFilename = @"C:\Users\fabriciok\Google Drive\Code\Parser\resumen20140123.txt";
    //        var fullOutputFileName = @"C:\Google Drive\Code\Parser\OutputResumen20140123_"+ Guid.NewGuid() +".txt";
    //        var parser = new ResumenParser(fullFilename);
    //        try
    //        {
    //            var movimientos = parser.GetMovimientos();
    //            foreach (var mov in movimientos)
    //            {
    //                Console.WriteLine(mov.ToString());
    //            }
    //            Console.WriteLine("Guardar en archivo? (Y/N) ");
    //            var resp = Console.ReadKey ();
    //            if (resp.ToString().ToLower().Equals("y"))
    //            {
    //                Console.WriteLine("Periodo (yyyymm) ");
    //                resp = Console.ReadKey ();
    //                if (resp.ToString().PeriodoValido())
    //                {
    //                    var periodoValido = ParserTools.PeriodoValido(resp.ToString());

    //                    var periodo = ParserTools.GetPeriodo(resp.ToString());

    //                    parser.SaveToXmlFile(movimientos, periodo, fullOutputFileName);
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.Write(ex.GetType().Name + ": " + ex.Message);

    //        }
    //        Console.ReadKey();
    //    }
    //}
}
