/*
FASE 2.1 - Optimizacion adicional para caja.aspx

Objetivo:
- Hacer mas liviana V_CuentasVenta
- Reducir dependencias repetidas por venta
- Quitar subconsultas por fila en V_ImprecionComandaAdd

Probar primero en base de pruebas.
*/

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER VIEW [dbo].[V_CuentasVenta]
AS
WITH TotalDetalle AS (
    SELECT
        dc.idVenta,
        SUM(dc.totalDetalle) AS total
    FROM dbo.V_DetalleCaja AS dc
    GROUP BY dc.idVenta
),
BaseCajaRank AS (
    SELECT
        rvb.idVenta,
        rvb.idBaseCaja,
        bc.idUsuarioApertura,
        ISNULL(u.nombreUsuario, '-') AS nombreUsuario,
        ROW_NUMBER() OVER (PARTITION BY rvb.idVenta ORDER BY rvb.idBaseCaja DESC) AS rn
    FROM dbo.R_VentaBase AS rvb
    INNER JOIN dbo.BaseCaja AS bc
        ON bc.id = rvb.idBaseCaja
    LEFT JOIN dbo.Usuario AS u
        ON u.id = bc.idUsuarioApertura
),
VendedorRank AS (
    SELECT
        rvv.idVenta,
        rvv.idVendedor,
        ISNULL(v.nombreVendedor, '-') AS nombreVendedor,
        ROW_NUMBER() OVER (PARTITION BY rvv.idVenta ORDER BY rvv.idVendedor) AS rn
    FROM dbo.R_VentaVendedor AS rvv
    LEFT JOIN dbo.Vendedor AS v
        ON v.id = rvv.idVendedor
),
ClienteRank AS (
    SELECT
        rvc.idVenta,
        rvc.idCliente,
        ISNULL(c.nameCliente, '-') AS nombreCliente,
        ROW_NUMBER() OVER (PARTITION BY rvc.idVenta ORDER BY rvc.idCliente) AS rn
    FROM dbo.R_VentaCliente AS rvc
    LEFT JOIN dbo.Clientes AS c
        ON c.id = rvc.idCliente
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
    LEFT JOIN dbo.Vehiculo AS vh
        ON vh.idVehiculo = rvm.idVehiculo
),
MesasAgr AS (
    SELECT
        vm.idVenta,
        STRING_AGG(vm.nombreMesa, ', ') AS nombremesa
    FROM (
        SELECT DISTINCT
            rvm.idVenta,
            ISNULL(m.nombreMesa, '-') AS nombreMesa
        FROM dbo.R_VentaMesa AS rvm
        INNER JOIN dbo.Mesas AS m
            ON m.id = rvm.idMesa
    ) AS vm
    GROUP BY vm.idVenta
),
DomicilioRank AS (
    SELECT
        r.idVenta,
        ISNULL(c.nombreCliente, '-') AS nombreCliente,
        ROW_NUMBER() OVER (PARTITION BY r.idVenta ORDER BY c.nombreCliente) AS rn
    FROM dbo.R_VentaClienteDomicilio AS r
    INNER JOIN dbo.ClienteDomicilio AS c
        ON c.id = r.idClienteDomicilio
)
SELECT
    ISNULL(tv.id, 0) AS id,
    ISNULL(tv.aliasVenta, '-') AS aliasVenta,
    ISNULL(tv.efectivoVenta, 0) AS efectivoVenta,
    ISNULL(tv.numeroVenta, 0) AS numeroVenta,
    ISNULL(tv.eliminada, 0) AS eliminada,
    ISNULL(td.total, 0) AS total,
    ISNULL(bc.idBaseCaja, 0) AS idbase,
    ISNULL(bc.idUsuarioApertura, 0) AS idusuario,
    ISNULL(bc.nombreUsuario, '-') AS numbreUnuario,
    ISNULL(vv.idVendedor, 0) AS idvendedor,
    ISNULL(vv.nombreVendedor, '-') AS nombrevendedor,
    ISNULL(cl.idCliente, 0) AS idcliente,
    ISNULL(cl.nombreCliente, '-') AS nombrecliente,
    ISNULL(vh.idVehiculo, 0) AS idVehiculo,
    ISNULL(vh.placa, '-') AS placa,
    ISNULL(vh.responsable, '-') AS responsable,
    ISNULL(vh.telefono, '-') AS telefonoResponsable,
    ISNULL(ma.nombremesa, '-') AS nombremesa,
    ISNULL(dm.nombreCliente, '-') AS nombreCD
FROM dbo.TablaVentas AS tv
LEFT JOIN TotalDetalle AS td
    ON td.idVenta = tv.id
LEFT JOIN BaseCajaRank AS bc
    ON bc.idVenta = tv.id
   AND bc.rn = 1
LEFT JOIN VendedorRank AS vv
    ON vv.idVenta = tv.id
   AND vv.rn = 1
LEFT JOIN ClienteRank AS cl
    ON cl.idVenta = tv.id
   AND cl.rn = 1
LEFT JOIN VehiculoRank AS vh
    ON vh.idVenta = tv.id
   AND vh.rn = 1
LEFT JOIN MesasAgr AS ma
    ON ma.idVenta = tv.id
LEFT JOIN DomicilioRank AS dm
    ON dm.idVenta = tv.id
   AND dm.rn = 1;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER VIEW [dbo].[V_ImprecionComandaAdd]
AS
SELECT
    ic.id,
    ic.idVenta,
    ic.idMesa,
    ISNULL(vc.nombremesa, '-') AS nombreMesa,
    ic.idMesero,
    ISNULL(vc.nombrevendedor, '-') AS nombreMesero,
    ic.estado
FROM dbo.ImprecionComandaAdd AS ic
LEFT JOIN dbo.V_CuentasVenta AS vc
    ON vc.id = ic.idVenta;
GO
