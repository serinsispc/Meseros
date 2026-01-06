using DAL.Controler;
using DAL.Model;
using System;
using System.Threading.Tasks;

namespace DAL.Funciones
{
    public class TablaVentas_f
    {
        /// <summary>
        /// Crea una nueva venta en estado PENDIENTE usando CRUD_TablaVentas.
        /// Retorna el id de la venta creada, o 0 si falla.
        /// </summary>
        public static async Task<int> NuevaVenta(string db, int porpro)
        {
            try
            {
                Guid guid = Guid.NewGuid();

                // 1. Crear objeto con los valores iniciales
                var tablaVentas = new TablaVentas
                {
                    id = 0,
                    aliasVenta = "--",
                    fechaVenta = DateTime.Now,
                    numeroVenta = 0,
                    descuentoVenta = 0,
                    efectivoVenta = 0,
                    cambioVenta = 0,
                    estadoVenta = "PENDIENTE",
                    numeroReferenciaPago = "-",
                    diasCredito = 0,
                    observacionVenta = "-",
                    IdSede = 0,
                    guidVenta = guid,
                    abonoTarjeta = 0,
                    propina = 0,
                    abonoEfectivo = 0,
                    idMedioDePago = 10,   // como lo tenías
                    idResolucion = 0,
                    idFormaDePago = 1,
                    razonDescuento = "-",
                    idBaseCaja = 0,
                    eliminada = false,
                    porpropina = Convert.ToDecimal(porpro) / 100m
                };

                // 2. INSERT usando SP CRUD_TablaVentas (funcion = 0)
                var respInsert = await TablaVentasControler.CRUD(db, tablaVentas, 0);

                if (respInsert == null || !respInsert.estado || respInsert.data == null)
                    return 0;

                int idVenta;
                if (!int.TryParse(respInsert.data.ToString(), out idVenta))
                    return 0;

                // 3. Actualizar aliasVenta con el id (como hacías antes)
                tablaVentas.id = idVenta;
                tablaVentas.aliasVenta = idVenta.ToString();

                var respUpdate = await TablaVentasControler.CRUD(db, tablaVentas, 1);
                // Si el update falla, igual devolvemos el id, pero podrías validar respUpdate.estado

                return idVenta;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return 0;
            }
        }
    }
}
