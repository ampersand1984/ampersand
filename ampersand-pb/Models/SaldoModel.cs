namespace ampersand_pb.Models
{
    public class SaldoModel : BaseMovimiento
    {
        public SaldoModel()
            : base()
        {

            Tipo = TiposDeMovimiento.Efectivo;
        }
    }
}
