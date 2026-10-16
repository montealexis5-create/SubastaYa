# Que cambio en este refactor

Comparado con `main` de Pabl0ScB/SubastaYa (commit `bb9449f`). La base de datos, la migracion, la API JSON, las rutas y el frontend no cambian de contrato.

## Antes
- Controllers finos, pero los servicios vivian dentro de la Api y usaban el `DbContext` directo.
- Solo 2 repositorios.

## Ahora: 4 proyectos
| Proyecto | Contiene | Depende de |
|---|---|---|
| `Domain` | entidades, interfaces de repositorio, `IUnitOfWork`, excepciones | nada |
| `Application` | servicios, DTOs, interfaces de consultas y del notificador | Domain |
| `Infrastructure` | EF Core, repositorios, UnitOfWork, consultas, seed, BCrypt | Application, Domain |
| `Api` | controllers, SignalR, worker, middleware, Swagger, `Program.cs` | todos |

La Api y Application no usan `DbContext`. Domain y Application no tienen EF.

## SOLID
- **S**: los servicios grandes se partieron en piezas de una sola tarea: `ValidadorDePujas`, `RegistradorDePujas`, `CierreDeSubastas`, `ActivadorDeSubastas`, `ServicioDeAdjudicacion`.
- **D**: Application depende de interfaces; Infrastructure las implementa; todo entra por inyeccion.
- **I**: interfaces chicas, sin metodos que nadie use.

## Otros cambios
- Un repositorio por entidad y una unidad de trabajo que maneja la transaccion. Ahi se convierten los errores de concurrencia de EF en excepciones del dominio (termina en el 409).
- Huecos del enunciado: auditoria de todas las acciones (activacion y cierre por worker, cierre desierta, rechazos de puja, acreditacion manual), anti-sniping configurable y reintento del worker ante conflicto.
- Limpieza de codigo muerto y estilo segun la guia de buenas practicas.

## Verificado
Compilo y corrio en la maquina de Alexis: migracion, seed, worker (cierre de subasta con ganador) y catalogo. 112 chequeos automaticos, prueba de inyeccion de dependencias y ESLint sin errores.

## Pendiente
Las pruebas manuales de `PRUEBAS-MANUALES.md` no se hicieron todavia.
