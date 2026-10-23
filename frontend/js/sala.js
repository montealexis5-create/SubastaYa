let subastaId = null;
let conexion = null;
let cerradaPorFin = false;
let cantidadOfertas = 0;
let liderActual = null;
let montoMostrado = 0;
const idsEnHistorial = new Set();

document.addEventListener('subasta:cargada', async evento => {
    const detalle = evento.detail;
    const primeraVez = subastaId === null;
    subastaId = detalle.id;
    cantidadOfertas = detalle.cantidadOfertas;
    montoMostrado = detalle.pujaActual;
    liderActual = detalle.seudonimoLider;

    await cargarHistorial();

    const abierta = detalle.estado === 'Activa' || detalle.estado === 'Programada';
    if (primeraVez && abierta) conectar();
    if (!abierta && conexion) {
        cerradaPorFin = true;
        conexion.stop();
    }
});

async function conectar() {
    conexion = new signalR.HubConnectionBuilder()
        .withUrl(HUB_SUBASTAS)
        .withAutomaticReconnect()
        .build();

    conexion.on('NuevaPuja', puja => registrarPujaEnSala(puja));
    conexion.on('SubastaFinalizada', () => recargarEstado());
    conexion.on('SubastaActivada', () => recargarEstado());

    conexion.onreconnecting(() => mostrarConexion('reconectando'));
    conexion.onreconnected(async () => {
        await conexion.invoke('UnirseASubasta', subastaId);
        await recargarEstado();
        mostrarConexion('conectado');
    });
    conexion.onclose(() => mostrarConexion(cerradaPorFin ? 'finalizada' : 'desconectado'));

    try {
        await conexion.start();
        await conexion.invoke('UnirseASubasta', subastaId);
        mostrarConexion('conectado');
    } catch {
        mostrarConexion('desconectado');
    }
}

function mostrarConexion(estado) {
    const textos = {
        conectado: 'En vivo: las ofertas se actualizan solas.',
        reconectando: 'Reconectando…',
        desconectado: 'Sin conexión en vivo. Recargá la página para ver las últimas ofertas.',
        finalizada: ''
    };
    document.getElementById('estado-conexion').textContent = textos[estado];
}

async function cargarHistorial() {
    const lista = document.getElementById('historial-pujas');
    idsEnHistorial.clear();
    try {
        const pujas = await api.get(`/auctions/${subastaId}/bids`);
        lista.replaceChildren(...pujas.map(crearFilaHistorial));
        pujas.forEach(p => idsEnHistorial.add(p.id));
        if (pujas.length === 0) lista.replaceChildren(crearFilaVacia());
    } catch (error) {
        lista.replaceChildren(crearFilaVacia(error.message));
    }
    actualizarEstadoPostor();
}

function crearFilaHistorial(puja) {
    const fila = document.createElement('li');
    fila.className = 'list-group-item d-flex justify-content-between align-items-center';
    fila.dataset.seudonimo = puja.seudonimo;

    const quien = document.createElement('span');
    quien.textContent = puja.seudonimo;

    const hora = document.createElement('span');
    hora.className = 'small text-muted';
    hora.textContent = formatearHoraExacta(puja.fechaPuja);

    const monto = document.createElement('span');
    monto.className = 'fw-semibold';
    monto.textContent = formatearPesos(puja.monto);

    fila.append(quien, hora, monto);
    return fila;
}

function crearFilaVacia(texto = 'Todavía no hay ofertas.') {
    const fila = document.createElement('li');
    fila.className = 'list-group-item text-muted js-sin-ofertas';
    fila.textContent = texto;
    return fila;
}

function registrarPujaEnSala(puja) {
    if (idsEnHistorial.has(puja.id)) return;

    if (puja.monto <= montoMostrado) {
        recargarEstado();
        return;
    }

    idsEnHistorial.add(puja.id);
    cantidadOfertas = puja.cantidadOfertas;
    montoMostrado = puja.monto;
    liderActual = puja.seudonimo;

    const lista = document.getElementById('historial-pujas');
    lista.querySelector('.js-sin-ofertas')?.remove();
    lista.prepend(crearFilaHistorial(puja));

    document.getElementById('puja-actual').textContent = formatearPesos(puja.monto);
    document.getElementById('lider').textContent = puja.seudonimo;
    document.getElementById('oferta-minima').textContent = formatearPesos(puja.montoMinimo);
    document.getElementById('ofertas').textContent = textoDeOfertas(cantidadOfertas);

    const contador = document.querySelector('#tiempo [data-fecha-fin]');
    if (contador) contador.dataset.fechaFin = puja.fechaFin;

    if (puja.tiempoExtendido) mostrarExtension(puja.fechaFin);
    actualizarEstadoPostor();

    document.dispatchEvent(new CustomEvent('sala:actualizada', {
        detail: {
            pujaActual: puja.monto,
            montoMinimo: puja.montoMinimo,
            seudonimoLider: puja.seudonimo,
            fechaFin: puja.fechaFin
        }
    }));
}

function actualizarEstadoPostor() {
    const badge = document.getElementById('estado-postor');
    const usuario = obtenerUsuario();
    const lista = document.getElementById('historial-pujas');
    const participa = usuario && lista.querySelector(
        `[data-seudonimo="${CSS.escape(usuario.seudonimo)}"]`);

    if (!participa) { badge.hidden = true; return; }

    const lidera = liderActual === usuario.seudonimo;
    badge.textContent = lidera ? 'Liderando' : 'Superado';
    badge.className = `badge ms-1 ${lidera ? 'text-bg-success' : 'text-bg-warning'}`;
    badge.hidden = false;
}

function mostrarExtension(fechaFin) {
    const titulo = document.getElementById('titulo')?.textContent ?? 'La subasta';
    mostrarAvisoFlotante(
        `${titulo}: una oferta llegó sobre el cierre. Ahora termina a las ${formatearHoraExacta(fechaFin)}.`,
        { tipo: 'warning', icono: '⏱️', duracionMs: 10000 }
    );
}

async function recargarEstado() {
    try {
        const detalle = await api.get(`/auctions/${subastaId}`);
        dibujarDetalle(detalle);
    } catch {
        mostrarConexion('desconectado');
    }
}
