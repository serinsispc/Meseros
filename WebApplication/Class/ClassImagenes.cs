using System;
using System.IO;
using System.Web;
using System.Web.UI;

namespace WebApplication.Class
{
    public static class ClassImagenes
    {
        public static void GuardarImagen(string base64String, string nombreImangen)
        {
            // Quitar encabezado si existe (ej: "data:image/png;base64,")
            if (base64String.Contains(","))
            {
                base64String = base64String.Split(',')[1];
            }

            byte[] imagenBytes = Convert.FromBase64String(base64String);

            // Ruta física del servidor
            string ruta = HttpContext.Current.Server.MapPath("~/Recursos/Imagenes/Logo/") + $"{nombreImangen}.png";

            // Guardar el archivo en el disco
            File.WriteAllBytes(ruta, imagenBytes);
        }
    }
}
