using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
