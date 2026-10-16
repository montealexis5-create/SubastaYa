# Pruebas manuales pendientes

Para correr: `docker compose up -d`, `dotnet ef database update`, `dotnet run` en la Api y abrir el frontend con Live Server (127.0.0.1:5500).
Todos los usuarios tienen password `Test1234`: vendedor, comprador1, comprador2, comprador3 y sinfondos (@test.com).
Desactivar el traductor automatico del navegador. La subasta critica dura 90 s y la estandar 25 min desde que se carga el seed.
Para empezar de cero: `dotnet ef database drop -f`, `dotnet ef database update` y `dotnet run`.

- [ ] Pujar con comprador2 en la subasta activa estandar. Tiene que subir el precio y quedar de lider.
- [ ] Pujar con comprador1 arriba de comprador2. A comprador2 se le libera lo retenido y le llega el aviso de superado.
- [ ] Pujar con sinfondos. Tiene que rechazar por saldo insuficiente y quedar en auditoria PUJA_RECHAZADA_SALDO.
- [ ] Pujar menos que el minimo o el vendedor en su propia subasta. Tiene que dar 422 y 403.
- [ ] Publicar una subasta de 3 minutos. En el ultimo minuto el reloj se pone rojo, una puja extiende 2 minutos y sale el aviso.
- [ ] Esperar el cierre. El worker (cada 10 s) tiene que pagar al vendedor con lo retenido del ganador.
- [ ] Una subasta vencida sin pujas tiene que quedar desierta (CIERRE_DESIERTA).
- [ ] Depositar plata en la billetera y revisar el historial de movimientos y la paginacion.
- [ ] Mis Actividades: mis pujas y mis subastas.
- [ ] La subasta Programada no debe aceptar pujas hasta que el worker la active.
- [ ] `scripts/prueba-concurrencia.ps1`: tiene que dar un solo 201 y el resto 409.
- [ ] Swagger: una puja concurrente devuelve 409.
