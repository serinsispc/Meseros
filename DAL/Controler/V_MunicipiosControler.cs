using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class V_MunicipiosControler
    {
        public static async Task<List<V_Municipios>> ListaMunicipios(string db)
        {
            try
            {
                using (var cn = new Conection_SQL(db))
                {
                    var json = await cn.EjecutarConsulta("SELECT * FROM V_Municipios ORDER BY name", true);
                    if (string.IsNullOrWhiteSpace(json))
                        return new List<V_Municipios>();

                    var lista = JsonConvert.DeserializeObject<List<V_Municipios>>(json);
                    return lista ?? new List<V_Municipios>();
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new List<V_Municipios>();
            }
        }
    }
}
