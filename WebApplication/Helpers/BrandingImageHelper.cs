using System;
using System.IO;
using System.Web.UI;

namespace WebApplication.Helpers
{
    public static class BrandingImageHelper
    {
        public static string ResolveFaviconUrl(Page page)
        {
            if (page == null)
            {
                return string.Empty;
            }

            var db = ResolveDb(page);
            if (string.IsNullOrWhiteSpace(db))
            {
                return string.Empty;
            }

            var rutaVirtual = $"~/Recursos/Imagenes/logo/{db}.png";
            var rutaFisica = page.Server.MapPath(rutaVirtual);
            if (!File.Exists(rutaFisica))
            {
                return string.Empty;
            }

            return page.ResolveUrl(rutaVirtual) + "?v=" + File.GetLastWriteTimeUtc(rutaFisica).Ticks;
        }

        private static string ResolveDb(Page page)
        {
            var model = SessionContextHelper.LoadModels(page.Session);
            var db = Convert.ToString(page.Session?[SessionContextHelper.DbKey] ?? model?.db ?? page.Request?.QueryString["db"]);
            return SanitizarDb(db);
        }

        private static string SanitizarDb(string db)
        {
            if (string.IsNullOrWhiteSpace(db))
            {
                return string.Empty;
            }

            return db.Replace("/", string.Empty)
                     .Replace("\\", string.Empty)
                     .Replace("..", string.Empty)
                     .Trim();
        }
    }
}
