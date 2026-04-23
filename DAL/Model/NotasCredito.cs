namespace DAL.Model
{
    public class NotasCredito
    {
        public int? id { get; set; }
        public int? idVenta { get; set; }
        public string cufe { get; set; }
        public string numeroFactura { get; set; }
        public string fechaEmision { get; set; }
        public string fecahVensimiento { get; set; }
        public string dataQR { get; set; }
        public string imagenQR { get; set; }
    }
}
