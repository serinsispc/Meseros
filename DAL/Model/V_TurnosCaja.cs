using System;

namespace DAL.Model
{
    public class V_TurnosCaja
    {
        public int id { get; set; }
        public DateTime fechaApertura { get; set; }
        public DateTime? fechaCierre { get; set; }
        public int idUsuarioApertura { get; set; }
        public string nombreUsuario { get; set; }
        public decimal valorBase { get; set; }
        public decimal ventasCredito { get; set; }
        public decimal totalEfectivo { get; set; }
        public decimal efectivoMasBase { get; set; }
        public decimal pagoCC_Efectivo { get; set; }
        public decimal pagoCC_Targeta { get; set; }
        public decimal pagoCP_Efectivo { get; set; }
        public decimal gastos_Efectivo { get; set; }
        public decimal ventasEfectivo { get; set; }
        public decimal ventasTargeta { get; set; }
        public decimal totalIngresos { get; set; }
        public decimal totalEgresos { get; set; }
        public decimal producido { get; set; }
        public string estadoBase { get; set; }
    }
}
