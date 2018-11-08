namespace ampersand_pb.Models
{
    public class DeudaModel: BaseMovimiento
    {
        public DeudaModel()
            : base()
        {

            Tipo = TiposDeMovimiento.Deuda;
        }
    }
}
