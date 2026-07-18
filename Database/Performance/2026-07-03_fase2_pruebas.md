**Fase 2**

Archivo SQL:
[2026-07-03_fase2_vistas_caja.sql](/D:/POS_SQL_2025/ComandasVentas/Database/Performance/2026-07-03_fase2_vistas_caja.sql)

**Que cambia**

- `V_TablaVentas`
  Antes: hacia muchas subconsultas separadas a `V_DetalleCaja` por cada venta.
  Ahora: agrega una sola vez por `idVenta` y reutiliza esos resultados.

- `V_CuentaCliente`
  Antes: dependia de `V_TablaVentas` aunque no necesitaba sus columnas para calcular subtotal, iva y total.
  Ahora: calcula directo desde la agregacion de `V_DetalleCaja`.

**Beneficio esperado**

- Menos lecturas repetidas por cada refresco de `caja.aspx`
- Mejor tiempo al crear servicio, cambiar cuenta y recalcular venta
- Menor carga global sobre la base de prueba

**Como probar**

1. Respaldar las vistas actuales en SSMS.
2. Ejecutar `2026-07-03_fase2_vistas_caja.sql` en la base de pruebas.
3. Probar:
   - abrir `caja.aspx`
   - crear servicio
   - seleccionar cuenta
   - mover entre mesas
   - abrir domicilio
4. Validar que los valores de total, pendiente, propina, iva y cliente sigan correctos.

**Cuidado funcional**

- `totalPendienteVenta` se dejo con la misma logica de la vista actual para no cambiar comportamiento.
- Si despues quieres, hacemos una fase 2.1 para revisar si esa formula debe corregirse funcionalmente.
