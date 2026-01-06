using DAL.Controler;
using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Funciones
{
    public class DetalleVenta_f
    {
        public static async Task<Respuesta_DAL> AgregarProducto(string db,int idpresentacion,int cantidad,int idventa)
        {
            try
            {
                /* consultamos el producto con el id presentación */
                var producto =await v_productoVentaControler.Consultar_idpresentacion(db,idpresentacion);
                if (producto != null)
                {
                    /* llamamos el objeto detalle venta */
                    var dv = new DetalleVenta
                    {
                        id = 0,
                        idVenta = idventa,
                        idPresentacion = idpresentacion,
                        nombreProducto = producto.nombreProducto,
                        costoUnidad = (decimal)producto.costo_mas_impuesto,
                        precioVenta = producto.precioVenta,
                        estadoDetalle = 1,
                        ivaDetalle = producto.porcentaje_impuesto,
                        cantidadDetalle = cantidad,
                        codigoProducto = producto.codigoProducto,
                        observacion = "--",
                        guidDetalle = Guid.NewGuid(),
                        opciones = "--",
                        adiciones = "--",
                        impuesto_id = producto.impuesto_id
                    };
                    string data = JsonConvert.SerializeObject(dv);
                    /* llamamos el crud para guardar */
                    var respCRUD =await DetalleVentaControler.CRUD(db,dv,0);
                    if (respCRUD.estado) 
                    {
                        return new Respuesta_DAL { data=respCRUD.data, estado= respCRUD.estado, mensaje=$"Producto ({producto.nombreProducto}) agregado." };
                    }
                    else
                    {
                        return new Respuesta_DAL { data = respCRUD.data, estado = respCRUD.estado, mensaje = $"Producto ({producto.nombreProducto}) no fue agregado." };
                    }
                }
                else
                {
                    return new Respuesta_DAL { data = null, estado = false, mensaje = "El producto no se encontro." };
                }
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return new Respuesta_DAL { data=null, estado=false, mensaje=msg };
            }
        }

        public static async Task<Respuesta_DAL> ActualizarCantidadDetalle(string db, int iddetalle,int cantidad)
        {
            try
            {
                var detalle = new DetalleVenta();
                detalle =await DetalleVentaControler.ConsultarId(db,iddetalle);
                if (detalle == null) 
                {
                    return new Respuesta_DAL { data = null, estado = false, mensaje="No se actualizo." }; 
                }

                detalle.cantidadDetalle = cantidad;
                var crud =await DetalleVentaControler.CRUD(db,detalle,1);
                if(crud.estado == false)
                {
                    return new Respuesta_DAL { data = null, estado = false, mensaje = "No se actualizo." };
                }

                return new Respuesta_DAL { data = null, estado = true, mensaje = "Cantidad Actualizada." };
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return new Respuesta_DAL { data = null, estado = false, mensaje = "NO se actualizo." };
            }
        }

        public static async Task<Respuesta_DAL> Eliminar(string db, int iddetalle, string nota)
        {
            try
            {
                var detalle = new DetalleVenta();
                detalle =await DetalleVentaControler.ConsultarId(db,iddetalle);
                if (detalle == null)
                {
                    return new Respuesta_DAL { data = null, estado = false, mensaje = "No se elimino." };
                }
                detalle.nombreProducto = nota;
                detalle.estadoDetalle = 0;
                var crud =await DetalleVentaControler.CRUD(db, detalle, 1);
                if (crud.estado == false)
                {
                    return new Respuesta_DAL { data = null, estado = false, mensaje = "No se elimino." };
                }

                return new Respuesta_DAL { data = null, estado = true, mensaje = "Cantidad eliminada." };
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return new Respuesta_DAL { data = null, estado = false, mensaje = "NO se elimino." };
            }
        }
        public static async Task<Respuesta_DAL> Dividir(string db,int iddetalle, int cantidadActual, int cantidadDividir,int idventa)
        {
            try
            {
                var detalle =await DetalleVentaControler.ConsultarId(db, iddetalle);
                if (detalle == null)
                {
                    return new Respuesta_DAL { data=null, estado=false, mensaje=$"no se encontro el id detalle {iddetalle}" };
                }

                var nuevoDetalle = new DetalleVenta
                {
                    id = 0,
                    idVenta = idventa,
                    idPresentacion = detalle.idPresentacion,
                    nombreProducto = detalle.nombreProducto,
                    costoUnidad = detalle.costoUnidad,
                    precioVenta = detalle.precioVenta,
                    estadoDetalle = detalle.estadoDetalle,
                    ivaDetalle = detalle.ivaDetalle,
                    cantidadDetalle = cantidadDividir,
                    codigoProducto = detalle.codigoProducto,
                    observacion = detalle.observacion,
                    guidDetalle = Guid.NewGuid(),
                    opciones = detalle.opciones,
                    adiciones = detalle.adiciones,
                    impuesto_id = detalle.impuesto_id
                };
                var dal1 =await DetalleVentaControler.CRUD(db, nuevoDetalle,0);
                if (!dal1.estado)
                {
                    return new Respuesta_DAL { data = "Error", estado = false, mensaje = $"no se crear agregar el nuevo detalle." };
                }

                detalle.cantidadDetalle = cantidadActual - cantidadDividir;
                var dal2= await DetalleVentaControler.CRUD(db, detalle, 1);
                if (!dal2.estado)
                {
                    return new Respuesta_DAL { data = "Error", estado = false, mensaje = $"se agrego el nuevo detalle, <br>pero no se logro modificar la cantidad del detalle actual... <br>NOTA: modifica manualmente la cantidad del detalle actual." };
                }

                return new Respuesta_DAL { data = "Success", estado = true, mensaje = $"proceso exitoso." };
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new Respuesta_DAL { data = null, estado = false, mensaje = "no fue posible dividir el item." };
            }
        }

        public static async Task<Respuesta_DAL> NotasDetalle(string db, int iddetalle, string nota)
        {
            try
            {
                var detalle = new DetalleVenta();
                detalle =await DetalleVentaControler.ConsultarId(db,iddetalle);
                if (detalle == null)
                {
                    return new Respuesta_DAL { data = null, estado = false, mensaje = "No se actualizo." };
                }

                detalle.adiciones = nota;
                var crud =await DetalleVentaControler.CRUD(db, detalle, 1);
                if (crud.estado == false)
                {
                    return new Respuesta_DAL { data = null, estado = false, mensaje = "No se actualizo." };
                }

                return new Respuesta_DAL { data = null, estado = true, mensaje = "Nota Actualizada." };
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return new Respuesta_DAL { data = null, estado = false, mensaje = "NO se actualizo." };
            }
        }
    }
}
