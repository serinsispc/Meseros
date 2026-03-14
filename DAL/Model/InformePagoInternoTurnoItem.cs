namespace DAL.Model
{
    public class InformePagoInternoTurnoItem
    {
        public int id { get; set; }
        public string nombreMPI { get; set; }
        public int estado { get; set; }
        public int reporteDIAN { get; set; }
        public decimal total { get; set; }
    }
}
