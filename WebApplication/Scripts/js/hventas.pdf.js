(function () {
    'use strict';

    function safe(value, fallback) {
        return value == null || value === '' ? (fallback || '') : String(value);
    }

    function plain(value, fallback) {
        var text = safe(value, fallback)
            .replace(/&nbsp;/gi, ' ')
            .replace(/&amp;/gi, '&')
            .replace(/&quot;/gi, '"')
            .replace(/&#39;/gi, "'")
            .replace(/&lt;/gi, '<')
            .replace(/&gt;/gi, '>');

        var map = {
            '\u00e1': 'a', '\u00e9': 'e', '\u00ed': 'i', '\u00f3': 'o', '\u00fa': 'u',
            '\u00c1': 'A', '\u00c9': 'E', '\u00cd': 'I', '\u00d3': 'O', '\u00da': 'U',
            '\u00f1': 'n', '\u00d1': 'N', '\u00fc': 'u', '\u00dc': 'U'
        };

        text = text.replace(/[^\x20-\x7E]/g, function (char) {
            return map[char] || ' ';
        });

        return text.replace(/\s+/g, ' ').trim();
    }

    function money(value) {
        var number = Number(value || 0);
        try {
            return number.toLocaleString('es-CO', {
                style: 'currency',
                currency: 'COP',
                maximumFractionDigits: 0
            });
        } catch (e) {
            return '$ ' + Math.round(number || 0);
        }
    }

    function split(doc, text, width) {
        return doc.splitTextToSize(plain(text, '--'), width);
    }

    function box(doc, x, y, w, h, fill) {
        if (fill) {
            doc.setFillColor(fill[0], fill[1], fill[2]);
            doc.rect(x, y, w, h, 'F');
        }
        doc.setDrawColor(105, 105, 105);
        doc.setLineWidth(0.35);
        doc.rect(x, y, w, h);
    }

    function textPair(doc, label, value, xLabel, xValue, y, maxWidth) {
        doc.setFont('helvetica', 'bold');
        doc.text(label, xLabel, y);
        doc.setFont('helvetica', 'normal');
        doc.text(plain(value, '--'), xValue, y, maxWidth ? { maxWidth: maxWidth } : undefined);
    }

    window.hvBuildInvoicePdf = function (payload) {
        if (!payload || !window.jspdf || !window.jspdf.jsPDF) {
            if (window.LoaderGlobal) LoaderGlobal.ocultar();
            if (window.Swal) Swal.fire('Error', 'No fue posible construir el PDF.', 'error');
            return;
        }

        var jsPDF = window.jspdf.jsPDF;
        var doc = new jsPDF('p', 'mm', 'a4');
        var pageWidth = doc.internal.pageSize.getWidth();
        var pageHeight = doc.internal.pageSize.getHeight();
        var margin = 10;
        var contentWidth = pageWidth - (margin * 2);

        doc.setTextColor(0, 0, 0);
        doc.setLineJoin('miter');
        doc.setLineCap('butt');

        doc.setFont('helvetica', 'bold');
        doc.setFontSize(13);
        doc.text(plain(payload.empresaNombre, 'MI EMPRESA').toUpperCase(), pageWidth / 2, 17, { align: 'center' });
        doc.setLineWidth(0.45);
        doc.line(margin, 22, pageWidth - margin, 22);

        if (payload.logoBase64) {
            try {
                doc.addImage(payload.logoBase64, 'PNG', 10, 27, 27, 20);
            } catch (e) {
            }
        }

        doc.setFont('helvetica', 'normal');
        doc.setFontSize(8.7);
        doc.text([
            plain('NIT: ' + safe(payload.empresaNit, '--')),
            plain(payload.empresaDireccion, '--'),
            plain(payload.empresaTelefono, '--'),
            plain(payload.empresaEmail, '--')
        ], 88, 31, { align: 'center' });

        if (payload.qrBase64) {
            try {
                doc.addImage(payload.qrBase64, 'PNG', 126, 25, 28, 28);
            } catch (e) {
            }
        }

        box(doc, 156, 29, 44, 26, [250, 250, 250]);
        doc.setFont('helvetica', 'bold');
        doc.setFontSize(8.5);
        doc.text(split(doc, payload.tipoFactura || 'FACTURA DE VENTA', 36), 178, 38, { align: 'center' });
        doc.setFontSize(10.3);
        doc.text(plain(payload.numeroFactura, ''), 178, 49, { align: 'center' });

        box(doc, 10, 60, 126, 31, [255, 255, 255]);
        box(doc, 138, 60, 62, 31, [255, 255, 255]);

        doc.setFont('helvetica', 'bold');
        doc.setFontSize(8.4);
        textPair(doc, 'Senores:', payload.clienteNombre, 12, 29, 67, 102);
        textPair(doc, 'Nit:', payload.clienteDocumento, 12, 29, 74, 102);
        textPair(doc, 'Direccion:', payload.clienteDireccion, 12, 29, 81, 102);
        textPair(doc, 'Telefono:', payload.clienteTelefono, 12, 29, 88, 44);
        textPair(doc, 'Ciudad:', payload.clienteCiudad, 78, 92, 88, 40);

        box(doc, 138, 60, 62, 8, [248, 248, 248]);
        doc.setFont('helvetica', 'bold');
        doc.text('Fecha y hora factura', 169, 65.2, { align: 'center' });
        doc.setFont('helvetica', 'normal');
        doc.setFontSize(8.3);
        doc.text(split(doc, 'Generacion: ' + plain(payload.fechaGeneracion, '--'), 52), 140, 73.5);
        doc.text(split(doc, 'Expedicion: ' + plain(payload.fechaExpedicion, '--'), 52), 140, 80.5);
        doc.text(split(doc, 'Vencimiento: ' + plain(payload.fechaVencimiento, '--'), 52), 140, 87.5);

        var rows = (payload.detalles || []).map(function (item) {
            return [
                plain(item.codigo, 'ITEM'),
                plain(item.descripcion, ''),
                plain(item.cantidad, '0'),
                money(item.unitario),
                money(item.impuesto),
                money(item.total)
            ];
        });

        doc.autoTable({
            startY: 95,
            margin: { left: margin, right: margin },
            head: [['Codigo', 'Descripcion', 'Cantidad', 'Vr. Unitario', 'Vr. Impto', 'Vr. Total']],
            body: rows.length ? rows : [['', 'Sin detalles', '', '', '', '']],
            theme: 'grid',
            styles: {
                fontSize: 8.1,
                cellPadding: 1.8,
                lineColor: [125, 125, 125],
                lineWidth: 0.12,
                textColor: [0, 0, 0]
            },
            headStyles: {
                fillColor: [224, 224, 224],
                textColor: [0, 0, 0],
                fontStyle: 'bold'
            },
            columnStyles: {
                0: { cellWidth: 24 },
                1: { cellWidth: 77 },
                2: { cellWidth: 18, halign: 'right' },
                3: { cellWidth: 24, halign: 'right' },
                4: { cellWidth: 22, halign: 'right' },
                5: { cellWidth: 25, halign: 'right' }
            }
        });

        var afterTableY = (doc.lastAutoTable ? doc.lastAutoTable.finalY : 110) + 4;
        box(doc, 10, afterTableY, 126, 50, [255, 255, 255]);
        box(doc, 138, afterTableY, 62, 50, [255, 255, 255]);

        doc.setFont('helvetica', 'bold');
        doc.setFontSize(9.2);
        doc.text('Total items: ' + plain(payload.totalItems, '0'), 12, afterTableY + 8.5);

        doc.setFont('helvetica', 'normal');
        doc.setFontSize(8.7);
        doc.text('Valor en letras: ' + plain(payload.valorLetras, ''), 12, afterTableY + 18, { maxWidth: 121 });
        doc.text('Condiciones de pago: ' + plain(payload.formaPago, '--'), 12, afterTableY + 28, { maxWidth: 121 });
        doc.setFont('helvetica', 'bold');
        doc.text('Observaciones:', 12, afterTableY + 39);
        doc.setFont('helvetica', 'normal');
        doc.text(split(doc, payload.observacion || '--', 121), 12, afterTableY + 45);

        var summaryRows = [
            ['Total Bruto', money(payload.totalBruto)],
            [plain(payload.nombreImpuesto, 'IVA'), money(payload.iva)],
            ['Descuento', money(payload.descuento)],
            [plain(payload.nombreRecargo, 'Propina'), money(payload.recargo)],
            ['Total a Pagar', money(payload.totalPagar)]
        ];

        doc.setFont('helvetica', 'bold');
        doc.setFontSize(9.3);
        summaryRows.forEach(function (row, index) {
            var lineY = afterTableY + 8 + (index * 8.1);
            if (index > 0) {
                doc.line(138, lineY - 4.7, 200, lineY - 4.7);
            }
            doc.text(row[0], 140, lineY);
            doc.text(row[1], 198, lineY, { align: 'right' });
        });

        var footerY = afterTableY + 56;
        box(doc, 10, footerY, contentWidth, 30, [238, 238, 238]);
        doc.setFont('helvetica', 'normal');
        doc.setFontSize(7.8);
        var footerLines = [];
        if (payload.resolucionNumero) footerLines.push('Numero de autorizacion ' + plain(payload.resolucionNumero));
        if (payload.resolucionFecha) footerLines.push('Autorizada el ' + plain(payload.resolucionFecha));
        if (payload.resolucionRango) footerLines.push(plain(payload.resolucionRango));
        if (payload.cufe) footerLines.push('CUFE: ' + plain(payload.cufe));
        doc.text(footerLines, pageWidth / 2, footerY + 7, { align: 'center', maxWidth: 176 });

        doc.setFontSize(7.1);
        doc.text('Elaborado desde el sistema web', 198, Math.min(pageHeight - 6, footerY + 24), { align: 'right' });

        var nombreArchivo = plain(payload.archivo, 'factura').replace(/[\\/:*?"<>|]+/g, '_') + '.pdf';
        doc.save(nombreArchivo);

        if (window.LoaderGlobal) LoaderGlobal.ocultar();
    };
})();
