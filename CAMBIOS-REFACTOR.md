# Que cambio en este refactor

Comparado con `main` de Pabl0ScB/SubastaYa (commit `bb9449f`). La base de datos y la migracion no cambian. La API JSON y las rutas solo suman: campos `fechaServidor`, `cantidadOfertas`, `montoMinimo` y `efecto`, y el endpoint `GET /config/auctions`. Nada de lo que el original devolvia se quito ni se renombro.

## Antes
- Controllers finos, pero los servicios vivian dentro de la Api y usaban el `DbContext` directo.
- `Subasta` solo con propiedades; las reglas estaban en los servicios.
- Solo 2 repositorios.
- Los servicios atrapaban las excepciones de EF y Npgsql (concurrencia, restricciones) y avisaban por `IHubContext`.

## Ahora: 4 proyectos
| Proyecto | Contiene | Depende de |
|---|---|---|
| `Domain` | entidades con sus reglas, interfaces de repositorio, `IUnitOfWork`, excepciones | nada |
| `Application` | servicios, DTOs, interfaces de consultas, del notificador y del reloj | Domain |
| `Infrastructure` | EF Core, repositorios, UnitOfWork, consultas, seed, BCrypt | Application, Domain |
| `Api` | controllers, SignalR, worker, middleware, Swagger, `Program.cs` | todos |

La Api y Application no usan `DbContext`. Domain y Application no tienen EF.

## Responsabilidad de cada capa
- Controllers y hub: reciben, delegan y devuelven el codigo HTTP. Sin reglas ni calculos.
- Servicios (escrituras): reglas, validaciones y transacciones.
- Consultas (lecturas): interfaces `IConsultas*` en Application, implementadas en Infrastructure. Solo traen datos; no hay servicios que reenvien.
- Repositorios: solo persistencia. Reciben una especificacion de `Subasta` (`ParaActivar`, `Vencida`) en vez de decidir ellos.
- Dominio: `Subasta.CalcularMontoMinimo` es la unica formula del minimo; las especificaciones definen cuando una subasta se activa o vence.
- Tiempo: `IReloj` da la hora del servidor a validador, registrador, servicios y consultas.

## SOLID
- **S**: los servicios grandes se partieron en piezas de una sola tarea: `ValidadorDePujas`, `RegistradorDePujas`, `CierreDeSubastas`, `ActivadorDeSubastas`, `ServicioDeAdjudicacion`, `ServicioDeConfiguracion`.
- **D**: Application depende de interfaces; Infrastructure las implementa; todo entra por inyeccion.
- **I**: interfaces chicas; cada servicio depende solo de lo que usa. `SumarPorBilleteraAsync` se conserva sin llamadas como herramienta para conciliar el saldo con el ledger.

## Se conserva del original
El middleware global de excepciones (misma tabla 404/422/403/409/500), el `SubastaHub` (solo grupos), el worker, el anti-sniping configurable y el reintento del worker.

## Otros cambios
- Un repositorio por entidad y una unidad de trabajo que maneja la transaccion. Ahi se convierten los errores de concurrencia y de unicidad de EF/Npgsql en excepciones del dominio (terminan en 409); los servicios ya no los atrapan.
- Cliente tonto: el frontend no calcula el minimo, no decide si una subasta vencio, no suma ofertas ni valida limites de publicacion. Usa lo que manda el back (`montoMinimo`, `fechaServidor`, `cantidadOfertas`, `/config/auctions`) y muestra el 422 o 409 de la API.
- Auditoria segun la seccion 3.4 del enunciado: cambios de estado (activacion y cierre por el worker, cierre desierta), extensiones anti-sniping, rechazos de puja y acreditacion manual. La unica accion nueva respecto del original es ACTIVACION_WORKER (pasa de Programada a Activa).
- Contrasenas de PostgreSQL y pgAdmin fuera del repo (`.env`), puertos publicados solo en 127.0.0.1, Swagger y datos de prueba solo en Development.
- Limpieza de codigo muerto y estilo segun la guia "Buenas practicas Java y C#" de la catedra (por ejemplo, indentacion con tabs de 4 espacios).

## Verificado
- `dotnet test`: 15 pruebas de dominio.
- `scripts/verificar.ps1`: 36 chequeos contra la API levantada (codigos HTTP, reglas rechazadas con 422, concurrencia, saldos, contrato nuevo y que el frontend no tenga logica propia). Crea sus propios usuarios y subastas: conviene resetear la base antes de una demo.
- `scripts/prueba-concurrencia.ps1`: una oferta aceptada y el resto rechazadas por ronda. Saldo total conciliado con el ledger y saldo retenido igual a las pujas lideres.

## Pruebas automaticas
El proyecto `tests/SubastaYa.Domain.Tests` prueba las reglas de `Subasta` sin base de datos (anti-sniping, monto minimo, versiones). Se corre con `dotnet test` y tambien en GitHub Actions (`.github/workflows/ci.yml`).

## Pendiente
Las pruebas de interfaz de `PRUEBAS-MANUALES.md` desde el navegador (superada, saldo insuficiente, anti-sniping, cierre y avisos en vivo) las hace Pablo. Lo que se puede automatizar ya lo cubren los scripts.
