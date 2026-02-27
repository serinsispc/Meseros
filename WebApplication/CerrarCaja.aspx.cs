using System;
using System.Threading.Tasks;
using WebApplication.Helpers; //esta es la clase que nos ayuda a interpretar los datos que llegan en eventArgument

namespace WebApplication
{
    public partial class CerrarCaja : System.Web.UI.Page
    {
        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) 
            {
                // PRIMER LOAD DE LA PÁGINA
                await InicializarPagina();
            }
        }
        private async Task InicializarPagina()
        {

        }
        protected async void btn_Click(object sender, EventArgs e)
        {
            string accion = hidAccion.Value;
            string eventArgument = hidArgumento.Value;

            switch (accion)
            {
                case "EventoPrueva":
                    await EventoPrueva(eventArgument);
                    break;
            }
        }

        private async Task EventoPrueva(string parametro)
        {
            var datos = new EventArgumentParser(parametro);

            int id=datos.GetInt("ID");
            int cantidad=datos.GetInt("CANT");

            //ID=15|CANT=3|PRECIO=4500|ESBONO=true
            int id_ = datos.GetInt("ID");
            int cant = datos.GetInt("CANT");
            decimal precio = datos.GetDecimal("PRECIO");
            bool esBono = datos.GetBool("ESBONO");
        }
    }
}