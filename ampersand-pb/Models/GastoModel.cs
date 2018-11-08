namespace ampersand_pb.Models
{
    public class GastoModel: BaseMovimiento
    {
        public GastoModel()
            : base()
        {
            
            Tipo = TiposDeMovimiento.Credito;
        }
    }
}
