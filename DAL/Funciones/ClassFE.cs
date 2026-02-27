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
        public static async Task<bool> FacturaElectronica(string db,V_TablaVentas v_TablaVentas, List<V_DetalleCaja> dataTable,string TokenFE, [Optional] bool numeroFE)
        {
            int IdCliente_frm = 0;
            /* declaramos la url del json */
            FacturacionElectronicaDIANFactory.urlJSON = "https://erog.apifacturacionelectronica.xyz/api/ubl2.1/";
            FacturacionElectronicaDIANFactory facturacionElectronica = new FacturacionElectronicaDIANFactory();

            FacturaNacionalRequest facturaNacional = new FacturaNacionalRequest();
            int numeroFacturaElectronica = 0;
            //if (numeroFE == false)
            //{
            //    numeroFacturaElectronica = await HallarNumeroFE(db,Convert.ToInt32(v_TablaVentas.idResolucion));
            //}
            //else
            //{
            //    numeroFacturaElectronica = v_TablaVentas.numeroVenta;
            //}

            numeroFacturaElectronica = v_TablaVentas.numeroVenta;

            facturaNacional.number = numeroFacturaElectronica;



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
                string observacion = $"el servicio ó producto fue facturado el día {v_TablaVentas.fechaVenta:yyyy-MM-dd} pero con inconvenientes en el sistema de facturación ha sido aceptada por la DIAN el día {DateTime.Today:yyyy-MM-dd}";
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
            Clientes clientes = new Clientes();
            clientes = await ClientesControler.Consultar_id(db, v_TablaVentas.idCliente);
            if (clientes != null)
            {
                IdCliente_frm = (int)clientes.id;
                facturaNacional.customer.identification_number = clientes.identificationNumber;
                facturaNacional.customer.name = clientes.nameCliente;
                facturaNacional.customer.phone = clientes.phone;
                facturaNacional.customer.municipality_id = (int)clientes.municipality_id;
                facturaNacional.customer.address = clientes.adress;
                facturaNacional.customer.email = clientes.email;
                facturaNacional.customer.type_document_identification_id = (int)clientes.typeDocumentIdentification_id;
                facturaNacional.customer.type_organization_id = (int)clientes.typeOrganization_id;
                facturaNacional.customer.merchant_registration = "No tiene";
            }

            /*agregamos el descuento*/
            if (v_TablaVentas.propina > 0 || v_TablaVentas.descuentoVenta > 0)
            {
                facturaNacional.allowance_charges = new List<AllowanceCharge>();
                if (v_TablaVentas.propina > 0)
                {
                        AllowanceCharge listaDescuentos = new AllowanceCharge();
                    listaDescuentos.charge_indicator = true;
                    listaDescuentos.discount_id = 3;
                    listaDescuentos.allowance_charge_reason = "Propina voluntaria por el cliente";
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

                    if (Convert.ToInt32(row.totalDetalle) > 0)
                    {
                        InvoiceLine itemDetalleFactura = new InvoiceLine();

                        itemDetalleFactura.unit_measure_id = 70;
                        itemDetalleFactura.invoiced_quantity = $"{row.unidad}".Replace(",", ".");
                        itemDetalleFactura.line_extension_amount = $"{row.subTotalDetalle}".Replace(",", ".");

                        /*cargamos los descuentos*/
                        itemDetalleFactura.allowance_charges = new List<AllowanceCharge_InvoiceLine>();

                        itemDetalleFactura.tax_totals = new List<TaxTotal>();
                        decimal ivaDetalle = Convert.ToDecimal(row.porImpuesto);
                        if (row.impuesto_id != 24)
                        {

                            TaxTotal taxTotalItem = new TaxTotal();

                            taxTotalItem.tax_id = Convert.ToInt32(row.impuesto_id);
                            taxTotalItem.tax_amount = $"{row.valorImpuesto}".Replace(",", ".");
                            taxTotalItem.taxable_amount = $"{row.baseImpuesto}".Replace(",", ".");
                            string iva = Convert.ToString(row.porImpuesto);
                            int xx = Convert.ToInt32(Convert.ToDecimal(iva) * 100);
                            taxTotalItem.percent = $"{xx}.00";

                            itemDetalleFactura.tax_totals.Add(taxTotalItem);
                        }


                        itemDetalleFactura.description = Convert.ToString(row.nombreProducto);
                        itemDetalleFactura.code = Convert.ToString(row.codigoProducto);
                        itemDetalleFactura.type_item_identification_id = 3;
                        itemDetalleFactura.price_amount = $"{row.precioVenta}".Replace(",", ".");
                        itemDetalleFactura.base_quantity = "1.000000";

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

                        string nombreCliente = string.Empty;
                        string correo = string.Empty;

                        /* en esta parte consultamos los datos del cliente */
                        nombreCliente = clientes.nameCliente;
                        correo = clientes.email;

                        //enviamos el correo
                        CorreoRequest correoRequest = new CorreoRequest();

                        correoRequest.to = new List<To>();
                        To toCorreo = new To();
                        toCorreo.email = correo;
                        correoRequest.to.Add(toCorreo);

                        correoRequest.cc = new List<Cc>();
                        List<V_CorreosCliente> v_CorreosCliente = new List<V_CorreosCliente>();
                        v_CorreosCliente = await V_CorreosClienteControler.Lista(db,IdCliente_frm);
                        if (v_CorreosCliente != null && v_CorreosCliente.Count > 0)
                        {
                            foreach (V_CorreosCliente correos in v_CorreosCliente)
                            {
                                Cc CcCorreo = new Cc();
                                CcCorreo.email = correos.email;
                                correoRequest.cc.Add(CcCorreo);
                            }
                        }

                        correoRequest.bcc = new List<Bcc>();

                        //RFacturacionElectronicaDIAN.Entities.Request.Bcc bccCorreo2 = new RFacturacionElectronicaDIAN.Entities.Request.Bcc();
                        //bccCorreo2.email = "facturacion@serinsispc.com";
                        //correoRequest.bcc.Add(bccCorreo2);

                        //Bcc bccCorreo = new Bcc();
                        //bccCorreo.email = await CorreoInterno();
                        //correoRequest.bcc.Add(bccCorreo);


                        correoRequest.token = TokenFE;
                        CorreoResponse correoResponse = await facturacionElectronica.FacturaMail(correoRequest, uuid);
                        if (correoResponse != null)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                    return false;
                }
            
            }
            else
            {
                return false;
            }
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
            int boton = 0;
            int IdFE = 0;
            FacturaElectronica fe = new FacturaElectronica();
            fe = await FacturaElectronicaControler.ConsultarCUfe(db,cufe);
            if (fe != null)
            {
                boton = 1;
                IdFE = (int)fe.id;
            }
            else
            {
                fe = new FacturaElectronica();
                boton = 0;
                IdFE = 0;
            }
            fe.id = IdFE;
            fe.idVenta = v_TablaVentas.id;
            fe.cufe = cufe;
            fe.numeroFactura = numero;
            fe.fechaEmision = fechaEmisian;
            fe.fecahVensimiento = fechaVencimiento;
            fe.dataQR = dataQR;
            fe.imagenQR = "--";
            fe.resolucion_id = v_TablaVentas.idResolucion;
            fe.prefijo = v_TablaVentas.prefijo;
            fe.numero_factura = numeroFE;
            return await FacturaElectronicaControler.Crud(db,fe, boton);
        }
    }
}
