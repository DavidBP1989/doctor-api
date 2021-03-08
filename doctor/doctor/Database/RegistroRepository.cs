using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace doctor.Database
{

    public class RegistroRepository
    {
        public int IdRegistro { get; set; }
        public string Status { get; set; }
        public string Clave { get; set; }
        public string Emeci { get; set; }
    }
}