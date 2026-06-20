using DAL.Controler;
using DAL.Model;
using Newtonsoft.Json;
using QRCoder;
using RFacturacionElectronicaDIAN.Entities.Request;
using RFacturacionElectronicaDIAN.Entities.Response;
using RFacturacionElectronicaDIAN.Factories;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AllowanceCharge = RFacturacionElectronicaDIAN.Entities.Request.AllowanceCharge;
using Bcc = RFacturacionElectronicaDIAN.Entities.Request.Bcc;
using Cc = RFacturacionElectronicaDIAN.Entities.Request.Cc;
using Customer = RFacturacionElectronicaDIAN.Entities.Request.Customer;
using LegalMonetaryTotals = RFacturacionElectronicaDIAN.Entities.Request.LegalMonetaryTotals;
using TaxTotal = RFacturacionElectronicaDIAN.Entities.Request.TaxTotal;
using To = RFacturacionElectronicaDIAN.Entities.Request.To;

namespace DAL.Funciones
{
    public class ClassFE
    {
        private const int ReferencePriceValorComercialId = 1;

        public static async Task<bool> FacturaElectronica(string db,V_TablaVentas v_TablaVentas, List<V_DetalleCaja> dataTable,string TokenFE, [Optional] bool numeroFE)
        {
            int IdCliente_frm = 0;
            var documento = ObtenerDocumentoNotificacion(v_TablaVentas);
            int? consecutivoIntentado = v_TablaVentas?.numeroVenta;

            var facturaExistente = await FacturaElectronicaControler.ConsultarIdVenta(db, v_TablaVentas.id);
            if (VentaYaFacturadaElectronicamente(v_TablaVentas, facturaExistente))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(TokenFE))
            {
                await RegistrarNotificacionSeguraAsync(
                    db,
                    v_TablaVentas?.id ?? 0,
                    "ERROR_ENVIO",
                    "Error al enviar factura a DIAN",
                    "No se encontro el token de facturacion electronica para enviar la factura.",
                    "El proceso se detuvo antes de invocar la API de DIAN porque el token estaba vacio o no cargado en sesion.",
                    documento,
                    v_TablaVentas?.cufe,
                    consecutivoIntentado);
                return false;
            }

            /* declaramos la url del json */
            FacturacionElectronicaDIANFactory.urlJSON = "https://erog.apifacturacionelectronica.xyz/api/ubl2.1/";
            FacturacionElectronicaDIANFactory facturacionElectronica = new FacturacionElectronicaDIANFactory();

            FacturaNacionalRequest facturaNacional = new FacturaNacionalRequest();
            int numeroFacturaElectronica = 0;
            if (numeroFE == false)
            {
                numeroFacturaElectronica = await HallarNumeroFE(db, Convert.ToInt32(v_TablaVentas.idResolucion));
            }
            else
            {
                numeroFacturaElectronica = v_TablaVentas.numeroVenta;
            }



            facturaNacional.number = numeroFacturaElectronica;
            consecutivoIntentado = numeroFacturaElectronica > 0 ? numeroFacturaElectronica : consecutivoIntentado;



            facturaNacional.resolution_id = Convert.ToInt32(v_TablaVentas.idResolucion);




            facturaNacional.type_document_id = 1;
            if ($"{v_TablaVentas.fechaVenta:yyyy-MM-dd}" == $"{DateTime.Today:yyy-MM-dd}")
            {
                facturaNacional.date = $"{v_TablaVentas.fechaVenta:yyyy-MM-dd}";
                facturaNacional.time = $"{v_TablaVentas.fechaVenta:HH:mm:ss}";
            }
            else
            {
                facturaNacional.date = $"{DateTime.Today:yyyy-MM-dd}";
                facturaNacional.time = $"{DateTime.Today:HH:mm:ss}";
                /*en esta parte creamos la observacion*/
                string observacion = $"el servicio \u00F3 producto fue facturado el d\u00EDa {v_TablaVentas.fechaVenta:yyyy-MM-dd} pero con inconvenientes en el sistema de facturaci\u00F3n ha sido aceptada por la DIAN el d\u00EDa {DateTime.Today:yyyy-MM-dd}";
                TablaVentas tablaVentas = new TablaVentas();
                tablaVentas = await TablaVentasControler.ConsultarIdVenta(db,v_TablaVentas.id);
                if (tablaVentas != null)
                {
                    tablaVentas.observacionVenta = $"{tablaVentas.observacionVenta} - {observacion}";
                    await TablaVentasControler.CRUD(db,tablaVentas, 1);
                }
                v_TablaVentas.observacionVenta = tablaVentas.observacionVenta;
            }

            /* Metodos de Pago */
            facturaNacional.payment_forms = new List<PaymentForms>();
            PaymentForms paymentForms = new PaymentForms();
            paymentForms.payment_method_id = Convert.ToInt32(v_TablaVentas.idMedioDePago);
            if (v_TablaVentas.formaDePago == "Contado")
            {
                paymentForms.payment_form_id = 1;
            }
            else
            {
                paymentForms.payment_form_id = 2;
            }
            paymentForms.duration_measure = Convert.ToInt32(v_TablaVentas.diasCredito);
            paymentForms.payment_due_date = v_TablaVentas.fechaVenta.ToString("yyyy-MM-dd");
            facturaNacional.payment_forms.Add(paymentForms);


            facturaNacional.customer = new Customer();

            //en esta parte cargamos los datos del cliente
            Clientes clientes = await ConsultarClienteFacturacionAsync(db, v_TablaVentas);
            if (clientes == null)
            {
                await RegistrarNotificacionSeguraAsync(
                    db,
                    v_TablaVentas?.id ?? 0,
                    "VALIDACION_CLIENTE",
                    "Validacion previa de cliente",
                    "La venta no tiene un cliente asociado para facturacion electronica.",
                    "Se reviso idCliente en la venta y la relacion R_VentaCliente sin encontrar un cliente valido.",
                    documento,
                    v_TablaVentas?.cufe,
                    consecutivoIntentado);
                return false;
            }

            var validacionesCliente = ValidarClienteFacturacion(clientes);
            if (validacionesCliente.Count > 0)
            {
                await RegistrarNotificacionSeguraAsync(
                    db,
                    v_TablaVentas?.id ?? 0,
                    "VALIDACION_CLIENTE",
                    "Validacion previa de cliente",
                    "La venta no cumple los requisitos minimos del cliente para facturacion electronica.",
                    string.Join(" ", validacionesCliente),
                    documento,
                    v_TablaVentas?.cufe,
                    consecutivoIntentado);
                return false;
            }

            IdCliente_frm = clientes.id;
            facturaNacional.customer.identification_number = clientes.identificationNumber;
            facturaNacional.customer.name = clientes.nameCliente;
            facturaNacional.customer.phone = clientes.phone;
            facturaNacional.customer.municipality_id = clientes.municipality_id;
            facturaNacional.customer.address = clientes.adress;
            facturaNacional.customer.email = clientes.email;
            facturaNacional.customer.type_document_identification_id = clientes.typeDocumentIdentification_id;
            facturaNacional.customer.type_organization_id = clientes.typeOrganization_id;
            facturaNacional.customer.merchant_registration = string.IsNullOrWhiteSpace(clientes.merchantRegistration)
                ? "No tiene"
                : clientes.merchantRegistration;

            /*agregamos el descuento*/
            if (v_TablaVentas.propina > 0 || v_TablaVentas.descuentoVenta > 0)
            {
                facturaNacional.allowance_charges = new List<AllowanceCharge>();
                if (v_TablaVentas.propina > 0)
                {
                    var porcentajePropina = Math.Round(v_TablaVentas.por_propina * 100m, 2);
                    AllowanceCharge listaDescuentos = new AllowanceCharge();
                    listaDescuentos.charge_indicator = true;
                    listaDescuentos.discount_id = 3;
                    listaDescuentos.allowance_charge_reason = porcentajePropina > 0
                        ? $"Propina voluntaria por el cliente ({porcentajePropina:0.##}%)"
                        : "Propina voluntaria por el cliente";
                    listaDescuentos.amount = $"{v_TablaVentas.propina}".Replace(",", ".");
                    listaDescuentos.base_amount = $"{v_TablaVentas.subtotalVenta}".Replace(",", ".");
                    facturaNacional.allowance_charges.Add(listaDescuentos);
                }
                if (v_TablaVentas.descuentoVenta > 0)
                {

                    AllowanceCharge listaDescuentos = new AllowanceCharge();
                    listaDescuentos.discount_id = 1;
                    listaDescuentos.charge_indicator = false;
                    listaDescuentos.allowance_charge_reason = $"{v_TablaVentas.razonDescuento}";
                    listaDescuentos.amount = Convert.ToInt32(v_TablaVentas.descuentoVenta).ToString();
                    listaDescuentos.base_amount = Convert.ToInt32(v_TablaVentas.subtotalVenta).ToString();

                    facturaNacional.allowance_charges.Add(listaDescuentos);
                }
            }



            facturaNacional.legal_monetary_totals = new LegalMonetaryTotals();

            facturaNacional.legal_monetary_totals.line_extension_amount = $"{v_TablaVentas.subtotalVenta}".Replace(",", ".");
            facturaNacional.legal_monetary_totals.tax_exclusive_amount = $"{v_TablaVentas.basesIva}".Replace(",", ".");
            facturaNacional.legal_monetary_totals.tax_inclusive_amount = $"{v_TablaVentas.totalVenta}".Replace(",", ".");
            facturaNacional.legal_monetary_totals.payable_amount = $"{v_TablaVentas.total_A_Pagar}".Replace(",", ".");

            facturaNacional.notes = new List<Notas>();



            if (v_TablaVentas.observacionVenta != string.Empty)
            {
                Notas itemNotas = new Notas();
                string NOTA = "";
                if (v_TablaVentas.observacionVenta == null)
                {
                    NOTA = "...";
                }
                else
                {
                    NOTA = v_TablaVentas.observacionVenta;
                }
                itemNotas.text = NOTA;
                facturaNacional.notes.Add(itemNotas);
            }



            facturaNacional.invoice_lines = new List<InvoiceLine>();


            //en esta parte tramor el listado de los productos
            if (dataTable != null)
            {
                foreach (V_DetalleCaja row in dataTable)
                {
                    bool esCortesia = EsCortesiaAutomatica(row);
                    if (Convert.ToInt32(row.totalDetalle) > 0 || esCortesia)
                    {
                        decimal precioReferencia = esCortesia
                            ? await ObtenerPrecioReferenciaCortesiaAsync(db, row)
                            : row.precioVenta;
                        if (esCortesia && precioReferencia <= 0)
                        {
                            // El item queda en la venta como control interno, pero no se reporta
                            // en la factura electronica si no existe valor comercial de referencia.
                            continue;
                        }

                        InvoiceLine itemDetalleFactura = new InvoiceLine();

                        itemDetalleFactura.unit_measure_id = 70;
                        itemDetalleFactura.invoiced_quantity = FormatearNumero(row.unidad);
                        itemDetalleFactura.line_extension_amount = esCortesia
                            ? "0.00"
                            : FormatearNumero(row.subTotalDetalle);

                        /*cargamos los descuentos*/
                        itemDetalleFactura.allowance_charges = new List<AllowanceCharge_InvoiceLine>();

                        itemDetalleFactura.tax_totals = new List<TaxTotal>();
                        if (!esCortesia && row.impuesto_id != 24)
                        {

                            TaxTotal taxTotalItem = new TaxTotal();

                            taxTotalItem.tax_id = Convert.ToInt32(row.impuesto_id);
                            taxTotalItem.tax_amount = FormatearNumero(row.valorImpuesto);
                            taxTotalItem.taxable_amount = FormatearNumero(row.baseImpuesto);
                            string iva = Convert.ToString(row.porImpuesto);
                            int xx = Convert.ToInt32(Convert.ToDecimal(iva) * 100);
                            taxTotalItem.percent = $"{xx}.00";

                            itemDetalleFactura.tax_totals.Add(taxTotalItem);
                        }


                        itemDetalleFactura.description = Convert.ToString(row.nombreProducto);
                        itemDetalleFactura.code = Convert.ToString(row.codigoProducto);
                        itemDetalleFactura.type_item_identification_id = 3;
                        itemDetalleFactura.price_amount = FormatearNumero(precioReferencia);
                        itemDetalleFactura.base_quantity = "1.000000";
                        if (esCortesia)
                        {
                            itemDetalleFactura.reference_price_id = ReferencePriceValorComercialId;
                            itemDetalleFactura.free_of_charge_indicator = true;
                        }

                        facturaNacional.invoice_lines.Add(itemDetalleFactura);
                    }

                }
            }




            facturaNacional.token = TokenFE;
            // Esto solo enviar cuando estamos en pruebas.
            facturaNacional.sync = true;

            /****************************************************************************/
            /****************************************************************************/
            /****************************************************************************/
            /****************************************************************************/


            FacturaNacionalResponse facturaNacionalRespuesta = await facturacionElectronica.FacturaNacional(facturaNacional, v_TablaVentas.id);
            if (facturaNacionalRespuesta != null)
            {
                try
                {
                    int cantidadMensajes = 0;
                    string mensajeError = "";
                    if (facturaNacionalRespuesta.errors_messages != null && facturaNacionalRespuesta.errors_messages.Count > 0)
                    {
                        cantidadMensajes = facturaNacionalRespuesta.errors_messages.Count();
                        for (int i = 0; i < cantidadMensajes; i++)
                        {
                            mensajeError = $"{mensajeError} {System.Environment.NewLine} {System.Environment.NewLine}" +
                                $"{facturaNacionalRespuesta.errors_messages.ToList()[i]}";
                        }
                    }
                    if (facturaNacionalRespuesta.uuid != null)
                    {
                        TablaVentas venta = new TablaVentas();
                        venta = await TablaVentasControler.ConsultarIdVenta(db,v_TablaVentas.id);
                        if (venta != null)
                        {
                            string s = facturaNacionalRespuesta.number.Replace(v_TablaVentas.prefijo, "");
                            numeroFacturaElectronica = Convert.ToInt32(s);
                            int numero = numeroFacturaElectronica;
                            venta.numeroVenta = numero;
                            var resp = await TablaVentasControler.CRUD(db, venta,1);
                        }
                        V_Resoluciones resolucione = new V_Resoluciones();
                        resolucione = await V_Resoluciones_Controler.ConsultarID(db,v_TablaVentas.idResolucion);



                        string uuid = facturaNacionalRespuesta.uuid.ToString();
                        string cufeFE = uuid;

                        if (cufeFE == string.Empty)
                        {
                            return false;
                        }

                        //cacturamos todos los catos de espueta.
                        string numeroFactura = facturaNacionalRespuesta.number;
                        string fechaEmisian = facturaNacionalRespuesta.issue_date.ToString();
                        string fechaVencimiento = facturaNacionalRespuesta.expedition_date.ToString();
                        string dpf = facturaNacionalRespuesta.pdf_download_link.ToString();
                        string dataCode = facturaNacionalRespuesta.qr_data.ToString();
                        string data64 = facturaNacionalRespuesta.xml_base64_bytes.ToString();
                        string name_xml = facturaNacionalRespuesta.xml_name.ToString();
                        string name_pdf = facturaNacionalRespuesta.uuid.ToString();
                        string name_zip = facturaNacionalRespuesta.zip_name.ToString();
                        string factura = facturaNacionalRespuesta.number.ToString();
                        //string totalFactura = facturaNacionalRespuesta.payload.legal_monetary_totals.payable_amount.ToString();
                        //string xx = $"{totalFactura.Replace(".00", ""):C0}";
                        string url_pdf = facturaNacionalRespuesta.pdf_download_link.ToString();
                        string pdf_Base64 = facturaNacionalRespuesta.pdf_base64_bytes.ToString();

                        //Crear_carpete(name_zip);
                        string rqfe=await General_qr(dataCode, v_TablaVentas.id);
                        /* guardamos los catos de la factura */
                        var respg=await GestionarFacturaElectronica(db,uuid, numeroFactura, fechaEmisian, fechaVencimiento, dataCode, rqfe, numeroFacturaElectronica, resolucione,v_TablaVentas);
                        if (!respg)
                        {
                            await RegistrarNotificacionSeguraAsync(
                                db,
                                v_TablaVentas.id,
                                "ERROR_ENVIO",
                                "Factura aceptada por DIAN sin persistencia local",
                                $"La DIAN devolvio CUFE para la factura {documento}, pero no se pudo guardar localmente el resultado en FacturaElectronica.",
                                $"CUFE: {uuid}. Numero DIAN: {numeroFactura}. Valide persistencia local de FacturaElectronica y la vista V_TablaVentas.",
                                documento,
                                uuid,
                                numeroFacturaElectronica);
                            return false;
                        }

                        await RegistrarNotificacionSeguraAsync(
                            db,
                            v_TablaVentas.id,
                            "ENVIO_OK",
                            "Factura aceptada por DIAN",
                            $"La factura {documento} fue enviada correctamente a la DIAN.",
                            "La validacion se completo sin errores y el CUFE quedo guardado localmente.",
                            documento,
                            uuid,
                            numeroFacturaElectronica);

                        var correoEnviado = await EnviarFacturaElectronicaCorreo(db, IdCliente_frm, uuid, TokenFE, clientes?.email);
                        if (correoEnviado)
                        {
                            await RegistrarNotificacionSeguraAsync(
                                db,
                                v_TablaVentas.id,
                                "CORREO_OK",
                                "Factura enviada por correo",
                                $"La factura {documento} fue enviada por correo despues de ser aceptada por DIAN.",
                                clientes?.email,
                                documento,
                                uuid,
                                numeroFacturaElectronica);
                        }
                        else
                        {
                            await RegistrarNotificacionSeguraAsync(
                                db,
                                v_TablaVentas.id,
                                "CORREO_ERROR",
                                "No fue posible enviar la factura por correo",
                                $"La factura {documento} fue aceptada por DIAN, pero no se pudo completar el envio por correo.",
                                clientes?.email,
                                documento,
                                uuid,
                                numeroFacturaElectronica);
                        }
                        if (correoEnviado)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        var detalleError = mensajeError;
                        if (string.IsNullOrWhiteSpace(detalleError))
                        {
                            detalleError = FirstNotEmpty(
                                facturaNacionalRespuesta.message,
                                facturaNacionalRespuesta.status_message,
                                facturaNacionalRespuesta.status_description,
                                FacturacionElectronicaDIANFactory.ErrorProsesoDian);
                        }

                        await RegistrarNotificacionSeguraAsync(
                            db,
                            v_TablaVentas.id,
                            "ERROR_ENVIO",
                            "Error al enviar factura a DIAN",
                            string.IsNullOrWhiteSpace(detalleError) ? "La DIAN no devolvio CUFE para la factura enviada." : detalleError,
                            $"Documento: {documento}. Consecutivo intentado: {numeroFacturaElectronica}.",
                            documento,
                            v_TablaVentas?.cufe,
                            numeroFacturaElectronica);
                        return false;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    await RegistrarNotificacionSeguraAsync(
                        db,
                        v_TablaVentas?.id ?? 0,
                        "ERROR_ENVIO",
                        "Error al procesar respuesta de DIAN",
                        ex.Message,
                        ex.ToString(),
                        documento,
                        v_TablaVentas?.cufe,
                        consecutivoIntentado);
                    return false;
                }
            
            }
            else
            {
                await RegistrarNotificacionSeguraAsync(
                    db,
                    v_TablaVentas?.id ?? 0,
                    "ERROR_ENVIO",
                    "Sin respuesta valida de DIAN",
                    "El proveedor de facturacion no devolvio una respuesta interpretable para esta factura.",
                    FacturacionElectronicaDIANFactory.ErrorProsesoDian,
                    documento,
                    v_TablaVentas?.cufe,
                    consecutivoIntentado);
                return false;
            }
        }

        private static bool EsCortesiaAutomatica(V_DetalleCaja row)
        {
            if (row == null)
            {
                return false;
            }

            return row.unidad > 0
                && row.precioVenta <= 0
                && row.totalDetalle <= 0;
        }

        private static bool VentaYaFacturadaElectronicamente(V_TablaVentas venta, FacturaElectronica facturaElectronica)
        {
            if (venta != null && !string.IsNullOrWhiteSpace(venta.cufe) && venta.cufe != "--")
            {
                return true;
            }

            if (facturaElectronica == null)
            {
                return false;
            }

            return !string.IsNullOrWhiteSpace(facturaElectronica.cufe)
                || !string.IsNullOrWhiteSpace(facturaElectronica.numeroFactura)
                || (facturaElectronica.numero_factura ?? 0) > 0;
        }

        private static async Task<decimal> ObtenerPrecioReferenciaCortesiaAsync(string db, V_DetalleCaja row)
        {
            if (row == null)
            {
                return 0m;
            }

            if (row.preVentaNeto > 0)
            {
                return row.preVentaNeto;
            }

            if (row.precioVenta > 0)
            {
                return row.precioVenta;
            }

            if (row.unidad > 0 && row.subTotalDetalleNeto > 0)
            {
                return Math.Round(row.subTotalDetalleNeto / row.unidad, 2);
            }

            if (row.costoUnidad > 0)
            {
                return row.costoUnidad;
            }

            if (row.idPresentacion > 0)
            {
                var producto = await v_productoVentaControler.Consultar_idpresentacion(db, row.idPresentacion);
                if (producto != null)
                {
                    if (producto.precioVenta > 0)
                    {
                        return producto.precioVenta;
                    }

                    if (producto.costo_mas_impuesto > 0)
                    {
                        return producto.costo_mas_impuesto;
                    }
                }
            }

            return 0m;
        }

        private static string FormatearNumero(decimal valor)
        {
            return valor.ToString("0.00####", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static async Task<bool> EnviarFacturaElectronicaCorreo(string db, int idCliente, string uuid, string tokenFE, string correoPrincipal = null, IEnumerable<string> correosCopia = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(uuid) || uuid == "--" || string.IsNullOrWhiteSpace(tokenFE))
                {
                    return false;
                }

                FacturacionElectronicaDIANFactory.urlJSON = "https://erog.apifacturacionelectronica.xyz/api/ubl2.1/";
                FacturacionElectronicaDIANFactory facturacionElectronica = new FacturacionElectronicaDIANFactory();

                string correo = string.IsNullOrWhiteSpace(correoPrincipal) ? string.Empty : correoPrincipal.Trim();
                if (string.IsNullOrWhiteSpace(correo) && idCliente > 0)
                {
                    var cliente = await ClientesControler.Consultar_id(db, idCliente);
                    correo = cliente?.email?.Trim() ?? string.Empty;
                }

                if (string.IsNullOrWhiteSpace(correo))
                {
                    return false;
                }

                CorreoRequest correoRequest = new CorreoRequest();
                correoRequest.to = new List<To>();
                correoRequest.cc = new List<Cc>();
                correoRequest.bcc = new List<Bcc>();

                To toCorreo = new To();
                toCorreo.email = correo;
                correoRequest.to.Add(toCorreo);

                IEnumerable<string> correosAdicionales = correosCopia;
                if (correosAdicionales == null && idCliente > 0)
                {
                    List<V_CorreosCliente> v_CorreosCliente = await V_CorreosClienteControler.Lista(db, idCliente);
                    if (v_CorreosCliente != null && v_CorreosCliente.Count > 0)
                    {
                        correosAdicionales = v_CorreosCliente.Select(x => x?.email);
                    }
                }

                if (correosAdicionales != null)
                {
                    var correosNormalizados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var correoCc in correosAdicionales)
                    {
                        if (string.IsNullOrWhiteSpace(correoCc))
                        {
                            continue;
                        }

                        var correoCcNormalizado = correoCc.Trim();
                        if (string.Equals(correoCcNormalizado, correo, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (!correosNormalizados.Add(correoCcNormalizado))
                        {
                            continue;
                        }

                        Cc ccCorreo = new Cc();
                        ccCorreo.email = correoCcNormalizado;
                        correoRequest.cc.Add(ccCorreo);
                    }
                }

                string correoCopiaOculta = await ObtenerCorreoCopiaOcultaDian(db);
                if (!string.IsNullOrWhiteSpace(correoCopiaOculta))
                {
                    Bcc bccCorreo = new Bcc();
                    bccCorreo.email = correoCopiaOculta.Trim();
                    correoRequest.bcc.Add(bccCorreo);
                }

                correoRequest.token = tokenFE;
                CorreoResponse correoResponse = await facturacionElectronica.FacturaMail(correoRequest, uuid);
                return correoResponse != null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return false;
            }
        }
        private static async Task<string> ObtenerCorreoCopiaOcultaDian(string db)
        {
            try
            {
                string query = @"
                    SELECT TOP 1 correo
                    FROM ConfiguracionDian
                    WHERE correo IS NOT NULL
                      AND LTRIM(RTRIM(correo)) <> ''";

                var cn = new SqlAutoDAL();
                var resultado = await cn.EjecutarSQLObjeto<ConfiguracionDianCorreoItem>(db, query);
                return resultado?.correo;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return string.Empty;
            }
        }

        private class ConfiguracionDianCorreoItem
        {
            public string correo { get; set; }
        }
        public static async Task<int> HallarNumeroFE(string db, int idresolucion)
        {
            try
            {
                string query = $@"
            SELECT ISNULL(MAX(numero_factura), 0) + 1 AS consecutivo 
            FROM FacturaElectronica 
            WHERE resolucion_id = {idresolucion}";

                var cn = new SqlAutoDAL();
                var resultado = await cn.EjecutarSQLObjeto<V_ConsecutivoVenta>(db,query);
                if (resultado == null) return 0;
                return resultado.consecutivo;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return 0;
            }
        }

        public static async Task<string> General_qr(string qr, int IdVenta_frm)
        {
            using (QRCodeGenerator qRGenerator = new QRCodeGenerator())
            {
                QRCodeData qrDatos = qRGenerator.CreateQrCode(qr, QRCodeGenerator.ECCLevel.H);

                using (QRCode qrCodigo = new QRCode(qrDatos))
                using (Bitmap qrImagen = qrCodigo.GetGraphic(10, Color.Black, Color.White, null))
                using (MemoryStream ms = new MemoryStream())
                {
                    qrImagen.Save(ms, ImageFormat.Png);

                    byte[] arreglo = ms.ToArray();

                    string base64 = Convert.ToBase64String(arreglo);

                    return base64;
                }
            }
        }
        public static async Task<bool> GestionarFacturaElectronica(string db,string cufe, string numero, string fechaEmisian, string fechaVencimiento, string dataQR, string imagenQR, int numeroFE, V_Resoluciones resolucion, V_TablaVentas v_TablaVentas)
        {
            try
            {
                string cufeSql = EscapeSql(cufe);
                string numeroSql = EscapeSql(numero);
                string fechaEmisionSql = EscapeSql(fechaEmisian);
                string fechaVencimientoSql = EscapeSql(fechaVencimiento);
                string dataQrSql = EscapeSql(dataQR);
                string imagenQrSql = EscapeSql(string.IsNullOrWhiteSpace(imagenQR) ? "--" : imagenQR);
                string prefijoSql = EscapeSql(v_TablaVentas?.prefijo ?? string.Empty);

                string query = $@"
DECLARE @idFacturaElectronica int;
IF EXISTS (
    SELECT 1
    FROM dbo.FacturaElectronica
    WHERE idVenta = {v_TablaVentas.id}
      AND ISNULL(cufe, '') <> '{cufeSql}'
)
BEGIN
    SELECT CAST(0 AS bit) AS estado, 'La venta ya tiene una factura electronica registrada con otro CUFE.' AS mensaje;
    RETURN;
END;

SELECT TOP 1 @idFacturaElectronica = id
FROM dbo.FacturaElectronica
WHERE cufe = '{cufeSql}';

IF @idFacturaElectronica IS NULL
BEGIN
    INSERT INTO dbo.FacturaElectronica
    (
        idVenta,cufe,numeroFactura,fechaEmision,fecahVensimiento,dataQR,imagenQR,resolucion_id,prefijo,numero_factura
    )
    VALUES
    (
        {v_TablaVentas.id},'{cufeSql}','{numeroSql}','{fechaEmisionSql}','{fechaVencimientoSql}','{dataQrSql}','{imagenQrSql}',{v_TablaVentas.idResolucion},'{prefijoSql}',{numeroFE}
    );
END
ELSE
BEGIN
    UPDATE dbo.FacturaElectronica
    SET
        idVenta = {v_TablaVentas.id},
        cufe = '{cufeSql}',
        numeroFactura = '{numeroSql}',
        fechaEmision = '{fechaEmisionSql}',
        fecahVensimiento = '{fechaVencimientoSql}',
        dataQR = '{dataQrSql}',
        imagenQR = '{imagenQrSql}',
        resolucion_id = {v_TablaVentas.idResolucion},
        prefijo = '{prefijoSql}',
        numero_factura = {numeroFE}
    WHERE id = @idFacturaElectronica;
END;

IF COL_LENGTH('dbo.TablaVentas', 'cufe') IS NOT NULL
BEGIN
    EXEC('UPDATE dbo.TablaVentas SET cufe = ''''{0}'''' WHERE id = {1};');
END;

IF COL_LENGTH('dbo.TablaVentas', 'imagenQR') IS NOT NULL
BEGIN
    EXEC('UPDATE dbo.TablaVentas SET imagenQR = ''''{2}'''' WHERE id = {1};');
END;

SELECT CAST(1 AS bit) AS estado, '' AS mensaje;";

                query = string.Format(query, cufeSql, v_TablaVentas.id, imagenQrSql);

                var cn = new SqlAutoDAL();
                var resultado = await cn.EjecutarSQLObjeto<SqlResultadoOperacion>(db, query);
                return resultado != null && resultado.estado;
            }
            catch
            {
                return false;
            }
        }

        private static string ObtenerDocumentoNotificacion(V_TablaVentas venta)
        {
            if (venta == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(venta.prefijo) && venta.numeroVenta > 0)
            {
                return $"{venta.prefijo.Trim()}-{venta.numeroVenta}";
            }

            if (venta.numeroVenta > 0)
            {
                return venta.numeroVenta.ToString();
            }

            return venta.aliasVenta ?? string.Empty;
        }

        private static async Task RegistrarNotificacionSeguraAsync(
            string db,
            int idVenta,
            string tipoNotificacion,
            string titulo,
            string mensaje,
            string detalle,
            string documento,
            string cufe,
            int? consecutivoIntentado)
        {
            try
            {
                await FacturaElectronicaNotificacionControler.RegistrarAsync(
                    db,
                    idVenta,
                    tipoNotificacion,
                    titulo,
                    mensaje,
                    detalle,
                    documento,
                    cufe,
                    consecutivoIntentado);
            }
            catch
            {
            }
        }

        private static string FirstNotEmpty(params string[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return string.Empty;
        }

        private static async Task<Clientes> ConsultarClienteFacturacionAsync(string db, V_TablaVentas venta)
        {
            if (venta == null)
            {
                return null;
            }

            if (venta.idCliente > 0)
            {
                var clienteVenta = await ClientesControler.Consultar_id(db, venta.idCliente);
                if (clienteVenta != null)
                {
                    return clienteVenta;
                }
            }

            if (venta.id > 0)
            {
                var relacion = await R_VentaCliente_Controler.ConsultarRelacion(db, venta.id);
                if (relacion != null && relacion.idCliente > 0)
                {
                    return await ClientesControler.Consultar_id(db, relacion.idCliente);
                }
            }

            return null;
        }

        private static List<string> ValidarClienteFacturacion(Clientes cliente)
        {
            var errores = new List<string>();
            if (cliente == null)
            {
                errores.Add("No fue posible cargar el cliente.");
                return errores;
            }

            if (string.IsNullOrWhiteSpace(cliente.identificationNumber))
            {
                errores.Add("El cliente no tiene identificacion configurada.");
            }

            if (string.IsNullOrWhiteSpace(cliente.nameCliente))
            {
                errores.Add("El cliente no tiene nombre o razon social configurada.");
            }

            if (cliente.municipality_id <= 0)
            {
                errores.Add("El cliente no tiene municipio DIAN configurado.");
            }

            if (string.IsNullOrWhiteSpace(cliente.adress))
            {
                errores.Add("El cliente no tiene direccion configurada.");
            }

            if (string.IsNullOrWhiteSpace(cliente.email))
            {
                errores.Add("El cliente no tiene correo configurado.");
            }

            if (cliente.typeDocumentIdentification_id <= 0)
            {
                errores.Add("El cliente no tiene tipo de documento configurado.");
            }

            if (cliente.typeOrganization_id <= 0)
            {
                errores.Add("El cliente no tiene tipo de organizacion configurado.");
            }

            return errores;
        }

        private static string EscapeSql(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }

        private class SqlResultadoOperacion
        {
            public bool estado { get; set; }
            public string mensaje { get; set; }
        }
    }
}

