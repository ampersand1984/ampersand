using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using ampersand_pb.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ampersand_pb.API.Controllers
{
    public class MovimientosController : ApiController
    {
        public MovimientosController()
        {
            var configuracionM = new ConfiguracionModel();
            _movimientosDA = new MovimientosDataAccess(configuracionM);
            _bienvenidaVM = new BienvenidaViewModel(_movimientosDA, configuracionM);
        }

        private BienvenidaViewModel _bienvenidaVM;

        private MovimientosDataAccess _movimientosDA;

        [HttpGet]
        public IEnumerable<BaseMovimiento> GetUltimosGastos()
        {
            return _bienvenidaVM.Ultimos_Gastos;
        }

    }
}
