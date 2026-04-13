using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Web.SessionState;
using WebApplication.ViewModels;

namespace WebApplication.Helpers
{
    public static class SessionContextHelper
    {
        public const string ModelsKey = "Models";
        public const string ModelsJsonKey = "ModelsJson";
        public const string ModelsJsonAltKey = "modelsJSON";
        public const string DbKey = "db";
        public const string VendedorKey = "Vendedor";
        public const string IdVendedorKey = "idvendedor";
        public const string TokenFeKey = "TokenFE";
        public const string BaseCajaKey = "base_caja";
        public const string IdBaseKey = "idBase";
        public const string UsuarioCajaKey = "usuario_caja";
        public const string AdminControlReminderAtKey = "admincontrol_reminder_at";

        public static MenuViewModels LoadModels(HttpSessionState session)
        {
            if (session == null)
            {
                return null;
            }

            var model = session[ModelsKey] as MenuViewModels;
            if (model != null)
            {
                return model;
            }

            var json = session[ModelsJsonKey] as string;
            if (string.IsNullOrWhiteSpace(json))
            {
                json = session[ModelsJsonAltKey] as string;
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<MenuViewModels>(json);
            }
            catch
            {
                return null;
            }
        }

        public static bool TryLoadModels(HttpSessionState session, out MenuViewModels model)
        {
            model = LoadModels(session);
            return model != null;
        }

        public static void SaveModels(HttpSessionState session, MenuViewModels model)
        {
            if (session == null || model == null)
            {
                return;
            }

            var json = JsonConvert.SerializeObject(model);
            session[ModelsKey] = model;
            session[ModelsJsonKey] = json;
            session[ModelsJsonAltKey] = json;
        }

        public static void ApplyOperationalContext(HttpSessionState session, MenuViewModels model)
        {
            if (session == null || model == null)
            {
                return;
            }

            SaveModels(session, model);
            session[DbKey] = model.db ?? string.Empty;
            session[IdVendedorKey] = model.vendedor?.id ?? 0;
            session[VendedorKey] = model.vendedor == null ? null : JsonConvert.SerializeObject(model.vendedor);
            session[TokenFeKey] = model.TokenEmpresa ?? string.Empty;

            if (model.BaseCaja != null && model.BaseCaja.id > 0)
            {
                session[BaseCajaKey] = JsonConvert.SerializeObject(model.BaseCaja);
                session[IdBaseKey] = model.BaseCaja.id;
            }
            else
            {
                session.Remove(BaseCajaKey);
                session.Remove(IdBaseKey);
            }
        }

        public static int ResolveBaseCajaId(HttpSessionState session, MenuViewModels model)
        {
            if (session == null)
            {
                return model?.BaseCaja?.id ?? 0;
            }

            if (session[IdBaseKey] != null && int.TryParse(Convert.ToString(session[IdBaseKey]), out var idBase) && idBase > 0)
            {
                return idBase;
            }

            if (model?.BaseCaja != null && model.BaseCaja.id > 0)
            {
                session[IdBaseKey] = model.BaseCaja.id;
                session[BaseCajaKey] = JsonConvert.SerializeObject(model.BaseCaja);
                return model.BaseCaja.id;
            }

            var baseCajaJson = session[BaseCajaKey] as string;
            if (!string.IsNullOrWhiteSpace(baseCajaJson))
            {
                try
                {
                    var baseCaja = JsonConvert.DeserializeObject<BaseCaja>(baseCajaJson);
                    if (baseCaja != null && baseCaja.id > 0)
                    {
                        session[IdBaseKey] = baseCaja.id;
                        return baseCaja.id;
                    }
                }
                catch
                {
                }
            }

            return 0;
        }

        public static void ClearOperationalContext(HttpSessionState session)
        {
            if (session == null)
            {
                return;
            }

            session.Remove(ModelsKey);
            session.Remove(ModelsJsonKey);
            session.Remove(ModelsJsonAltKey);
            session.Remove(DbKey);
            session.Remove(VendedorKey);
            session.Remove(IdVendedorKey);
            session.Remove(TokenFeKey);
            session.Remove(BaseCajaKey);
            session.Remove(IdBaseKey);
            session.Remove(UsuarioCajaKey);
            session.Remove(AdminControlReminderAtKey);
        }
    }
}
