**Hallazgos**

El cuello principal de `caja.aspx` no esta solo en C#. En la base de datos hay 2 niveles de costo:

1. `V_DetalleCaja` recalcula datos por cada fila de `DetalleVenta`.
2. `V_TablaVentas` vuelve a sumar muchas veces sobre `V_DetalleCaja` para la misma venta.

Eso hace que abrir una cuenta, seleccionar una cuenta o refrescar una venta termine ejecutando muchas sumas repetidas.

**Objetos mas sensibles**

- `V_DetalleCaja`
  Archivo fuente: `E:\DB MAESTRA\VISTAS.sql`
  Riesgo: usa `OUTER APPLY` contra `CargosDescuentosDetalleVenta`, `V_Presentacion`, `v_productoVenta` y `R_CuentaCliente_DetalleVenta` por cada detalle.

- `V_TablaVentas`
  Archivo fuente: `E:\DB MAESTRA\VISTAS.sql`
  Riesgo: hace muchas subconsultas independientes a `V_DetalleCaja` para la misma venta:
  `subtotalVenta`, `basesIva`, `IVA`, `IVA_5`, `IVA_19`, `INC`, `INCBolsas`, `otrosImpuestos`, `totalVenta`, `total_A_Pagar`, `totalPendienteVenta`, `costoTotalVenta`, `utilidadTotalVenta`, `propina`.

- `V_CuentasVenta`
  Archivo fuente: `E:\DB MAESTRA\VISTAS.sql`
  Riesgo: depende de `V_DetalleCaja`, `R_VentaBase`, `R_VentaVendedor`, `R_VentaCliente`, relaciones de mesa y domicilio.

- `V_CuentaCliente`
  Archivo fuente: `E:\DB MAESTRA\VISTAS.sql`
  Riesgo: agrega por `idVenta` e `idCuentaCliente` usando `V_DetalleCaja`.

**Tablas que mas necesitan indices**

- `DetalleVenta`
- `CuentaCliente`
- `R_CuentaCliente_DetalleVenta`
- `CargosDescuentosDetalleVenta`
- `R_VentaMesa`
- `R_VentaVendedor`
- `R_VentaBase`
- `R_VentaCliente`
- `R_VentaClienteDomicilio`
- `TablaVentas`
- `PagosCreditoTienda`
- `FacturaElectronica`
- `Mesas`
- `ClienteDomicilio`

**Accion recomendada**

1. Ejecutar primero `2026-07-02_caja_indexes.sql` en QA.
2. Medir tiempos de `caja.aspx`.
3. Si aun sigue pesado, segunda fase:
   reescribir `V_TablaVentas` para que agregue `V_DetalleCaja` una sola vez por venta, en lugar de repetir subconsultas.

**Prioridad de mejora**

1. Indices.
2. Reescritura de `V_TablaVentas`.
3. Reescritura de `V_CuentasVenta` si todavia hay lentitud.
