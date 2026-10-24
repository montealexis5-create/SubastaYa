# Pruebas manuales de interfaz

Cubren lo que `scripts/verificar.ps1` y `scripts/prueba-concurrencia.ps1` no pueden ver: lo que se ve y se oye en el navegador (colores, avisos, tiempo real) y los procesos que dependen del reloj. Los codigos HTTP, el monto minimo, los saldos, el escrow y la concurrencia ya los comprueban los scripts.

Para correr: copiar `.env.example` a `.env` con sus contrasenas, `docker compose up -d`, `dotnet ef database update`, `dotnet run` en la Api y abrir el frontend con Live Server (127.0.0.1:5500).
Todos los usuarios tienen password `Test1234`: vendedor, comprador1, comprador2, comprador3 y sinfondos (@test.com).
Desactivar el traductor automatico del navegador. La subasta critica dura 90 s y la estandar 25 min desde que se carga el seed.
Para empezar de cero: `dotnet ef database drop -f`, `dotnet ef database update` y `dotnet run`.

## Sala en vivo
- [ ] Pujar con comprador2 en la subasta activa estandar. Sube el precio y queda como "Liderando".
- [ ] Pujar con comprador1 arriba de comprador2. A comprador2 le llega el aviso de superado y se le libera lo retenido.
- [ ] Con dos navegadores en la misma subasta, una puja en uno actualiza precio, cantidad de ofertas y lider en el otro sin recargar.
- [ ] Pujar con sinfondos: aparece el aviso de saldo insuficiente.
- [ ] Pujar menos que el minimo: el formulario avisa antes de enviar. Si otra oferta sube el minimo mientras se escribe, aparece el aviso.
- [ ] El vendedor en su propia subasta: la API lo rechaza y se muestra el mensaje.
- [ ] Publicar una subasta de 3 minutos. En el ultimo minuto el reloj se pone rojo, una puja extiende 2 minutos y sale el aviso.
- [ ] La subasta Programada no acepta pujas hasta que el worker la active.

## Procesos en segundo plano
- [ ] Esperar el cierre: el worker (cada 10 s) paga al vendedor con lo retenido del ganador.
- [ ] Una subasta vencida sin pujas queda desierta.

## Otras pantallas
- [ ] Publicar con fechas incoherentes (fin antes de inicio, incremento mayor al precio): el aviso sale junto al campo. Si el back rechaza con 422, tambien sale junto al campo.
- [ ] En los primeros 10 s despues de vencer (con el Album), el catalogo y el detalle muestran "Finalizando…" y el detalle no ofrece el formulario.
- [ ] Depositar en la billetera; revisar el historial de movimientos y la paginacion.
- [ ] Mis Actividades: mis pujas y mis subastas.
