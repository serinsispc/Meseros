/*
FASE 2 - Optimizacion de vistas para caja.aspx

Objetivo:
- Reducir recalculos repetidos sobre V_DetalleCaja
- Mantener la logica actual de negocio
- Probar primero en base de pruebas

Recomendacion:
1. Respaldar definiciones actuales de las vistas
2. Ejecutar este script en QA
3. Probar caja.aspx, crear servicio, seleccionar cuenta y cargar cobro
*/

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER VIEW [dbo].[V_TablaVentas]
AS
WITH DetalleAgr AS (
    SELECT
        dc.idVenta,
        SUM(dc.subTotalDetalle) AS subtotalVenta,
        SUM(dc.baseImpuesto) AS basesIva,
        SUM(CASE WHEN dc.porImpuesto = 0.05 THEN dc.baseImpuesto ELSE 0 END) AS basesIva_5,
        SUM(CASE WHEN dc.porImpuesto = 0.19 THEN dc.baseImpuesto ELSE 0 END) AS basesIva_19,
        SUM(dc.valorImpuesto) AS IVA,
        SUM(CASE WHEN dc.porImpuesto = 0.05 THEN dc.valorImpuesto ELSE 0 END) AS IVA_5,
        SUM(CASE WHEN dc.porImpuesto = 0.19 THEN dc.valorImpuesto ELSE 0 END) AS IVA_19,
        SUM(CASE WHEN dc.impuesto_id = 4 THEN dc.valorImpuesto ELSE 0 END) AS INC,
        SUM(CASE WHEN dc.impuesto_id = 10 THEN dc.valorImpuesto ELSE 0 END) AS INCBolsas,
        SUM(CASE WHEN dc.impuesto_id <> 24
                  AND dc.impuesto_id <> 1
                  AND dc.impuesto_id <> 4
                  AND dc.impuesto_id <> 10
                  AND dc.impuesto_id <> 5
                  AND dc.impuesto_id <> 6
                  AND dc.impuesto_id <> 7
                 THEN dc.valorImpuesto ELSE 0 END) AS otrosImpuestos,
        SUM(dc.totalDetalle) AS totalVenta,
        SUM(CASE WHEN dc.impuesto_id <> 24 THEN dc.valorImpuesto ELSE 0 END) AS impuestosUtilidad,
        SUM(dc.costoTotal) AS costoTotalVenta
    FROM dbo.V_DetalleCaja AS dc
    GROUP BY dc.idVenta
),
CreditoAgr AS (
    SELECT
        pct.idVentaPago AS idVenta,
        SUM(ISNULL(pct.valor_pago_credito_tienda, 0)) AS totalCredito
    FROM dbo.PagosCreditoTienda AS pct
    GROUP BY pct.idVentaPago
),
ClienteAgr AS (
    SELECT
        rvc.idVenta,
        MIN(rvc.idCliente) AS idCliente
    FROM dbo.R_VentaCliente AS rvc
    GROUP BY rvc.idVenta
),
FacturaAgr AS (
    SELECT
        fe.idVenta,
        MAX(fe.id) AS idFacturaElectronica
    FROM dbo.FacturaElectronica AS fe
    GROUP BY fe.idVenta
)
SELECT
    t.id,
    t.fechaVenta,
    t.aliasVenta,
    ISNULL(res.nombreRosolucion, '--') AS tipoFactura,
    ISNULL(res.prefijo, '--') AS prefijo,
    t.numeroVenta,
    t.descuentoVenta,
    t.idMedioDePago,
    t.idResolucion,
    t.idFormaDePago,
    CONVERT(decimal(18,2), ISNULL(da.subtotalVenta, 0)) AS subtotalVenta,
    CONVERT(decimal(18,2), ISNULL(da.basesIva, 0)) AS basesIva,
    CONVERT(decimal(18,2), ISNULL(da.basesIva_5, 0)) AS basesIva_5,
    CONVERT(decimal(18,2), ISNULL(da.basesIva_19, 0)) AS basesIva_19,
    CONVERT(decimal(18,2), ISNULL(da.IVA, 0)) AS IVA,
    CONVERT(decimal(18,2), ISNULL(da.IVA_5, 0)) AS IVA_5,
    CONVERT(decimal(18,2), ISNULL(da.IVA_19, 0)) AS IVA_19,
    CONVERT(decimal(18,2), ISNULL(da.INC, 0)) AS INC,
    CONVERT(decimal(18,2), ISNULL(da.INCBolsas, 0)) AS INCBolsas,
    CONVERT(decimal(18,2), ISNULL(da.otrosImpuestos, 0)) AS otrosImpuestos,
    CONVERT(decimal(18,2), ISNULL(da.IVA, 0)) AS ivaVenta,
    CONVERT(decimal(18,2), ISNULL(da.totalVenta, 0)) AS totalVenta,
    (
        CONVERT(decimal(18,2), ISNULL(da.totalVenta, 0))
        + ISNULL(
            CASE
                WHEN ISNULL(t.propina, 0) = 0
                    THEN CONVERT(decimal(18,2), ISNULL(da.subtotalVenta, 0)) * ISNULL(t.porpropina, 0)
                ELSE t.propina
            END,
            t.propina
        )
        - t.descuentoVenta
    ) AS total_A_Pagar,
    ROUND(CONVERT(decimal, t.efectivoVenta), 0) AS efectivoVenta,
    ROUND(CONVERT(decimal, t.cambioVenta), 0) AS cambioVenta,
    pf.name AS formaDePago,
    CONVERT(decimal, t.abonoEfectivo) AS abonoEfectivo,
    CONVERT(decimal, t.abonoTarjeta) AS abonoTarjeta,
    ROUND(CONVERT(decimal, t.abonoEfectivo + t.abonoTarjeta + ISNULL(ca.totalCredito, 0)), 0) AS totalPagadoVenta,
    (
        CONVERT(decimal(18,2), ISNULL(da.totalVenta, 0) + ISNULL(t.propina, 0))
        - (ISNULL(t.abonoEfectivo, 0) + ISNULL(t.abonoTarjeta, 0))
        - ISNULL(ca.totalCredito, 0)
    ) AS totalPendienteVenta,
    t.estadoVenta,
    pm.name AS medioDePago,
    t.numeroReferenciaPago,
    t.diasCredito,
    DATEADD(DAY, t.diasCredito, t.fechaVenta) AS fechaVencimiento,
    ISNULL(t.observacionVenta, '--') AS observacionVenta,
    t.IdSede,
    t.guidVenta,
    ISNULL(da.costoTotalVenta, 0) AS costoTotalVenta,
    ISNULL(
        (
            ISNULL(da.totalVenta, 0)
            + ROUND(ISNULL(da.impuestosUtilidad, 0), 0)
            - t.descuentoVenta
            - ISNULL(da.costoTotalVenta, 0)
        ),
        0
    ) AS utilidadTotalVenta,
    ISNULL(cli.idCliente, 0) AS idCliente,
    ISNULL(c.identificationNumber, '--') AS nit,
    ISNULL(c.nameCliente, '--') AS nombreCliente,
    ISNULL(
        CASE
            WHEN ISNULL(t.propina, 0) = 0
                THEN CONVERT(decimal(18,2), ISNULL(da.subtotalVenta, 0)) * ISNULL(t.porpropina, 0)
            ELSE t.propina
        END,
        t.propina
    ) AS propina,
    ISNULL(fe.cufe, '--') AS cufe,
    ISNULL(CASE WHEN fe.id IS NOT NULL THEN 'ACEPTADA' ELSE 'DENEGADA' END, 'DENEGADA') AS estadoFE,
    ISNULL(fe.imagenQR, '--') AS imagenQR,
    t.idBaseCaja,
    t.razonDescuento,
    t.porpropina AS por_propina,
    t.eliminada
FROM dbo.TablaVentas AS t
LEFT JOIN DetalleAgr AS da
    ON da.idVenta = t.id
LEFT JOIN CreditoAgr AS ca
    ON ca.idVenta = t.id
LEFT JOIN dbo.Resoluciones AS res
    ON res.idResolucion = t.idResolucion
LEFT JOIN dbo.payment_forms AS pf
    ON pf.id = t.idFormaDePago
LEFT JOIN dbo.payment_methods AS pm
    ON pm.id = t.idMedioDePago
LEFT JOIN ClienteAgr AS cli
    ON cli.idVenta = t.id
LEFT JOIN dbo.Clientes AS c
    ON c.id = cli.idCliente
LEFT JOIN FacturaAgr AS feKey
    ON feKey.idVenta = t.id
LEFT JOIN dbo.FacturaElectronica AS fe
    ON fe.id = feKey.idFacturaElectronica;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER VIEW [dbo].[V_CuentaCliente]
AS
WITH DetallesAgr AS (
    SELECT
        dv.idVenta,
        dv.idCuentaCliente,
        SUM(dv.subTotalDetalle) AS SubtotalVenta,
        SUM(dv.valorImpuesto) AS IvaVenta,
        SUM(dv.totalDetalle) AS TotalVenta
    FROM dbo.V_DetalleCaja AS dv
    GROUP BY dv.idVenta, dv.idCuentaCliente
)
SELECT
    cc.id,
    cc.fecha,
    cc.idVenta,
    cc.nombreCuenta,
    cc.preCuenta,
    cc.eliminada,
    COALESCE(d.SubtotalVenta, 0) AS subtotalVenta,
    COALESCE(d.IvaVenta, 0) AS ivaVenta,
    COALESCE(d.TotalVenta, 0) AS totalVenta,
    cc.por_propina,
    CASE
        WHEN cc.propina = 0 THEN COALESCE(d.SubtotalVenta, 0) * (cc.por_propina / 100.0)
        ELSE cc.propina
    END AS propina,
    CASE
        WHEN cc.propina = 0 THEN COALESCE(d.TotalVenta, 0) + (COALESCE(d.SubtotalVenta, 0) * (cc.por_propina / 100.0))
        ELSE COALESCE(d.TotalVenta, 0) + cc.propina
    END AS total_A_Pagar
FROM dbo.CuentaCliente AS cc
LEFT JOIN DetallesAgr AS d
    ON d.idVenta = cc.idVenta
   AND d.idCuentaCliente = cc.id;
GO
