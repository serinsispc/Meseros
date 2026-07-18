/*
FASE 2.2 - Optimizacion de V_DetalleCaja

Objetivo:
- Reducir costo por fila en el detalle de caja
- Reemplazar APPLY/EXISTS repetidos por agregaciones previas
- Mantener el mismo contrato de columnas para WebApplication

Probar primero en base de pruebas.
*/

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER VIEW [dbo].[V_DetalleCaja]
AS
WITH DescuentoAgr AS (
    SELECT
        cdd.idDetalleVenta,
        SUM(cdd.valorDescuento) AS totalDescuento
    FROM dbo.CargosDescuentosDetalleVenta AS cdd
    GROUP BY cdd.idDetalleVenta
),
ComandaAgr AS (
    SELECT
        ci.idDetalleVenta,
        1 AS itemComandado
    FROM dbo.ComandImpresaa AS ci
    GROUP BY ci.idDetalleVenta
),
CuentaDetalleAgr AS (
    SELECT
        rcdv.idDetalleVenta,
        MAX(rcdv.idCuentaCliente) AS idCuentaCliente
    FROM dbo.R_CuentaCliente_DetalleVenta AS rcdv
    WHERE ISNULL(rcdv.eliminada, 0) = 0
    GROUP BY rcdv.idDetalleVenta
)
SELECT
    d.id,
    d.guidDetalle,
    d.idVenta,
    d.idPresentacion,
    d.codigoProducto,
    d.nombreProducto,
    CONVERT(int, d.impuesto_id) AS impuesto_id,
    pre.nombreTipoPresentacion AS presentacion,
    d.cantidadDetalle AS unidad,
    ISNULL(ca.itemComandado, 0) AS itemComandado,
    ISNULL(da.totalDescuento, 0) AS descuentoDetalle,
    CONVERT(decimal(18,2),
        (
            d.precioVenta
            - CASE
                WHEN ISNULL(d.cantidadDetalle, 0) = 0 THEN 0
                ELSE ISNULL(da.totalDescuento, 0) / d.cantidadDetalle
              END
        ) / (1.0 + COALESCE(d.ivaDetalle, 0.0))
    ) AS preVentaNeto,
    CONVERT(decimal(18,2),
        d.precioVenta
        - CASE
            WHEN ISNULL(d.cantidadDetalle, 0) = 0 THEN 0
            ELSE ISNULL(da.totalDescuento, 0) / d.cantidadDetalle
          END
    ) AS precioVenta,
    d.ivaDetalle AS porImpuesto,
    CONVERT(decimal(18,2),
        CASE
            WHEN d.impuesto_id <> 24 THEN
                (
                    (
                        d.precioVenta
                        - CASE
                            WHEN ISNULL(d.cantidadDetalle, 0) = 0 THEN 0
                            ELSE ISNULL(da.totalDescuento, 0) / d.cantidadDetalle
                          END
                    ) / (1.0 + COALESCE(d.ivaDetalle, 0.0))
                ) * d.cantidadDetalle
            ELSE 0
        END
    ) AS baseImpuesto,
    CONVERT(decimal(18,2),
        CASE
            WHEN d.impuesto_id <> 24 THEN
                (
                    (
                        (
                            d.precioVenta
                            - CASE
                                WHEN ISNULL(d.cantidadDetalle, 0) = 0 THEN 0
                                ELSE ISNULL(da.totalDescuento, 0) / d.cantidadDetalle
                              END
                        ) / (1.0 + COALESCE(d.ivaDetalle, 0.0))
                    ) * d.cantidadDetalle
                ) * COALESCE(d.ivaDetalle, 0.0)
            ELSE 0
        END
    ) AS valorImpuesto,
    CONVERT(decimal(18,2),
        (
            (
                d.precioVenta
                - CASE
                    WHEN ISNULL(d.cantidadDetalle, 0) = 0 THEN 0
                    ELSE ISNULL(da.totalDescuento, 0) / d.cantidadDetalle
                  END
            ) / (1.0 + COALESCE(d.ivaDetalle, 0.0))
        ) * d.cantidadDetalle
    ) AS subTotalDetalleNeto,
    CONVERT(decimal(18,2),
        (
            (
                d.precioVenta
                - CASE
                    WHEN ISNULL(d.cantidadDetalle, 0) = 0 THEN 0
                    ELSE ISNULL(da.totalDescuento, 0) / d.cantidadDetalle
                  END
            ) / (1.0 + COALESCE(d.ivaDetalle, 0.0))
        ) * d.cantidadDetalle
    ) AS subTotalDetalle,
    CONVERT(decimal(18,2),
        (d.precioVenta * d.cantidadDetalle) - ISNULL(da.totalDescuento, 0)
    ) AS totalDetalle,
    CONVERT(decimal(18,0), ROUND(d.costoUnidad, 0)) AS costoUnidad,
    pre.contenidoPresentacion AS contenido,
    CONVERT(decimal(18,2),
        CONVERT(decimal(18,0), ROUND(d.costoUnidad, 0)) * COALESCE(d.cantidadDetalle, 0)
    ) AS costoTotal,
    d.observacion,
    d.opciones,
    d.adiciones,
    d.estadoDetalle,
    COALESCE(pv.idCategoria, 0) AS idCategoria,
    COALESCE(cda.idCuentaCliente, 0) AS idCuentaCliente,
    COALESCE(cc.nombreCuenta, '') AS nombreCuenta
FROM dbo.DetalleVenta AS d
LEFT JOIN DescuentoAgr AS da
    ON da.idDetalleVenta = d.id
LEFT JOIN ComandaAgr AS ca
    ON ca.idDetalleVenta = d.id
LEFT JOIN dbo.V_Presentacion AS pre
    ON pre.id = d.idPresentacion
LEFT JOIN dbo.v_productoVenta AS pv
    ON pv.idPresentacion = d.idPresentacion
LEFT JOIN CuentaDetalleAgr AS cda
    ON cda.idDetalleVenta = d.id
LEFT JOIN dbo.CuentaCliente AS cc
    ON cc.id = cda.idCuentaCliente
WHERE d.estadoDetalle = 1;
GO
