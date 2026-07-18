**Fase 3**

Archivo SQL:
[2026-07-03_fase3_cache_caja.sql](/D:/POS_SQL_2025/ComandasVentas/Database/Performance/2026-07-03_fase3_cache_caja.sql)

**Idea**

Esta fase no intenta exprimir un poco mas las vistas.
Esta fase crea una capa nueva:

- `CajaVentaResumen`
- `sp_RebuildCajaVentaResumen`
- `sp_RebuildCajaVentaResumen_All`
- `V_CajaVentaResumen`

Con eso puedes materializar por venta los datos que caja recalcula todo el tiempo.

**Beneficio esperado**

- Menor carga al cambiar entre cuentas
- Menor tiempo de respuesta cuando haya muchas ventas activas
- Base lista para una siguiente mejora donde `V_TablaVentas` o la app lean desde el cache

**Como probar**

1. Ejecutar:
   [2026-07-03_fase3_cache_caja.sql](/D:/POS_SQL_2025/ComandasVentas/Database/Performance/2026-07-03_fase3_cache_caja.sql)

2. Reconstruir cache completo:

```sql
EXEC dbo.sp_RebuildCajaVentaResumen_All;
```

3. Revisar:

```sql
SELECT TOP 50 *
FROM dbo.V_CajaVentaResumen
ORDER BY fechaActualizacion DESC, idVenta DESC;
```

4. Comparar algunos `idVenta` contra `V_TablaVentas`:

```sql
SELECT v.id, v.totalVenta, v.total_A_Pagar, v.totalPendienteVenta, v.nombreCliente
FROM dbo.V_TablaVentas v
WHERE v.id IN (/* ids de prueba */);

SELECT r.idVenta, r.totalVenta, r.total_A_Pagar, r.totalPendienteVenta, r.nombreCliente
FROM dbo.V_CajaVentaResumen r
WHERE r.idVenta IN (/* ids de prueba */);
```

**Como usarlo despues**

Si los datos coinciden bien, el siguiente paso seria una fase 3.1:

- cambiar `V_TablaVentas` para que tome primero desde `CajaVentaResumen`
- o cambiar `caja.aspx` para consultar directo `V_CajaVentaResumen`

Ahí sí aparecería un salto aun más fuerte de rendimiento.

**Observacion**

Esta fase 3 deja la infraestructura lista pero no cambia todavia el consumo de la app.
La mejora inmediata visible vendra cuando la siguiente fase haga que las vistas o la app lean este cache.
