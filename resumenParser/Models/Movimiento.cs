using ampersand.Core;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using ampersand.Core.Common;

namespace ResumenParser.Models
{
	public class Movimiento: BaseModel
    {
        public Movimiento()
        {
            Tipo = "Tarjeta";
            Cuota = string.Empty;
            Error = string.Empty;
        }

        public string Tipo { get; set; }

        public int IdMovimiento
        {
            get;
            set;
        }

        public string Fecha
        {
            get;
            set;
        }

        public string Descripcion
        {
            get;
            set;
        }

        public string DescripcionAdicional
        {
            get;
            set;
        }

        public decimal Monto
        {
            get;
            set;
        }

        public string Cuota
        {
            get;
            set;
        }

        public bool EsMensual { get; set; }

        public string Error
        {
            get;
            set;
        }

        private string _tags = string.Empty;
        public string Tags
        {
            get { return _tags; }
            set { _tags = value; OnPropertyChanged("Tags"); }
        }

        public bool IsSelected { get; set; }

        public static Movimiento GetFromText(string line)
        {
            var mov = new Movimiento();
            try
            {
                var fecha = GetFecha(line);
                mov.Fecha = fecha;

                var monto = GetMonto(line);
                mov.Monto = monto;

                var descri = GetDescri(line);
                mov.Descripcion = descri;

                var cuota = GetCuota(mov.Descripcion);
                if (!cuota.IsNullOrEmpty())
                {
                    mov.Cuota = cuota;
                    mov.Descripcion = mov.Descripcion.Replace("C." + cuota, "").Trim();
                }

                var idMovimiento = GetIdMovimiento(mov.Descripcion);
                if (idMovimiento != -1)
                {
                    mov.IdMovimiento = idMovimiento;
                    mov.Descripcion = mov.Descripcion.Replace(idMovimiento.ToString() + "*", "").Trim();
                }
            }
            catch (Exception ex)
            {
                mov.Error = ex.Message;
                Console.Write(ex.Message);
            }

            return mov;
        }

        private static int GetIdMovimiento(string descri)
        {
            var idMovimiento = -1;

            var end = descri.IndexOf("*");
            if (end > 0)
            {
                var strIdMovimiento = descri.Substring(0, end).Trim();

                if (!int.TryParse(strIdMovimiento, out idMovimiento))
                    idMovimiento = -1;
            }

            return idMovimiento;
        }

        private static string GetCuota(string descri)
        {
            var cuota = string.Empty;
            var start = descri.LastIndexOf("C.");
            if (start > 0)
            {
                cuota = descri.Substring(start, 7);

                var esValida = cuota.IndexOf("/").Equals(4);
                if (esValida)
                    cuota = cuota.Substring(2);
                else
                    cuota = string.Empty;
            }

            return cuota;
        }

        static string GetDescri(string line)
        {
            var start = 9;
            var end = line.Trim().LastIndexOf(" ");
            var descri = line.Trim().Substring(start, end - start);
            return descri.Trim();
        }

        static decimal GetMonto(string line)
        {
            var start = line.Trim().LastIndexOf(" ") + 1;
            var strMonto = line.Trim().Substring(start);
            var monto = 0M;

            decimal.TryParse(strMonto, out monto);
            return monto;
        }

        static string GetFecha(string line)
        {
            var strFecha = line.Substring(0, 9).Trim();

            var fechaArray = strFecha.Split(' ');

            var strDate = fechaArray[0];

            var strMonth = fechaArray[1].ToLower();

            var monthArray = new List<string>()
            {
                "ene", "feb", "mar", "abr", "may", "jun",
                "jul", "ago", "sep", "oct", "nov", "dic" 
            };
            var i = monthArray.IndexOf(strMonth);

            strMonth = (i + 1).ToString("00");

            var strYear = fechaArray[2];
            strYear = "20" + strYear;

            strFecha = strYear + strMonth + strDate;

            return strFecha;
        }

        public override string ToString()
        {
			var str = "IdMovimiento: " + this.IdMovimiento;
			str += "; Fecha: " + this.Fecha;
			str += "; Descripcion: " + this.Descripcion;
			str += this.Cuota.IsNullOrEmpty() ? 
			       string.Empty: "; Cuota: " + this.Cuota;
			str += "; Monto: " + this.Monto;
			str += this.Error.IsNullOrEmpty() ? 
			       string.Empty: 
			       "; Error: " + this.Error;
			return str;
        }

        internal XElement GetXElement()
        {
            var attributes = new List<XAttribute>();

            attributes.Add(new XAttribute("Tipo", this.Tipo));

            attributes.Add(new XAttribute("IdMovimiento", this.IdMovimiento));

            attributes.Add(new XAttribute("Fecha", this.Fecha));

            attributes.Add(new XAttribute("Descripcion", this.Descripcion));

            if (!this.DescripcionAdicional.IsNullOrEmpty())
                attributes.Add(new XAttribute("DescripcionAdicional", this.DescripcionAdicional));

            attributes.Add(new XAttribute("Monto", this.Monto));

            if (!this.Cuota.IsNullOrEmpty())
                attributes.Add(new XAttribute("Cuota", this.Cuota));

            if (!this.Tags.IsNullOrEmpty())
                attributes.Add(new XAttribute("Tags", this.Tags));

                attributes.Add(new XAttribute("EsMensual", this.EsMensual));

            if (!this.Error.IsNullOrEmpty())
                attributes.Add(new XAttribute("Error", this.Error));

            var xelement = new XElement("Mov", attributes);

            return xelement;
        }
    }
}

