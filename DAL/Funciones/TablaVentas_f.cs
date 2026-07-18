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
            var resp = await NuevaVentaDetallada(db, porpro, 0, 0);
            if (resp == null || !resp.estado || resp.data == null)
            {
                return 0;
            }

            int idVenta;
            return int.TryParse(resp.data.ToString(), out idVenta) ? idVenta : 0;
        }

        public static async Task<Respuesta_DAL> NuevaVentaDetallada(string db, int porpro, int idSede, int idBaseCaja)
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
                        IdSede = idSede,
                        guidVenta = guid,
                        abonoTarjeta = 0,
                        propina = 0,
                        abonoEfectivo = 0,
                        idMedioDePago = 10,
                        idResolucion = 0,
                        idFormaDePago = 1,
                        razonDescuento = "-",
                        idBaseCaja = idBaseCaja,
                        aliasVenta = "--",
                        porpropina = Convert.ToDecimal(porpro) / 100m,
                        eliminada = false
                    };

                    var respInsert = await TablaVentasControler.CRUD(db, tablaVentas, 0);
                    if (respInsert == null || !respInsert.estado || respInsert.data == null)
                    {
                        return respInsert ?? new Respuesta_DAL
                        {
                            data = 0,
                            estado = false,
                            mensaje = "No fue posible insertar la nueva venta."
                        };
                    }

                    if (!int.TryParse(respInsert.data.ToString(), out int idVenta))
                    {
                        return new Respuesta_DAL
                        {
                            data = 0,
                            estado = false,
                            mensaje = "La base devolvió un identificador inválido para la nueva venta."
                        };
                    }

                    tablaVentas.id = idVenta;
                    tablaVentas.aliasVenta = idVenta.ToString();

                    if (await FacturaElectronicaControler.ConsultarIdVenta(db, idVenta) != null)
                    {
                        tablaVentas.eliminada = true;
                        tablaVentas.observacionVenta = "ID descartado automaticamente por cruce con FacturaElectronica.idVenta";
                        await TablaVentasControler.CRUD(db, tablaVentas, 1);
                        continue;
                    }

                    var respUpdate = await TablaVentasControler.CRUD(db, tablaVentas, 1);
                    if (respUpdate == null || !respUpdate.estado)
                    {
                        return respUpdate ?? new Respuesta_DAL
                        {
                            data = idVenta,
                            estado = false,
                            mensaje = "La venta se insertó, pero no se pudo actualizar el alias."
                        };
                    }

                    return new Respuesta_DAL
                    {
                        data = idVenta,
                        estado = true,
                        mensaje = "Venta creada correctamente."
                    };
                }

                return new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "No fue posible generar una nueva venta sin cruce con FacturaElectronica."
                };
            }
            catch (Exception ex)
            {
                return new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = ex.Message
                };
            }
        }
    }
}
