/*
Objetivo:
- Mejorar los filtros mas frecuentes del flujo de caja.aspx
- Reducir scans sobre tablas de detalle, relaciones y ventas pendientes

Revisar primero en QA o una copia de produccion.
Si algun indice ya existe con otro nombre o misma llave, ajustarlo antes de ejecutar.
*/

SET NOCOUNT ON;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_DetalleVenta_IdVenta_EstadoDetalle'
      AND object_id = OBJECT_ID('dbo.DetalleVenta')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_DetalleVenta_IdVenta_EstadoDetalle
    ON dbo.DetalleVenta (idVenta, estadoDetalle)
    INCLUDE (idPresentacion, nombreProducto, precioVenta, cantidadDetalle, codigoProducto, observacion, impuesto_id, guidDetalle, opciones, adiciones);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_CuentaCliente_IdVenta_Eliminada'
      AND object_id = OBJECT_ID('dbo.CuentaCliente')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_CuentaCliente_IdVenta_Eliminada
    ON dbo.CuentaCliente (idVenta, eliminada)
    INCLUDE (fecha, nombreCuenta, preCuenta, por_propina, propina);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_R_VentaMesa_IdVenta_IdMesa'
      AND object_id = OBJECT_ID('dbo.R_VentaMesa')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_R_VentaMesa_IdVenta_IdMesa
    ON dbo.R_VentaMesa (idVenta, idMesa);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_R_VentaMesa_IdMesa'
      AND object_id = OBJECT_ID('dbo.R_VentaMesa')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_R_VentaMesa_IdMesa
    ON dbo.R_VentaMesa (idMesa)
    INCLUDE (idVenta);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_R_VentaVendedor_IdVendedor_IdVenta'
      AND object_id = OBJECT_ID('dbo.R_VentaVendedor')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_R_VentaVendedor_IdVendedor_IdVenta
    ON dbo.R_VentaVendedor (idVendedor, idVenta);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_R_VentaBase_IdVenta_IdBaseCaja'
      AND object_id = OBJECT_ID('dbo.R_VentaBase')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_R_VentaBase_IdVenta_IdBaseCaja
    ON dbo.R_VentaBase (idVenta, idBaseCaja);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_R_VentaCliente_IdVenta_IdCliente'
      AND object_id = OBJECT_ID('dbo.R_VentaCliente')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_R_VentaCliente_IdVenta_IdCliente
    ON dbo.R_VentaCliente (idVenta, idCliente);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_R_VehiculoVenta_IdVenta_IdVehiculo'
      AND object_id = OBJECT_ID('dbo.R_VehiculoVenta')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_R_VehiculoVenta_IdVenta_IdVehiculo
    ON dbo.R_VehiculoVenta (idVenta, idVehiculo);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_R_VentaClienteDomicilio_IdVenta'
      AND object_id = OBJECT_ID('dbo.R_VentaClienteDomicilio')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_R_VentaClienteDomicilio_IdVenta
    ON dbo.R_VentaClienteDomicilio (idVenta)
    INCLUDE (idClienteDomicilio);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_R_CuentaCliente_DetalleVenta_IdDetalleVenta'
      AND object_id = OBJECT_ID('dbo.R_CuentaCliente_DetalleVenta')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_R_CuentaCliente_DetalleVenta_IdDetalleVenta
    ON dbo.R_CuentaCliente_DetalleVenta (idDetalleVenta)
    INCLUDE (idCuentaCliente, eliminada, fecha);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_CargosDescuentosDetalleVenta_IdDetalleVenta'
      AND object_id = OBJECT_ID('dbo.CargosDescuentosDetalleVenta')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_CargosDescuentosDetalleVenta_IdDetalleVenta
    ON dbo.CargosDescuentosDetalleVenta (idDetalleVenta)
    INCLUDE (valorDescuento);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PagosCreditoTienda_IdVentaPago'
      AND object_id = OBJECT_ID('dbo.PagosCreditoTienda')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_PagosCreditoTienda_IdVentaPago
    ON dbo.PagosCreditoTienda (idVentaPago)
    INCLUDE (valor_pago_credito_tienda);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_TablaVentas_IdBaseCaja_NumeroVenta_Eliminada'
      AND object_id = OBJECT_ID('dbo.TablaVentas')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_TablaVentas_IdBaseCaja_NumeroVenta_Eliminada
    ON dbo.TablaVentas (idBaseCaja, numeroVenta, eliminada)
    INCLUDE (fechaVenta, aliasVenta, descuentoVenta, efectivoVenta, cambioVenta, estadoVenta, observacionVenta, abonoTarjeta, abonoEfectivo, idMedioDePago, idResolucion, idFormaDePago, propina, porpropina);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_FacturaElectronica_IdVenta'
      AND object_id = OBJECT_ID('dbo.FacturaElectronica')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_FacturaElectronica_IdVenta
    ON dbo.FacturaElectronica (idVenta);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Mesas_IdZona_EstadoMesa'
      AND object_id = OBJECT_ID('dbo.Mesas')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Mesas_IdZona_EstadoMesa
    ON dbo.Mesas (idZona, estadoMesa)
    INCLUDE (nombreMesa, guidMesa, widthMesa);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_ClienteDomicilio_CelularCliente'
      AND object_id = OBJECT_ID('dbo.ClienteDomicilio')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_ClienteDomicilio_CelularCliente
    ON dbo.ClienteDomicilio (celularCliente);
END;
GO
