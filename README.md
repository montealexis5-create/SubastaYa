# SubastaYa

Esta es una plataforma web de subastas en tiempo real con billetera virtual y saldo en
garantía (escrow).

Trabajo Práctico de la cátedra **Proyecto de Software** — Carrera de Ingeniería
en Informática, Instituto de Ingeniería y Agronomía. **Universidad Nacional Arturo Jauretche**

## ⏭ Integrantes del proyecto ⏭

- Pablo Daniel Scardiglia Billordo — [@Pabl0ScB](https://github.com/Pabl0ScB)
- Alexis Lionel Monte - [@montealexis5-create](https://github.com/montealexis5-create)

## Descripción

SubastaYa es una plataforma de subastas en línea que está construida con estos dos criterios:

- **Solvencia garantizada:** toda puja que se haga se respalda por un saldo real que está retenido
  en garantía (*escrow*) dentro de la billetera virtual del usuario, cosa que no se pueda
  ofertar dinero que no tenga disponible.
- **Juego limpio:** A partir de un mecanismo de extensión automática de tiempo conocido como
  (*anti-sniping*) evita que haya victorias a partir de ofertas de último momento.

## Funcionalidades

- **Registro e inicio de sesión** con contraseñas hasheadas (BCrypt) y autenticación por token JWT.
- **Catálogo de subastas** con filtros, paginación y detalle de cada subasta.
- **Publicación de subastas** con título, descripción, imagen, categoría, precio base,
  incremento mínimo y fechas de inicio y fin.
- **Pujas en tiempo real** por SignalR: todos los que están en la sala ven la oferta nueva,
  y la sala se actualiza sola cuando la subasta se activa o finaliza.
- **Billetera virtual** con depósitos, saldo disponible y retenido, e historial de movimientos.
- **Saldo en garantía (escrow):** al pujar se retiene el monto; si otro supera la oferta, se
  libera el saldo del anterior líder en la misma transacción.
- **Anti-sniping:** si una oferta llega en el último minuto, el cierre se extiende 2 minutos.
- **Cierre automático:** un proceso en segundo plano activa las subastas programadas, cierra
  las vencidas, le paga al vendedor con lo que el ganador tenía retenido y marca desiertas
  las que no tuvieron ofertas.
- **Auditoría** de las acciones relevantes: ofertas rechazadas, extensiones de tiempo,
  activaciones y cierres.
- **Control de concurrencia optimista:** dos ofertas simultáneas no pueden quedar ambas
  como ganadoras; la segunda recibe un 409 con el nuevo mínimo.
- **Mis actividades:** las pujas realizadas y las subastas publicadas por cada usuario.

## Stack tecnológico

| Capa | Tecnología | Versión |
|---|---|---|
| SDK | .NET | 10.0.400 (`net10.0`) |
| Backend | C# / ASP.NET Core Web API | 10.0.11 |
| ORM | Entity Framework Core (enfoque Code-First) | 10.0.11 |
| Driver de base de datos | Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.3 |
| Base de datos | PostgreSQL (contenedor Docker) | 16 |
| Hash de contraseñas | BCrypt.Net-Next | 4.2.0 |
| Documentación de API | Swashbuckle.AspNetCore | 10.2.3 |
| Tiempo real (servidor) | SignalR | el de ASP.NET Core 10 |
| Tiempo real (cliente) | `@microsoft/signalr` | 8.0.7 |
| Frontend | HTML, CSS y JavaScript (Vanilla) + Bootstrap | 5.3.3 |

## Arquitectura

Cuatro proyectos, con las dependencias apuntando siempre hacia adentro:

```
SubastaYa.Api  ──►  SubastaYa.Application  ──►  SubastaYa.Domain
      │                                               ▲
      └──►  SubastaYa.Infrastructure  ───────────────┘
              (también conoce a Application)
```

| Proyecto | Qué contiene |
|---|---|
| `Domain` | Entidades con sus reglas (`Subasta.RecibirOferta`, `Billetera.Retener`...), enums, excepciones e interfaces de repositorio y de unidad de trabajo |
| `Application` | Servicios que ejecutan los casos de uso (comandos), interfaces de consulta (lecturas que devuelven DTOs), DTOs y opciones |
| `Infrastructure` | `AppDbContext`, configuraciones EF, repositorios, unidad de trabajo, consultas, seed y hash de contraseñas |
| `Api` | Controllers, middleware, hub de SignalR, worker y configuración de arranque |

Las escrituras pasan por repositorios de una sola entidad y una unidad de trabajo
(`IUnitOfWork`) que maneja la transacción y traduce los errores de concurrencia de EF a
excepciones del dominio. Las lecturas van por interfaces de consulta que devuelven DTOs y
nunca modifican datos. Los controllers llaman a los servicios para escribir y a las
consultas para leer (ver *Lecturas y escrituras separadas* más abajo).

`Domain` no conoce a nadie. Por eso las entidades no tienen atributos de Entity Framework
ni de ASP.NET: se pueden usar sin arrastrar la base de datos ni el framework web, y un
cambio en la capa de persistencia no obliga a tocar el dominio. SignalR queda detrás de
`INotificadorDeSubastas`, así que los servicios avisan eventos sin conocer el hub.

### Qué hace cada capa

| Capa | Hace | No hace |
|---|---|---|
| Controllers y hub | Reciben la petición, validan el DTO, delegan y devuelven el código HTTP | Reglas de negocio, cálculos, consultas a la base |
| Servicios (`Application`) | Reglas del sistema, validaciones, hora del servidor, transacciones | Conocer HTTP, rutas o códigos de estado |
| Repositorios y consultas (`Infrastructure`) | Guardar y leer con EF Core | Decidir reglas: reciben el criterio y devuelven datos |
| Dominio | Reglas propias de cada entidad (`MontoMinimo`, `RecibirOferta`, estados) | Depender de algo externo |

### Qué pasa al ofertar

`POST /auctions/{id}/bids` recorre las capas en este orden:

1. `PujasController` toma el usuario del token y llama al servicio.
2. `ValidadorDePujas` rechaza antes de tocar plata: 403 si es el vendedor; 422 si la
   subasta no está activa o ya venció, si ya es el líder, si no llega al mínimo o si no
   tiene saldo. Cada rechazo queda en auditoría.
3. `RegistradorDePujas` abre una transacción: libera la retención del líder anterior,
   retiene el saldo del nuevo, guarda la puja y llama a `Subasta.RecibirOferta`
   (anti-sniping). Si otra oferta se adelantó, deshace todo y responde 409.
4. `ServicioDePujas` arma la respuesta (mínimo nuevo, cantidad de ofertas, fecha de
   cierre) y avisa a la sala por `INotificadorDeSubastas`.
5. El controller devuelve 201.

### Manejo global de errores

Los servicios lanzan excepciones del dominio, sin saber de HTTP. Un único middleware
(`ManejadorDeExcepcionesMiddleware`) las traduce, así los controllers no llevan `try-catch`:

| Excepción | Código |
|---|---|
| `RecursoNoEncontradoException` | 404 |
| `AccionProhibidaException` | 403 |
| `ReglaDeNegocioException` | 422 |
| `ConflictoDeConcurrenciaException`, `RecursoDuplicadoException` | 409 |
| Cualquier otra | 500, con mensaje genérico |

Además, 400 lo devuelve la validación de los DTOs (formato) y 401 la autenticación JWT.

### Concurrencia optimista

`Subasta` y `Billetera` llevan una columna `Version` que EF usa como token de
concurrencia. Si dos ofertas leen la misma versión, la segunda falla al guardar,
`UnitOfWork` lo traduce a una excepción del dominio y el registrador deshace la
transacción. El cliente recibe un 409 con la oferta actual y el nuevo mínimo, no un 500.

### Tiempo real con SignalR

El hub vive en `/hubs/subastas` y solo administra grupos: el cliente llama a
`UnirseASubasta(id)` y `SalirDeSubasta(id)`. Las ofertas no entran por el hub sino por la
API REST; después de guardar, `NotificadorSignalR` difunde al grupo `subasta-{id}` los
eventos `NuevaPuja`, `SubastaActivada` y `SubastaFinalizada`. Si el aviso falla, se
registra una advertencia y la oferta ya guardada no se pierde.

## Decisiones de diseño

**Se guardan dos saldos, no tres.** El saldo disponible de una billetera no es una
columna: se calcula al vuelo como total menos retenido. Persistirlo sería guardar un
dato que ya se puede derivar de los otros dos, y en un sistema de garantía un disponible
que se desincroniza del resto dejaría ofertar con plata que en realidad ya está
comprometida en otra subasta.

**El control de concurrencia optimista solo está en `Subasta` y en `Billetera`.** Son las
dos únicas filas que el sistema lee, usa para calcular algo y después vuelve a escribir,
así que son las únicas donde una actualización perdida es posible. Las pujas, los
asientos del ledger y los registros de auditoría solo se insertan una vez y nunca se
tocan de nuevo, así que no hay nada que perder ahí.

**`PujaActual` y `LiderId` viven en la propia `Subasta`**, aunque técnicamente se podrían
calcular mirando la puja más alta en la tabla de pujas. Es una decisión a propósito: el
control de concurrencia necesita una fila puntual sobre la que dos operaciones puedan
competir. Si el líder se calculara con un `MAX`, dos ofertas que llegan al mismo tiempo
insertarían cada una su fila sin chocar entre sí, y las dos "ganarían" — que es
exactamente lo que el control de concurrencia tiene que impedir.

**Los asientos del ledger y los registros de auditoría no se modifican nunca.** Sus
repositorios solo permiten agregar filas (el del ledger también suma los montos de una
billetera). Si en algún momento hay que corregir un error contable, la corrección no reescribe el asiento original: agrega uno nuevo que lo
compensa, y así queda el rastro de que la corrección existió.

**No existe una entidad `Producto`.** El título, la descripción, la imagen y la categoría
son columnas de `Subasta` en lugar de vivir en una tabla aparte, porque en este sistema
no hay stock ni un catálogo de productos por fuera de las subastas: un producto nace con
su subasta y no se vuelve a subastar por separado. Separarlo hubiera agregado una tabla
y una relación sin resolver ningún requisito real.

**Lecturas y escrituras separadas (CQRS liviano).** Las escrituras tienen reglas y
transacciones, así que pasan por un servicio. Las lecturas no tienen nada que proteger,
así que van por una interfaz de consulta de `Application` que devuelve DTOs. Pasar cada
GET por un servicio solo agregaría métodos que reenvían la llamada sin aportar nada. La
regla de dependencias se respeta igual: las interfaces viven en `Application` y la
implementación en `Infrastructure`.

**El repositorio no conoce reglas.** Para buscar las subastas a activar o a cerrar,
`ISubastaRepository` recibe un criterio (`Subasta.ParaActivar(ahora)`,
`Subasta.Vencida(ahora)`) que define el dominio. Qué significa "vencida" lo decide el
dominio; el repositorio solo ejecuta la consulta.

**El servidor es la única fuente de verdad.** El monto mínimo, el estado, la cantidad de
ofertas, la hora (`fechaServidor`) y los límites de publicación (`GET /config/auctions`)
los calcula y los manda el back. El frontend los usa pero no los calcula: descuenta el
contador contra la hora del servidor y compara la oferta con el `montoMinimo` que recibe.
En pantalla valida lo básico (fechas coherentes, montos positivos, oferta por debajo del
mínimo) para no enviar peticiones inútiles, como pide el enunciado, pero quien garantiza
cada regla es el back, que responde 422, 403 o 409. Para eso el negocio toma la hora de
`IReloj`, una abstracción inyectable, y no de `DateTime.UtcNow` en cada servicio.

## API

Base: `/api/v1`. Los endpoints de publicar, pujar, billetera y `/users/me` requieren el
token JWT en el encabezado `Authorization: Bearer <token>`.

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/auth/sessions` | Iniciar sesión |
| POST | `/users` | Registrar un usuario |
| GET | `/users/me` | Datos del usuario autenticado |
| GET | `/users/me/bids` | Pujas del usuario |
| GET | `/users/me/auctions` | Subastas publicadas por el usuario |
| GET | `/categories` | Categorías |
| GET | `/auctions` | Catálogo con filtros y paginación |
| GET | `/auctions/{id}` | Detalle de una subasta |
| POST | `/auctions` | Publicar una subasta |
| GET | `/config/auctions` | Límites de publicación (duración máxima y tolerancia de inicio) |
| GET / POST | `/auctions/{id}/bids` | Listar pujas / ofertar |
| GET | `/wallets/me` | Saldo total, retenido y disponible |
| POST | `/wallets/me/deposits` | Depositar saldo |
| GET | `/wallets/me/entries` | Historial de movimientos |

El hub de SignalR está en `/hubs/subastas`. La documentación completa con los códigos de
respuesta (201, 400, 401, 403, 404, 409, 422) está en Swagger.

## Estructura del repositorio

```
SubastaYa/
├── src/
│   ├── SubastaYa.Domain/          Entidades, reglas y contratos
│   ├── SubastaYa.Application/     Casos de uso, DTOs y consultas
│   ├── SubastaYa.Infrastructure/  EF Core, repositorios, migraciones y seed
│   └── SubastaYa.Api/             Controllers, SignalR, worker y arranque
├── tests/                         Pruebas de dominio (xUnit)
├── frontend/                      HTML, CSS y JavaScript
├── scripts/                       verificar.ps1 y prueba-concurrencia.ps1
├── .github/workflows/ci.yml       Compila y corre los tests en cada push
├── CAMBIOS-REFACTOR.md            Qué cambió respecto del repositorio original
├── PRUEBAS-MANUALES.md            Guion de pruebas de interfaz en el navegador
├── .env.example                   Plantilla de contraseñas (el .env real no se versiona)
└── docker-compose.yml             PostgreSQL y pgAdmin
```

## Usuarios de prueba

Todos con la contraseña **`Test1234`**. Los saldos son los del arranque: cambian apenas
el proceso en segundo plano cierra las subastas vencidas del sembrado, así que después de
un rato de uso pueden no coincidir con esta tabla.

| Email | Seudónimo | Saldo total | Retenido | Para qué sirve |
|---|---|---|---|---|
| `vendedor@test.com` | vendedor | $0 | $0 | Publica subastas; no puede ofertar en las suyas |
| `comprador1@test.com` | comprador1 | $150.000 | $45.000 | Lidera la Notebook |
| `comprador2@test.com` | comprador2 | $200.000 | $0 | Fue superado en la Notebook |
| `comprador3@test.com` | comprador3 | $50.000 | $5.500 | Lidera el Álbum, que ya venció |
| `sinfondos@test.com` | sinfondos | $500 | $0 | Para ver el rechazo por saldo insuficiente |

## Requisitos previos

| Herramienta | Versión | Para qué |
|---|---|---|
| .NET SDK | 10.0 (desarrollado con 10.0.400) | Compilar y ejecutar la API |
| Docker Desktop | Cualquiera reciente | Levantar PostgreSQL y pgAdmin |
| Git | Cualquiera reciente | Clonar el repositorio |
| `dotnet-ef` | 10.x | Aplicar las migraciones |
| Live Server | Extensión de VS Code | Servir el frontend |

La herramienta de EF Core se instala una sola vez, de forma global:

```bash
dotnet tool install --global dotnet-ef
```

Para verificar el SDK, `dotnet --version` tiene que devolver una versión **10.x**. El
proyecto apunta a `net10.0` y no compila con versiones anteriores.

## Puesta en marcha

### 1. Clonar el repositorio

```bash
git clone https://github.com/Pabl0ScB/SubastaYa.git
cd SubastaYa
```

### 2. Levantar la base de datos

Las contraseñas de PostgreSQL y de pgAdmin no están en el repositorio: se leen de un
archivo `.env` que git ignora. Se crea a partir del ejemplo y se le cambian los valores:

```bash
cp .env.example .env
```

En PowerShell: `Copy-Item .env.example .env`. Después:

```bash
docker compose up -d
```

Levanta dos contenedores: PostgreSQL 16 en el puerto `5432` y pgAdmin en el `5050`. Los
dos puertos quedan abiertos solo para la propia máquina.

Las contraseñas solo se aplican la primera vez que se crea cada volumen. Si los
contenedores ya existían con otra contraseña, el `.env` tiene que usar esa misma, o hay
que borrar los volúmenes con `docker compose down -v` (borra los datos).

### 3. Configurar los secretos

La cadena de conexión y la clave de firma de los tokens **no están en el repositorio**:
se guardan con la herramienta de secretos de usuario del SDK, que los deja fuera del
proyecto. El repositorio es público, y quien tenga la clave de firma puede generar un
token válido para cualquier usuario.

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Host=127.0.0.1;Port=5432;Database=subastaya;Username=subastaya;Password=<POSTGRES_PASSWORD del .env>" -p src/SubastaYa.Api

dotnet user-secrets set "Jwt:Clave" "<un texto propio de 32 caracteres o más>" -p src/SubastaYa.Api
```

En la cadena va la misma contraseña que en `POSTGRES_PASSWORD` del `.env`. El host es
`127.0.0.1` a propósito: en algunas máquinas `localhost` se resuelve a IPv6 y la conexión
expira, porque Docker publica el puerto solo en IPv4. La clave
puede ser cualquier texto, pero **de 32 caracteres o más**: el algoritmo de firma
necesita una clave de ese tamaño, y con una más corta la aplicación arranca igual pero
falla al iniciar sesión, cuando tiene que firmar el token.

> Si salteás este paso, los comandos siguientes fallan al intentar conectarse a la base
> o al construir la clave de firma. No hace falta `dotnet user-secrets init`: el
> identificador ya está en el archivo de proyecto.

### 4. Crear las tablas

```bash
dotnet ef database update -p src/SubastaYa.Infrastructure -s src/SubastaYa.Api
```

Durante la creación puede aparecer un `fail` con un error de conexión: es esperable, EF
intenta conectarse, ve que la base no existe todavía y la crea.

### 5. Ejecutar la API

```bash
dotnet run --project src/SubastaYa.Api
```

Queda escuchando en `http://localhost:5093` y la documentación interactiva está en
`http://localhost:5093/swagger`. Swagger y los datos de prueba solo se activan en el
ambiente `Development`, que es el que usa `dotnet run` por el perfil de
`launchSettings.json`.

**Los datos de prueba se cargan solos** la primera vez, con la base vacía. Si ya hay
usuarios cargados, el sembrado no hace nada.

Además queda corriendo el proceso en segundo plano que adjudica las subastas: cada 10
segundos activa las programadas que llegaron a su hora, cierra las vencidas liquidando
el pago del comprador al vendedor y marca desiertas las que no recibieron ofertas. Se ve
en la consola de la aplicación.

### 6. Abrir el frontend

Con la API corriendo, abrir `frontend/index.html` con **Live Server** (clic derecho →
*Open with Live Server*). Sirve cualquier servidor de archivos estáticos, siempre que
use el puerto indicado abajo.

Tiene que quedar servido en `http://127.0.0.1:5500` o `http://localhost:5500`: son los
dos únicos orígenes que la API acepta. Si Live Server usa otro puerto, hay que agregarlo
a la política de CORS en `src/SubastaYa.Api/Extensiones/PoliticaCors.cs`.

La dirección de la API está en `frontend/js/config.js`, por si se cambia el puerto.

### 7. pgAdmin (opcional)

`http://localhost:5050`, con `admin@subastaya.com` y la contraseña `PGADMIN_PASSWORD` del
`.env`. Al registrar el servidor, el host es **`db`** —el nombre del contenedor dentro de
la red de Docker—, no `localhost`; el usuario y la base son los del `docker-compose.yml`
y la contraseña es `POSTGRES_PASSWORD`.

### Empezar de nuevo

Para volver al estado inicial, con la API detenida:

```bash
dotnet ef database drop -f -p src/SubastaYa.Infrastructure -s src/SubastaYa.Api
dotnet ef database update -p src/SubastaYa.Infrastructure -s src/SubastaYa.Api
```

Al arrancar la aplicación se vuelven a cargar los datos de prueba. Conviene hacerlo antes
de una demostración: algunas subastas del sembrado nacen vencidas a propósito y el
proceso en segundo plano las cierra en cuanto arranca.

## Pruebas y verificación

| Qué | Cómo | Qué comprueba |
|---|---|---|
| Tests de dominio | `dotnet test` | Reglas de `Subasta` y `Billetera` sin base de datos: monto mínimo, anti-sniping, versiones, saldos |
| Verificación de punta a punta | `powershell -ExecutionPolicy Bypass -File scripts/verificar.ps1` | Con la API y la base arriba: códigos HTTP, reglas rechazadas con 422, ofertas, saldos y el contrato de la API |
| Integración continua | GitHub Actions (`ci.yml`) | `dotnet build` y `dotnet test` en cada push y pull request |

`verificar.ps1` crea su propio usuario y su propia subasta, así que no depende del sembrado.
Cada corrida deja una subasta de prueba más; para volver al estado inicial, ver *Empezar de
nuevo*. Con `-Completo` también compila, corre los tests y levanta la API sola.

Lo que los scripts no pueden ver (colores del reloj, avisos, tiempo real entre dos navegadores,
cierre por el worker) está en `PRUEBAS-MANUALES.md`.

## Prueba de concurrencia

Dos personas que ofertan el mismo monto en el mismo instante no pueden quedar las dos
como líderes: sería la misma subasta con saldo retenido en dos billeteras. El control de
concurrencia optimista lo impide — la columna `Version` de la subasta y de las
billeteras—, y `scripts/prueba-concurrencia.ps1` lo demuestra.

El script crea sus propios postores con saldo simulado, así no depende de quién esté
liderando en ese momento, y dispara todas las ofertas juntas para que lleguen al
servidor a la vez. No modifica nada de lo que ya existe.

Con la API corriendo y la base sembrada, desde la raíz del repositorio:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/prueba-concurrencia.ps1
```

Salida de una ejecución real (los identificadores, los montos y el nombre de la subasta
cambian según el estado de la base):

```
Subasta 6 - PS5 Slim (estado: Activa)
5 postores de prueba creados, con saldo simulado cargado.

Ronda 1 - 5 ofertas simultaneas de $ 1.105.000,00
  prueba-951477-1        201  {"fechaFin":"2026-09-20T23:00:00Z","montoMinimo":1120000.00,
                               "tiempoExtendido":false,"id":11,"monto":1105000.00,
                               "fechaPuja":"2026-09-20T18:17:29.4019206Z","seudonimo":"prueba-951477-1"}
  prueba-951477-2        409  {"mensaje":"Otra oferta se registró antes que la tuya. La oferta
                               mínima ahora es de $ 1.120.000,00.","pujaActual":1105000.00,
                               "montoMinimo":1120000.00,"fechaFin":"2026-09-20T23:00:00Z"}
  prueba-951477-3        409  {"mensaje":"Otra oferta se registró antes que la tuya. ...
  prueba-951477-4        409  {"mensaje":"Otra oferta se registró antes que la tuya. ...
  prueba-951477-5        409  {"mensaje":"Otra oferta se registró antes que la tuya. ...

OK: las ofertas simultaneas se rechazaron con 409 y solo una por ronda fue aceptada.
Verificacion en la base: la tabla RegistrosAuditoria tiene una fila PUJA_RECHAZADA_CONCURRENCIA por cada 409.
```

La respuesta del conflicto no es un error seco: trae la oferta actual, el nuevo mínimo y
la fecha de cierre, que es lo que necesita la pantalla para ofrecer el reintento en un
solo clic.

**Una sola aceptada y el resto rechazadas**: eso es lo que se está probando. El script
termina con código 1 si alguna ronda acepta más de una oferta.

Si en cambio informa que ninguna ronda produjo un conflicto, las ofertas no llegaron a
superponerse: se vuelve a ejecutar, o se aumenta la cantidad de postores con
`-Postores 10`.

Los rechazos quedan registrados. En la base:

```sql
SELECT "Accion", COUNT(*)
FROM "RegistrosAuditoria"
GROUP BY "Accion";
```

Tiene que haber una fila `PUJA_RECHAZADA_CONCURRENCIA` por cada 409.

Y ningún saldo queda retenido de más — el libro mayor respalda cada peso de cada
billetera:

```sql
SELECT b."UsuarioId", b."SaldoTotal",
       SUM(CASE WHEN a."Tipo" IN ('Deposito','Cobro') THEN a."Monto"
                WHEN a."Tipo" = 'Pago' THEN -a."Monto" ELSE 0 END) AS total_segun_ledger
FROM "Billeteras" b
LEFT JOIN "AsientosLedger" a ON a."BilleteraId" = b."Id"
GROUP BY b."UsuarioId", b."SaldoTotal"
ORDER BY b."UsuarioId";
```

Las dos columnas tienen que coincidir en todas las filas.
