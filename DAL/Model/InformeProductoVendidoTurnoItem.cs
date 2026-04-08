namespace DAL.Model
{
    public class InformeProductoVendidoTurnoItem
    {
        public int id { get; set; }
        public string nombreCategoria { get; set; }
        public string codigoProducto { get; set; }
        public string nombreProducto { get; set; }
        public int idCategoria { get; set; }
        public decimal cantidad { get; set; }
        public decimal valor { get; set; }
    }
}
