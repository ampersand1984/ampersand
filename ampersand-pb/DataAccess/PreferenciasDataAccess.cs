using System;

namespace ampersand_pb.DataAccess
{
    public class PreferenciasDataAccess : IPreferenciasDataAccess
    {
        public PreferenciasDataAccess()
        {
            _machineName = Environment.MachineName;
        }

        private string _machineName;
    }

    public interface IPreferenciasDataAccess
    {

    }
}
