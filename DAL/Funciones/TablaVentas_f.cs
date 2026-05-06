using DAL.Controler;
using DAL.Model;
using System;
using System.Threading.Tasks;

namespace DAL.Funciones
{
    public class TablaVentas_f
    {
        private const int MaxIntentosNuevaVenta = 20;

        /// <summary>
        /// Crea una nueva venta en estado PENDIENTE usando CRUD_TablaVentas.
        /// Si el id creado ya existe en FacturaElectronica.idVenta, descarta esa venta
        /// y crea la siguiente para evitar cruces de facturas.
        /// </summary>
        public static async Task<int> NuevaVenta(string db, int porpro)
        {
            try
            {
                for (int intento = 0; intento < MaxIntentosNuevaVenta; intento++)
                {
                    Guid guid = Guid.NewGuid();

                    var tablaVentas = new TablaVentas
                    {
                        id = 0,
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
                        idMedioDePago = 10,
                        idResolucion = 0,
                        idFormaDePago = 1,
                        razonDescuento = "-",
                        idBaseCaja = 0,
                        aliasVenta = "--",
                        porpropina = Convert.ToDecimal(porpro) / 100m,
                        eliminada = false
                    };

                    var respInsert = await TablaVentasControler.CRUD(db, tablaVentas, 0);
                    if (respInsert == null || !respInsert.estado || respInsert.data == null)
                        return 0;

                    if (!int.TryParse(respInsert.data.ToString(), out int idVenta))
                        return 0;

                    tablaVentas.id = idVenta;
                    tablaVentas.aliasVenta = idVenta.ToString();

                    if (await FacturaElectronicaControler.ConsultarIdVenta(db, idVenta) != null)
                    {
                        tablaVentas.eliminada = true;
                        tablaVentas.observacionVenta = "ID descartado automaticamente por cruce con FacturaElectronica.idVenta";
                        await TablaVentasControler.CRUD(db, tablaVentas, 1);
                        continue;
                    }

                    await TablaVentasControler.CRUD(db, tablaVentas, 1);
                    return idVenta;
                }

                return 0;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return 0;
            }
        }
    }
}
