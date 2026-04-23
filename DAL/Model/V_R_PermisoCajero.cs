namespace DAL.Model
{
    public class V_R_PermisoCajero
    {
        public int id { get; set; }
        public int idCajero { get; set; }
        public int idPermiso { get; set; }
        public string nombreCajero { get; set; }
        public string nombrePermiso { get; set; }
    }
}
