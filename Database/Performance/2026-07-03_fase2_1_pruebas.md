**Fase 2.1**

Archivo SQL:
[2026-07-03_fase2_1_vistas_caja.sql](/D:/POS_SQL_2025/ComandasVentas/Database/Performance/2026-07-03_fase2_1_vistas_caja.sql)

**Que mejora**

- `V_CuentasVenta`
  Antes:
  hacia varios `OUTER APPLY` por cada venta.

  Ahora:
  usa CTEs con `ROW_NUMBER` y agregaciones previas por `idVenta`, para que SQL Server resuelva las relaciones una sola vez por venta.

- `V_ImprecionComandaAdd`
  Antes:
  hacia dos subconsultas separadas a `V_CuentasVenta`.

  Ahora:
  hace un solo `LEFT JOIN`.

**Impacto esperado**

- Mejor cambio entre cuentas
- Mejor carga del listado de cuentas
- Menor costo cuando caja recarga el estado de las mesas y cuentas

**Como probar**

1. Ejecutar antes:
   [2026-07-03_fase2_vistas_caja.sql](/D:/POS_SQL_2025/ComandasVentas/Database/Performance/2026-07-03_fase2_vistas_caja.sql)

2. Luego ejecutar:
   [2026-07-03_fase2_1_vistas_caja.sql](/D:/POS_SQL_2025/ComandasVentas/Database/Performance/2026-07-03_fase2_1_vistas_caja.sql)

3. Validar:
   - nombres de mesero
   - nombres de mesa
   - cliente domicilio
   - totales de cuenta
   - impresion de comanda

**Indice adicional**

Tambien se agrego recomendacion de indice para:
- `R_VehiculoVenta (idVenta, idVehiculo)`

Ese indice ya quedo agregado en:
[2026-07-02_caja_indexes.sql](/D:/POS_SQL_2025/ComandasVentas/Database/Performance/2026-07-02_caja_indexes.sql)
