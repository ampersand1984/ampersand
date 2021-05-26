namespace ampersand_pb.Models
{
    public class PeriodoModel
    {
        public string Periodo { get; set; }

        public string TextoPeriodo { get; set; }

        public override string ToString()
        {
            var str = string.Format("{0}, {1}", Periodo ?? string.Empty, TextoPeriodo ?? string.Empty);
            return str;
        }
    }
}
