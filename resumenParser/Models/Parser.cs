using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;

namespace ResumenParser.Models
{
	public class Parser
	{
		public Parser()
		{
		}

		public IEnumerable<Movimiento> GetMovimientos(IEnumerable<string> lines)
		{
			var movimientos = new List<Movimiento>();
			foreach (var line in lines)
			{
				try
				{
					var mov = Movimiento.GetFromText(line);
					movimientos.Add(mov);
				}
				catch (Exception ex)
				{
					Console.Write(ex.Message);
				}
			}

			return movimientos;
		}

        public XDocument GetXDocument(IEnumerable<Movimiento> movimientos, string periodo, string fechaDeCierre)
		{
            var total = movimientos.Sum(a => a.Monto);

            var xdoc = new XDocument(
                new XElement("Movimientos", 
                    new XAttribute("Periodo", periodo), new XAttribute("FechaDeCierre", fechaDeCierre), new XAttribute("Total", total), new XAttribute("Tipo", "Credito"), new XAttribute("Descripcion", "Visa")));

            foreach (var mov in movimientos)
            {
                var xe = mov.GetXElement();
                xdoc.Root.Add(xe);
            }

            return xdoc;
		}
	}
}

