/*
FASE 3 - Cache / materializacion de agregados para caja

Objetivo:
- Evitar recalcular siempre los mismos totales y relaciones por venta
- Tener una tabla resumen que se pueda recalcular por venta o completa
- Preparar la base para una version aun mas rapida de caja.aspx

IMPORTANTE:
- Ejecutar primero en base de pruebas
- Probar reconstruccion total y por venta
- Este script no reemplaza automaticamente las vistas actuales; deja la base lista
  para una fase siguiente donde caja consuma el cache o las vistas lean de el
*/

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.CajaVentaResumen', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CajaVentaResumen
    (
        idVenta INT NOT NULL PRIMARY KEY,
        fechaActualizacion DATETIME2(0) NOT NULL,

        subtotalVenta DECIMAL(18,2) NOT NULL DEFAULT(0),
        basesIva DECIMAL(18,2) NOT NULL DEFAULT(0),
        basesIva_5 DECIMAL(18,2) NOT NULL DEFAULT(0),
        basesIva_19 DECIMAL(18,2) NOT NULL DEFAULT(0),
        IVA DECIMAL(18,2) NOT NULL DEFAULT(0),
        IVA_5 DECIMAL(18,2) NOT NULL DEFAULT(0),
        IVA_19 DECIMAL(18,2) NOT NULL DEFAULT(0),
        INC DECIMAL(18,2) NOT NULL DEFAULT(0),
        INCBolsas DECIMAL(18,2) NOT NULL DEFAULT(0),
        otrosImpuestos DECIMAL(18,2) NOT NULL DEFAULT(0),
        ivaVenta DECIMAL(18,2) NOT NULL DEFAULT(0),
        totalVenta DECIMAL(18,2) NOT NULL DEFAULT(0),
        total_A_Pagar DECIMAL(18,2) NOT NULL DEFAULT(0),
        totalPagadoVenta DECIMAL(18,2) NOT NULL DEFAULT(0),
        totalPendienteVenta DECIMAL(18,2) NOT NULL DEFAULT(0),
        costoTotalVenta DECIMAL(18,2) NOT NULL DEFAULT(0),
        utilidadTotalVenta DECIMAL(18,2) NOT NULL DEFAULT(0),
        propina DECIMAL(18,2) NOT NULL DEFAULT(0),

        idCliente INT NOT NULL DEFAULT(0),
        nit VARCHAR(100) NOT NULL DEFAULT('--'),
        nombreCliente VARCHAR(200) NOT NULL DEFAULT('--'),

        idBaseCaja INT NOT NULL DEFAULT(0),
        idUsuarioApertura INT NOT NULL DEFAULT(0),
        nombreUsuario VARCHAR(200) NOT NULL DEFAULT('-'),

        idVendedor INT NOT NULL DEFAULT(0),
        nombreVendedor VARCHAR(200) NOT NULL DEFAULT('-'),

        idVehiculo INT NOT NULL DEFAULT(0),
        placa VARCHAR(50) NOT NULL DEFAULT('-'),
        responsable VARCHAR(200) NOT NULL DEFAULT('-'),
        telefonoResponsable VARCHAR(50) NOT NULL DEFAULT('-'),

        nombremesa VARCHAR(500) NOT NULL DEFAULT('-'),
        nombreCD VARCHAR(200) NOT NULL DEFAULT('-'),

        cufe VARCHAR(200) NOT NULL DEFAULT('--'),
        estadoFE VARCHAR(50) NOT NULL DEFAULT('DENEGADA'),
        imagenQR VARCHAR(MAX) NULL
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_RebuildCajaVentaResumen
    @idVenta INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    ;WITH VentasObjetivo AS (
        SELECT t.id
        FROM dbo.TablaVentas AS t
        WHERE @idVenta IS NULL OR t.id = @idVenta
    ),
    DetalleAgr AS (
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
        INNER JOIN VentasObjetivo AS vo ON vo.id = dc.idVenta
        GROUP BY dc.idVenta
    ),
    CreditoAgr AS (
        SELECT
            pct.idVentaPago AS idVenta,
            SUM(ISNULL(pct.valor_pago_credito_tienda, 0)) AS totalCredito
        FROM dbo.PagosCreditoTienda AS pct
        INNER JOIN VentasObjetivo AS vo ON vo.id = pct.idVentaPago
        GROUP BY pct.idVentaPago
    ),
    ClienteRank AS (
        SELECT
            rvc.idVenta,
            rvc.idCliente,
            c.identificationNumber,
            c.nameCliente,
            ROW_NUMBER() OVER (PARTITION BY rvc.idVenta ORDER BY rvc.idCliente) AS rn
        FROM dbo.R_VentaCliente AS rvc
        INNER JOIN VentasObjetivo AS vo ON vo.id = rvc.idVenta
        LEFT JOIN dbo.Clientes AS c ON c.id = rvc.idCliente
    ),
    BaseCajaRank AS (
        SELECT
            rvb.idVenta,
            rvb.idBaseCaja,
            bc.idUsuarioApertura,
            ISNULL(u.nombreUsuario, '-') AS nombreUsuario,
            ROW_NUMBER() OVER (PARTITION BY rvb.idVenta ORDER BY rvb.idBaseCaja DESC) AS rn
        FROM dbo.R_VentaBase AS rvb
        INNER JOIN VentasObjetivo AS vo ON vo.id = rvb.idVenta
        INNER JOIN dbo.BaseCaja AS bc ON bc.id = rvb.idBaseCaja
        LEFT JOIN dbo.Usuario AS u ON u.id = bc.idUsuarioApertura
    ),
    VendedorRank AS (
        SELECT
            rvv.idVenta,
            rvv.idVendedor,
            ISNULL(v.nombreVendedor, '-') AS nombreVendedor,
            ROW_NUMBER() OVER (PARTITION BY rvv.idVenta ORDER BY rvv.idVendedor) AS rn
        FROM dbo.R_VentaVendedor AS rvv
        INNER JOIN VentasObjetivo AS vo ON vo.id = rvv.idVenta
        LEFT JOIN dbo.Vendedor AS v ON v.id = rvv.idVendedor
    ),
    VehiculoRank AS (
        SELECT
            rvm.idVenta,
            rvm.idVehiculo,
            ISNULL(vh.placa, '-') AS placa,
            ISNULL(vh.responsable, '-') AS responsable,
            ISNULL(vh.telefono, '-') AS telefono,
            ROW_NUMBER() OVER (PARTITION BY rvm.idVenta ORDER BY rvm.idVehiculo) AS rn
        FROM dbo.R_VehiculoVenta AS rvm
        INNER JOIN VentasObjetivo AS vo ON vo.id = rvm.idVenta
        LEFT JOIN dbo.Vehiculo AS vh ON vh.idVehiculo = rvm.idVehiculo
    ),
    MesasAgr AS (
        SELECT
            x.idVenta,
            STRING_AGG(x.nombreMesa, ', ') AS nombremesa
        FROM (
            SELECT DISTINCT
                rvm.idVenta,
                ISNULL(m.nombreMesa, '-') AS nombreMesa
            FROM dbo.R_VentaMesa AS rvm
            INNER JOIN VentasObjetivo AS vo ON vo.id = rvm.idVenta
            INNER JOIN dbo.Mesas AS m ON m.id = rvm.idMesa
        ) AS x
        GROUP BY x.idVenta
    ),
    DomicilioRank AS (
        SELECT
            r.idVenta,
            ISNULL(c.nombreCliente, '-') AS nombreCliente,
            ROW_NUMBER() OVER (PARTITION BY r.idVenta ORDER BY c.nombreCliente) AS rn
        FROM dbo.R_VentaClienteDomicilio AS r
        INNER JOIN VentasObjetivo AS vo ON vo.id = r.idVenta
        INNER JOIN dbo.ClienteDomicilio AS c ON c.id = r.idClienteDomicilio
    ),
    FacturaRank AS (
        SELECT
            fe.idVenta,
            fe.id,
            fe.cufe,
            fe.imagenQR,
            ROW_NUMBER() OVER (PARTITION BY fe.idVenta ORDER BY fe.id DESC) AS rn
        FROM dbo.FacturaElectronica AS fe
        INNER JOIN VentasObjetivo AS vo ON vo.id = fe.idVenta
    ),
    Resumen AS (
        SELECT
            t.id AS idVenta,
            CAST(SYSDATETIME() AS DATETIME2(0)) AS fechaActualizacion,
            CONVERT(DECIMAL(18,2), ISNULL(da.subtotalVenta, 0)) AS subtotalVenta,
            CONVERT(DECIMAL(18,2), ISNULL(da.basesIva, 0)) AS basesIva,
            CONVERT(DECIMAL(18,2), ISNULL(da.basesIva_5, 0)) AS basesIva_5,
            CONVERT(DECIMAL(18,2), ISNULL(da.basesIva_19, 0)) AS basesIva_19,
            CONVERT(DECIMAL(18,2), ISNULL(da.IVA, 0)) AS IVA,
            CONVERT(DECIMAL(18,2), ISNULL(da.IVA_5, 0)) AS IVA_5,
            CONVERT(DECIMAL(18,2), ISNULL(da.IVA_19, 0)) AS IVA_19,
            CONVERT(DECIMAL(18,2), ISNULL(da.INC, 0)) AS INC,
            CONVERT(DECIMAL(18,2), ISNULL(da.INCBolsas, 0)) AS INCBolsas,
            CONVERT(DECIMAL(18,2), ISNULL(da.otrosImpuestos, 0)) AS otrosImpuestos,
            CONVERT(DECIMAL(18,2), ISNULL(da.IVA, 0)) AS ivaVenta,
            CONVERT(DECIMAL(18,2), ISNULL(da.totalVenta, 0)) AS totalVenta,
            CONVERT(DECIMAL(18,2),
                ISNULL(da.totalVenta, 0)
                + CASE
                    WHEN ISNULL(t.propina, 0) = 0
                        THEN CONVERT(DECIMAL(18,2), ISNULL(da.subtotalVenta, 0)) * ISNULL(t.porpropina, 0)
                    ELSE ISNULL(t.propina, 0)
                  END
                - ISNULL(t.descuentoVenta, 0)
            ) AS total_A_Pagar,
            CONVERT(DECIMAL(18,2), ISNULL(t.abonoEfectivo, 0) + ISNULL(t.abonoTarjeta, 0) + ISNULL(ca.totalCredito, 0)) AS totalPagadoVenta,
            CONVERT(DECIMAL(18,2),
                (ISNULL(da.totalVenta, 0) + ISNULL(t.propina, 0))
                - (ISNULL(t.abonoEfectivo, 0) + ISNULL(t.abonoTarjeta, 0))
                - ISNULL(ca.totalCredito, 0)
            ) AS totalPendienteVenta,
            CONVERT(DECIMAL(18,2), ISNULL(da.costoTotalVenta, 0)) AS costoTotalVenta,
            CONVERT(DECIMAL(18,2), ISNULL(da.totalVenta, 0) + ROUND(ISNULL(da.impuestosUtilidad, 0), 0) - ISNULL(t.descuentoVenta, 0) - ISNULL(da.costoTotalVenta, 0)) AS utilidadTotalVenta,
            CONVERT(DECIMAL(18,2),
                CASE
                    WHEN ISNULL(t.propina, 0) = 0
                        THEN CONVERT(DECIMAL(18,2), ISNULL(da.subtotalVenta, 0)) * ISNULL(t.porpropina, 0)
                    ELSE ISNULL(t.propina, 0)
                END
            ) AS propina,
            ISNULL(cr.idCliente, 0) AS idCliente,
            ISNULL(cr.identificationNumber, '--') AS nit,
            ISNULL(cr.nameCliente, '--') AS nombreCliente,
            ISNULL(br.idBaseCaja, 0) AS idBaseCaja,
            ISNULL(br.idUsuarioApertura, 0) AS idUsuarioApertura,
            ISNULL(br.nombreUsuario, '-') AS nombreUsuario,
            ISNULL(vr.idVendedor, 0) AS idVendedor,
            ISNULL(vr.nombreVendedor, '-') AS nombreVendedor,
            ISNULL(vhr.idVehiculo, 0) AS idVehiculo,
            ISNULL(vhr.placa, '-') AS placa,
            ISNULL(vhr.responsable, '-') AS responsable,
            ISNULL(vhr.telefono, '-') AS telefonoResponsable,
            ISNULL(ma.nombremesa, '-') AS nombremesa,
            ISNULL(dr.nombreCliente, '-') AS nombreCD,
            ISNULL(fr.cufe, '--') AS cufe,
            CASE WHEN fr.id IS NOT NULL THEN 'ACEPTADA' ELSE 'DENEGADA' END AS estadoFE,
            fr.imagenQR
        FROM dbo.TablaVentas AS t
        INNER JOIN VentasObjetivo AS vo ON vo.id = t.id
        LEFT JOIN DetalleAgr AS da ON da.idVenta = t.id
        LEFT JOIN CreditoAgr AS ca ON ca.idVenta = t.id
        LEFT JOIN ClienteRank AS cr ON cr.idVenta = t.id AND cr.rn = 1
        LEFT JOIN BaseCajaRank AS br ON br.idVenta = t.id AND br.rn = 1
        LEFT JOIN VendedorRank AS vr ON vr.idVenta = t.id AND vr.rn = 1
        LEFT JOIN VehiculoRank AS vhr ON vhr.idVenta = t.id AND vhr.rn = 1
        LEFT JOIN MesasAgr AS ma ON ma.idVenta = t.id
        LEFT JOIN DomicilioRank AS dr ON dr.idVenta = t.id AND dr.rn = 1
        LEFT JOIN FacturaRank AS fr ON fr.idVenta = t.id AND fr.rn = 1
    )
    MERGE dbo.CajaVentaResumen AS target
    USING Resumen AS source
       ON target.idVenta = source.idVenta
    WHEN MATCHED THEN
        UPDATE SET
            fechaActualizacion = source.fechaActualizacion,
            subtotalVenta = source.subtotalVenta,
            basesIva = source.basesIva,
            basesIva_5 = source.basesIva_5,
            basesIva_19 = source.basesIva_19,
            IVA = source.IVA,
            IVA_5 = source.IVA_5,
            IVA_19 = source.IVA_19,
            INC = source.INC,
            INCBolsas = source.INCBolsas,
            otrosImpuestos = source.otrosImpuestos,
            ivaVenta = source.ivaVenta,
            totalVenta = source.totalVenta,
            total_A_Pagar = source.total_A_Pagar,
            totalPagadoVenta = source.totalPagadoVenta,
            totalPendienteVenta = source.totalPendienteVenta,
            costoTotalVenta = source.costoTotalVenta,
            utilidadTotalVenta = source.utilidadTotalVenta,
            propina = source.propina,
            idCliente = source.idCliente,
            nit = source.nit,
            nombreCliente = source.nombreCliente,
            idBaseCaja = source.idBaseCaja,
            idUsuarioApertura = source.idUsuarioApertura,
            nombreUsuario = source.nombreUsuario,
            idVendedor = source.idVendedor,
            nombreVendedor = source.nombreVendedor,
            idVehiculo = source.idVehiculo,
            placa = source.placa,
            responsable = source.responsable,
            telefonoResponsable = source.telefonoResponsable,
            nombremesa = source.nombremesa,
            nombreCD = source.nombreCD,
            cufe = source.cufe,
            estadoFE = source.estadoFE,
            imagenQR = source.imagenQR
    WHEN NOT MATCHED THEN
        INSERT
        (
            idVenta, fechaActualizacion, subtotalVenta, basesIva, basesIva_5, basesIva_19,
            IVA, IVA_5, IVA_19, INC, INCBolsas, otrosImpuestos, ivaVenta, totalVenta,
            total_A_Pagar, totalPagadoVenta, totalPendienteVenta, costoTotalVenta,
            utilidadTotalVenta, propina, idCliente, nit, nombreCliente, idBaseCaja,
            idUsuarioApertura, nombreUsuario, idVendedor, nombreVendedor, idVehiculo,
            placa, responsable, telefonoResponsable, nombremesa, nombreCD, cufe,
            estadoFE, imagenQR
        )
        VALUES
        (
            source.idVenta, source.fechaActualizacion, source.subtotalVenta, source.basesIva, source.basesIva_5, source.basesIva_19,
            source.IVA, source.IVA_5, source.IVA_19, source.INC, source.INCBolsas, source.otrosImpuestos, source.ivaVenta, source.totalVenta,
            source.total_A_Pagar, source.totalPagadoVenta, source.totalPendienteVenta, source.costoTotalVenta,
            source.utilidadTotalVenta, source.propina, source.idCliente, source.nit, source.nombreCliente, source.idBaseCaja,
            source.idUsuarioApertura, source.nombreUsuario, source.idVendedor, source.nombreVendedor, source.idVehiculo,
            source.placa, source.responsable, source.telefonoResponsable, source.nombremesa, source.nombreCD, source.cufe,
            source.estadoFE, source.imagenQR
        );

    IF @idVenta IS NULL
    BEGIN
        DELETE r
        FROM dbo.CajaVentaResumen AS r
        WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.TablaVentas AS t
            WHERE t.id = r.idVenta
        );
    END
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_RebuildCajaVentaResumen_All
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_RebuildCajaVentaResumen @idVenta = NULL;
END;
GO

CREATE OR ALTER VIEW dbo.V_CajaVentaResumen
AS
SELECT *
FROM dbo.CajaVentaResumen;
GO
