# Meseros.Blazor

Proyecto base para iniciar la migracion de `WebApplication` (ASP.NET Web Forms) a Blazor sobre .NET 8.

## Restriccion actual

`Meseros.Blazor` no puede referenciar directamente:

- `DAL`
- `3-RFacturacionElectronicaDIAN`

La causa es que ambos proyectos actuales apuntan a `.NET Framework 4.8`, mientras que Blazor corre sobre `.NET 8`.

## Estrategia recomendada

1. Extraer contratos, DTOs y logica reutilizable a una libreria compatible con `.NET 8` o `.NET Standard 2.0`.
2. Exponer la logica existente mediante una API intermedia si la migracion del DAL no puede hacerse de inmediato.
3. Migrar por modulos, no por paginas aisladas.

## Orden de migracion sugerido

1. `Menu`
2. `Caja`
3. `Cobrar`
4. `CerrarCaja`
5. `HVentas`
6. `PGastos`

## Resultado de esta fase

- Proyecto Blazor agregado a la solucion.
- Navegacion inicial creada.
- Pantallas semilla por modulo creadas.
- Web Forms actual mantenido sin cambios funcionales.
