**Fase 2.2**

Archivo SQL:
[2026-07-03_fase2_2_vistas_caja.sql](/D:/POS_SQL_2025/ComandasVentas/Database/Performance/2026-07-03_fase2_2_vistas_caja.sql)

**Que mejora**

- `V_DetalleCaja`
  Antes:
  usaba `OUTER APPLY` y `EXISTS` por cada fila de `DetalleVenta`.

  Ahora:
  usa agregaciones previas para:
  - descuentos por detalle
  - estado de comanda por detalle
  - cuenta cliente asociada por detalle

  Luego hace `LEFT JOIN` directos, que normalmente SQL Server resuelve mejor.

**Impacto esperado**

- Mejor recarga del detalle de venta
- Mejor tiempo al seleccionar cuenta
- Mejor respuesta al recalcular totales y subtotales
- Beneficio indirecto para `V_TablaVentas`, `V_CuentaCliente` y `V_CuentasVenta`

**Orden recomendado**

1. Tener aplicada la fase 2:
   [2026-07-03_fase2_vistas_caja.sql](/D:/POS_SQL_2025/ComandasVentas/Database/Performance/2026-07-03_fase2_vistas_caja.sql)

2. Tener aplicada la fase 2.1:
   [2026-07-03_fase2_1_vistas_caja.sql](/D:/POS_SQL_2025/ComandasVentas/Database/Performance/2026-07-03_fase2_1_vistas_caja.sql)

3. Aplicar esta fase 2.2:
   [2026-07-03_fase2_2_vistas_caja.sql](/D:/POS_SQL_2025/ComandasVentas/Database/Performance/2026-07-03_fase2_2_vistas_caja.sql)

**Validaciones**

- items comandados
- nombre de cuenta cliente por detalle
- descuentos por detalle
- subtotales
- impuestos
- total detalle
- costo total

**Nota**

Si despues de esta fase aun quieres mas velocidad, la siguiente mejora fuerte ya seria una fase 3:
- procedimientos especializados para caja
- o tabla/materializacion auxiliar de agregados por venta
