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
- **I**: interfaces chicas; cada servicio depende solo de lo que usa. `SumarPorBilleteraAsync` se conserva sin llamadas como herramienta para conciliar el saldo con el ledger.

## Otros cambios
- Un repositorio por entidad y una unidad de trabajo que maneja la transaccion. Ahi se convierten los errores de concurrencia de EF en excepciones del dominio (termina en el 409).
- Huecos del enunciado: auditoria de todas las acciones (activacion y cierre por worker, cierre desierta, rechazos de puja, acreditacion manual), anti-sniping configurable y reintento del worker ante conflicto.
- Limpieza de codigo muerto y estilo segun la guia de buenas practicas.

## Verificado
Compilo y corrio en la maquina de Alexis: migracion, seed, worker (cierre de subasta con ganador) y catalogo. 112 chequeos automaticos, prueba de inyeccion de dependencias y ESLint sin errores.

## Verificado en ejecucion (16/10)
Prueba de concurrencia (`scripts/prueba-concurrencia.ps1`): una oferta aceptada y el resto rechazadas por ronda. Auditoria con `PUJA_RECHAZADA_CONCURRENCIA`, `PUJA_RECHAZADA_VALIDACION` y `ACTIVACION_WORKER`. Saldo total conciliado con el ledger y saldo retenido igual a las pujas lideres, en todas las billeteras.

## Pendiente
Las pruebas manuales de `PRUEBAS-MANUALES.md` desde el navegador (pujas, superada, saldo insuficiente, anti-sniping, cierre y avisos de `puja.js`) las hace Pablo.
