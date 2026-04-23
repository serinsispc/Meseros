using DAL.Controler;
using DAL.Model;
using RFacturacionElectronicaDIAN.Entities.Request;
using RFacturacionElectronicaDIAN.Factories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AllowanceCharge = RFacturacionElectronicaDIAN.Entities.Request.AllowanceCharge;
using TaxTotal = RFacturacionElectronicaDIAN.Entities.Request.TaxTotal;

namespace DAL.Funciones
{
    public static class NotaCreditoFEHelper
    {
        public static async Task<bool> GenerarNotaCreditoAnulacion(string db, V_TablaVentas venta, List<V_DetalleCaja> detalle, string tokenFE, string motivo = null)
        {
            try
            {
                if (venta == null || string.IsNullOrWhiteSpace(tokenFE))
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(venta.cufe) || venta.cufe == "--")
                {
                    return false;
                }

                var resolucion = await V_Resoluciones_Controler.ConsultarID(db, venta.idResolucion);
                var cliente = await ClientesControler.Consultar_id(db, venta.idCliente);
                if (resolucion == null || cliente == null)
                {
                    return false;
                }

                FacturacionElectronicaDIANFactory.urlJSON = "https://erog.apifacturacionelectronica.xyz/api/ubl2.1/";
                var api = new FacturacionElectronicaDIANFactory();

                var request = new NotaCreditoRequest
                {
                    token = tokenFE,
                    sync = true,
                    type_document_id = 4,
                    number = await ClassFE.HallarNumeroFE(db, Convert.ToInt32(venta.idResolucion)),
                    discrepancy_response = new NotaCreditoRequest.DiscrepancyResponse
                    {
                        correction_concept_id = 2
                    },
                    billing_reference = new NotaCreditoRequest.BillingReference
                    {
                        number = $"{venta.prefijo}{venta.numeroVenta}",
                        uuid = venta.cufe,
                        issue_date = venta.fechaVenta.ToString("yyyy-MM-dd")
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
                            payment_method_id = Convert.ToInt32(venta.idMedioDePago),
                            payment_form_id = string.Equals(venta.formaDePago, "Contado", StringComparison.OrdinalIgnoreCase) ? 1 : 2,
                            duration_measure = Convert.ToInt32(venta.diasCredito),
                            payment_due_date = venta.fechaVenta.ToString("yyyy-MM-dd")
                        }
                    },
                    legal_monetary_totals = new NotaCreditoRequest.LegalMonetaryTotals_NC
                    {
                        line_extension_amount = FormatearNumero(venta.subtotalVenta),
                        tax_exclusive_amount = FormatearNumero(venta.basesIva),
                        tax_inclusive_amount = FormatearNumero(venta.totalVenta),
                        payable_amount = FormatearNumero(venta.total_A_Pagar)
                    },
                    notes = new List<NotaCreditoRequest.Notas_NC>
                    {
                        new NotaCreditoRequest.Notas_NC
                        {
                            text = string.IsNullOrWhiteSpace(motivo)
                                ? "Nota credito generada por anulacion de la factura electronica."
                                : motivo
                        }
                    },
                    credit_note_lines = new List<NotaCreditoRequest.CreditNoteLines>()
                };

                if (venta.propina > 0 || venta.descuentoVenta > 0)
                {
                    request.allowance_charges = new List<NotaCreditoRequest.AllowanceCharge_NC>();

                    if (venta.propina > 0)
                    {
                        var porcentajePropina = Math.Round(venta.por_propina * 100m, 2);
                        request.allowance_charges.Add(new NotaCreditoRequest.AllowanceCharge_NC
                        {
                            charge_indicator = true,
                            discount_id = 3,
                            allowance_charge_reason = porcentajePropina > 0
                                ? $"Propina voluntaria por el cliente ({porcentajePropina:0.##}%)"
                                : "Propina voluntaria por el cliente",
                            amount = FormatearNumero(venta.propina),
                            base_amount = FormatearNumero(venta.subtotalVenta)
                        });
                    }

                    if (venta.descuentoVenta > 0)
                    {
                        request.allowance_charges.Add(new NotaCreditoRequest.AllowanceCharge_NC
                        {
                            charge_indicator = false,
                            discount_id = 1,
                            allowance_charge_reason = venta.razonDescuento,
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
                        description = Convert.ToString(row.nombreProducto),
                        code = Convert.ToString(row.codigoProducto),
                        type_item_identification_id = 3,
                        price_amount = FormatearNumero(row.precioVenta),
                        base_quantity = "1.000000"
                    };

                    if (row.impuesto_id != 24)
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
                return response != null && response.uuid != null && (response.is_valid ?? false);
            }
            catch
            {
                return false;
            }
        }

        private static string FormatearNumero(decimal valor)
        {
            return valor.ToString("0.00######", CultureInfo.InvariantCulture);
        }
    }
}
