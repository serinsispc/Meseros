using DAL.Controler;
using DAL.Model;
using RFacturacionElectronicaDIAN.Entities.Request;
using RFacturacionElectronicaDIAN.Factories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AllowanceCharge = RFacturacionElectronicaDIAN.Entities.Request.AllowanceCharge;
using TaxTotal = RFacturacionElectronicaDIAN.Entities.Request.TaxTotal;

namespace DAL.Funciones
{
    public static class NotaCreditoFEHelper
    {
        public class NotaCreditoResultado
        {
            public bool Exitoso { get; set; }
            public string Mensaje { get; set; }
            public string NumeroNotaCredito { get; set; }
            public string CufeNotaCredito { get; set; }
            public string PdfDownloadLink { get; set; }
        }

        public static async Task<NotaCreditoResultado> GenerarNotaCreditoAnulacion(string db, V_TablaVentas venta, List<V_DetalleCaja> detalle, string tokenFE, string motivo = null)
        {
            try
            {
                if (venta == null || string.IsNullOrWhiteSpace(tokenFE))
                {
                    return new NotaCreditoResultado { Exitoso = false, Mensaje = "No hay informacion suficiente para generar la nota credito." };
                }

                if (string.IsNullOrWhiteSpace(venta.cufe) || venta.cufe == "--")
                {
                    return new NotaCreditoResultado { Exitoso = false, Mensaje = "La factura electronica no tiene CUFE." };
                }

                var resolucion = await V_Resoluciones_Controler.ConsultarID(db, venta.idResolucion);
                var cliente = await ClientesControler.Consultar_id(db, venta.idCliente);
                var facturaElectronica = await FacturaElectronicaControler.ConsultarIdVenta(db, venta.id);
                if (resolucion == null || cliente == null)
                {
                    return new NotaCreditoResultado { Exitoso = false, Mensaje = "No se encontro la resolucion o el cliente de la factura electronica." };
                }

                cliente.email = await ResolverCorreoClienteAsync(db, cliente);
                if (string.IsNullOrWhiteSpace(cliente.email))
                {
                    return new NotaCreditoResultado { Exitoso = false, Mensaje = "El cliente no tiene correo configurado." };
                }

                var numeroFacturaReferencia = string.IsNullOrWhiteSpace(facturaElectronica?.numeroFactura)
                    ? $"{venta.prefijo}{venta.numeroVenta}"
                    : facturaElectronica.numeroFactura;
                var fechaFacturaReferencia = string.IsNullOrWhiteSpace(facturaElectronica?.fechaEmision)
                    ? venta.fechaVenta.ToString("yyyy-MM-dd")
                    : facturaElectronica.fechaEmision;
                var paymentMethodId = venta.idMedioDePago > 0
                    ? Convert.ToInt32(venta.idMedioDePago)
                    : 10;
                var paymentFormId = string.Equals(venta.formaDePago, "Contado", StringComparison.OrdinalIgnoreCase)
                    ? 1
                    : 2;
                var paymentDueDate = venta.fechaVencimiento.ToString("yyyy-MM-dd");

                FacturacionElectronicaDIANFactory.urlJSON = "https://erog.apifacturacionelectronica.xyz/api/ubl2.1/";
                var api = new FacturacionElectronicaDIANFactory();

                var request = new NotaCreditoRequest
                {
                    token = tokenFE,
                    sync = true,
                    type_document_id = 5,
                    number = await ClassFE.HallarNumeroFE(db, Convert.ToInt32(venta.idResolucion)),
                    discrepancy_response = new NotaCreditoRequest.DiscrepancyResponse
                    {
                        correction_concept_id = 2
                    },
                    billing_reference = new NotaCreditoRequest.BillingReference
                    {
                        number = numeroFacturaReferencia,
                        uuid = venta.cufe,
                        issue_date = fechaFacturaReferencia
                    },
                    resolution = new NotaCreditoRequest.Resolution_NC
                    {
                        prefix = resolucion.prefijo,
                        from = Convert.ToInt32(string.IsNullOrWhiteSpace(resolucion.desde) ? "0" : resolucion.desde),
                        to = Convert.ToInt32(string.IsNullOrWhiteSpace(resolucion.hasta) ? "0" : resolucion.hasta)
                    },
                    customer = new NotaCreditoRequest.Customer_NC
                    {
                        identification_number = cliente.identificationNumber,
                        name = cliente.nameCliente,
                        phone = cliente.phone,
                        municipality_id = Convert.ToInt32(cliente.municipality_id),
                        address = cliente.adress,
                        email = cliente.email,
                        merchant_registration = string.IsNullOrWhiteSpace(cliente.merchantRegistration) ? "No tiene" : cliente.merchantRegistration,
                        type_organization_id = Convert.ToInt32(cliente.typeOrganization_id),
                        type_document_identification_id = Convert.ToInt32(cliente.typeDocumentIdentification_id),
                        trade_name = cliente.tradeName
                    },
                    payment_forms = new List<NotaCreditoRequest.PaymentForms_NC>
                    {
                        new NotaCreditoRequest.PaymentForms_NC
                        {
                            payment_method_id = paymentMethodId,
                            payment_form_id = paymentFormId,
                            duration_measure = Convert.ToInt32(venta.diasCredito),
                            payment_due_date = paymentDueDate
                        }
                    },
                    legal_monetary_totals = new NotaCreditoRequest.LegalMonetaryTotals_NC
                    {
                        line_extension_amount = FormatearNumero(venta.subtotalVenta),
                        tax_exclusive_amount = FormatearNumero(venta.basesIva),
                        tax_inclusive_amount = FormatearNumero(venta.totalVenta + venta.descuentoVenta),
                        payable_amount = FormatearNumero(venta.totalVenta)
                    },
                    notes = new List<NotaCreditoRequest.Notas_NC>
                    {
                        new NotaCreditoRequest.Notas_NC
                        {
                            text = string.IsNullOrWhiteSpace(motivo)
                                ? (string.IsNullOrWhiteSpace(venta.observacionVenta)
                                    ? "Anulacion de factura electronica mediante nota credito."
                                    : venta.observacionVenta)
                                : motivo
                        }
                    },
                    credit_note_lines = new List<NotaCreditoRequest.CreditNoteLines>()
                };

                if (venta.descuentoVenta > 0)
                {
                    request.allowance_charges = new List<NotaCreditoRequest.AllowanceCharge_NC>();

                    if (venta.descuentoVenta > 0)
                    {
                        request.allowance_charges.Add(new NotaCreditoRequest.AllowanceCharge_NC
                        {
                            charge_indicator = false,
                            discount_id = 1,
                            allowance_charge_reason = string.IsNullOrWhiteSpace(venta.razonDescuento) ? "Descuento" : venta.razonDescuento,
                            amount = FormatearNumero(venta.descuentoVenta),
                            base_amount = FormatearNumero(venta.subtotalVenta)
                        });
                    }
                }

                foreach (var row in detalle ?? new List<V_DetalleCaja>())
                {
                    if (row.totalDetalle <= 0)
                    {
                        continue;
                    }

                    var linea = new NotaCreditoRequest.CreditNoteLines
                    {
                        unit_measure_id = 70,
                        invoiced_quantity = FormatearNumero(row.unidad),
                        line_extension_amount = FormatearNumero(row.subTotalDetalle),
                        allowance_charges = new List<AllowanceCharge>(),
                        tax_totals = new List<TaxTotal>(),
                        description = string.IsNullOrWhiteSpace(Convert.ToString(row.nombreProducto))
                            ? "Producto"
                            : Convert.ToString(row.nombreProducto),
                        code = string.IsNullOrWhiteSpace(Convert.ToString(row.codigoProducto))
                            ? Convert.ToString(row.id)
                            : Convert.ToString(row.codigoProducto),
                        type_item_identification_id = 3,
                        price_amount = FormatearNumero(row.precioVenta),
                        base_quantity = "1.000000"
                    };

                    if (row.impuesto_id > 0 && row.impuesto_id != 24 && row.valorImpuesto > 0)
                    {
                        var porcentaje = Convert.ToDecimal(row.porImpuesto) * 100m;
                        linea.tax_totals.Add(new TaxTotal
                        {
                            tax_id = Convert.ToInt32(row.impuesto_id),
                            tax_amount = FormatearNumero(row.valorImpuesto),
                            taxable_amount = FormatearNumero(row.baseImpuesto),
                            percent = FormatearNumero(porcentaje)
                        });
                    }

                    request.credit_note_lines.Add(linea);
                }

                if (venta.ivaVenta > 0)
                {
                    request.tax_totals = new NotaCreditoRequest.TaxTotal_NC
                    {
                        tax_id = 1,
                        tax_amount = FormatearNumero(venta.ivaVenta),
                        taxable_amount = FormatearNumero(venta.basesIva),
                        percent = venta.basesIva > 0
                            ? FormatearNumero(Math.Round((venta.ivaVenta / venta.basesIva) * 100m, 2))
                            : "0.00"
                    };
                }

                var response = await api.NotaCredito(request);
                if (response == null)
                {
                    return new NotaCreditoResultado { Exitoso = false, Mensaje = "La API de nota credito no devolvio respuesta." };
                }

                if (response.uuid != null && (response.is_valid ?? false))
                {
                    var qrData = Convert.ToString(response.qr_data);
                    if (string.IsNullOrWhiteSpace(qrData))
                    {
                        qrData = Convert.ToString(response.qr_link);
                    }

                    var fechaEmision = Convert.ToString(response.issue_date);
                    var fechaVencimiento = Convert.ToString(response.expedition_date);
                    var cufeNotaCredito = Convert.ToString(response.uuid);

                    await NotasCreditoControler.GuardarResultadoAsync(db, new NotasCredito
                    {
                        idVenta = venta.id,
                        cufe = cufeNotaCredito,
                        numeroFactura = response.number,
                        fechaEmision = string.IsNullOrWhiteSpace(fechaEmision) ? DateTime.Today.ToString("yyyy-MM-dd") : fechaEmision,
                        fecahVensimiento = string.IsNullOrWhiteSpace(fechaVencimiento) ? DateTime.Today.ToString("yyyy-MM-dd") : fechaVencimiento,
                        dataQR = qrData ?? string.Empty,
                        imagenQR = string.Empty
                    });

                    return new NotaCreditoResultado
                    {
                        Exitoso = true,
                        Mensaje = $"Nota credito generada correctamente: {response.number}",
                        NumeroNotaCredito = response.number,
                        CufeNotaCredito = cufeNotaCredito,
                        PdfDownloadLink = Convert.ToString(response.pdf_download_link)
                    };
                }

                var mensaje = response.status_description;
                if (string.IsNullOrWhiteSpace(mensaje))
                {
                    mensaje = response.status_message;
                }

                if (response.errors_messages != null && response.errors_messages.Count > 0)
                {
                    mensaje = string.Join(" | ", response.errors_messages);
                }

                if (string.IsNullOrWhiteSpace(mensaje))
                {
                    mensaje = "La API rechazo la nota credito electronica.";
                }

                return new NotaCreditoResultado
                {
                    Exitoso = false,
                    Mensaje = mensaje
                };
            }
            catch (Exception ex)
            {
                return new NotaCreditoResultado
                {
                    Exitoso = false,
                    Mensaje = ex.GetBaseException().Message
                };
            }
        }

        private static string FormatearNumero(decimal valor)
        {
            return valor.ToString("0.00######", CultureInfo.InvariantCulture);
        }

        private static async Task<string> ResolverCorreoClienteAsync(string db, Clientes cliente)
        {
            var correos = new List<string>();

            if (!string.IsNullOrWhiteSpace(cliente?.email))
            {
                correos.Add(cliente.email.Trim());
            }

            if (cliente?.id > 0)
            {
                var correosAdicionales = await V_CorreosClienteControler.Lista(db, cliente.id);
                correos.AddRange((correosAdicionales ?? new List<V_CorreosCliente>())
                    .Select(x => (x?.email ?? string.Empty).Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x)));
            }

            return correos
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault() ?? string.Empty;
        }
    }
}
