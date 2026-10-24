const NOMBRES_MOVIMIENTO = {
    Deposito:   'Depósito',
    Cobro:      'Cobro',
    Pago:       'Pago',
    Retencion:  'Retención',
    Liberacion: 'Liberación'
};

const PRESENTACION_EFECTO = {
    suma:    { signo: '+ ', clase: 'text-success' },
    resta:   { signo: '− ', clase: 'text-danger' },
    reserva: { signo: '',   clase: 'text-muted' }
};

const TAMANO_PAGINA = 10;
const MENSAJE_MONTO = 'Ingresá un monto mayor a cero, con hasta dos decimales.';
let paginaMovimientos = 1;

document.addEventListener('DOMContentLoaded', () => {
    if (!haySesion()) return;

    const campoMonto = document.getElementById('monto-deposito');
    limitarADosDecimales(campoMonto);
    campoMonto.addEventListener('input', () => {
        campoMonto.setCustomValidity('');
        document.getElementById('error-monto-deposito').textContent = MENSAJE_MONTO;
    });

    document.getElementById('formulario-deposito').addEventListener('submit', depositar);
    document.getElementById('boton-ver-mas').addEventListener('click', verMasMovimientos);

    cargarBilletera();
});

const SECCIONES = {
    cargando: 'cargando-billetera',
    exito: 'contenido-billetera'
};

function mostrarEstado(estado) {
    mostrarSecciones(SECCIONES, estado, 'alerta-billetera');
}

async function cargarBilletera() {
    mostrarEstado('cargando');

    try {
        const [saldos, movimientos] = await Promise.all([
            api.get('/wallets/me'),
            api.get(`/wallets/me/entries?pagina=1&tamano=${TAMANO_PAGINA}`)
        ]);

        dibujarSaldos(saldos);
        paginaMovimientos = 1;
        dibujarMovimientos(movimientos, false);
        mostrarEstado('exito');
    } catch (error) {
        mostrarEstado('error');
        mostrarAlerta('alerta-billetera', error.message);
    }
}

function dibujarSaldos({ saldoTotal, saldoRetenido, saldoDisponible }) {
    document.getElementById('saldo-total').textContent      = formatearPesos(saldoTotal);
    document.getElementById('saldo-retenido').textContent   = formatearPesos(saldoRetenido);
    document.getElementById('saldo-disponible').textContent = formatearPesos(saldoDisponible);
}

function dibujarMovimientos(pagina, agregar) {
    const cuerpo = document.getElementById('cuerpo-movimientos');
    if (!agregar) cuerpo.replaceChildren();

    pagina.items.forEach(movimiento => cuerpo.appendChild(crearFila(movimiento)));

    const hayMovimientos = pagina.totalElementos > 0;
    document.getElementById('vacio-movimientos').hidden = hayMovimientos;
    document.getElementById('tabla-movimientos').hidden = !hayMovimientos;
    document.getElementById('boton-ver-mas').hidden = pagina.paginaActual >= pagina.totalPaginas;
}

function crearFila(movimiento) {
    const presentacion = PRESENTACION_EFECTO[movimiento.efecto];
    const fila = document.createElement('tr');

    const fecha = document.createElement('td');
    fecha.className = 'text-nowrap';
    fecha.textContent = formatearFecha(movimiento.fecha);

    const nombreTipo = document.createElement('td');
    nombreTipo.textContent = NOMBRES_MOVIMIENTO[movimiento.tipo] ?? movimiento.tipo;

    const detalle = document.createElement('td');
    detalle.textContent = movimiento.descripcion ?? '';
    if (movimiento.subastaId) {
        const enlace = document.createElement('a');
        enlace.href = `subasta.html?id=${movimiento.subastaId}`;
        enlace.textContent = movimiento.tituloSubasta;
        const linea = document.createElement('div');
        linea.className = 'small';
        linea.appendChild(enlace);
        detalle.appendChild(linea);
    }

    const monto = document.createElement('td');
    monto.className = `text-end text-nowrap fw-semibold ${presentacion.clase}`;
    monto.textContent = presentacion.signo + formatearPesos(movimiento.monto);

    fila.append(fecha, nombreTipo, detalle, monto);
    return fila;
}

async function verMasMovimientos() {
    alternarCargando('boton-ver-mas', 'spinner-ver-mas', true);
    try {
        const siguiente = paginaMovimientos + 1;
        const pagina = await api.get(`/wallets/me/entries?pagina=${siguiente}&tamano=${TAMANO_PAGINA}`);
        paginaMovimientos = siguiente;
        dibujarMovimientos(pagina, true);
    } catch (error) {
        mostrarAlerta('alerta-billetera', error.message);
    } finally {
        alternarCargando('boton-ver-mas', 'spinner-ver-mas', false);
    }
}

async function depositar(evento) {
    evento.preventDefault();
    const formulario = evento.target;
    const campo = document.getElementById('monto-deposito');

    ocultarAlerta('alerta-deposito');

    if (!formulario.checkValidity()) {
        formulario.classList.add('was-validated');
        return;
    }

    alternarCargando('boton-depositar', 'spinner-depositar', true);

    try {
        const saldos = await api.post('/wallets/me/deposits', { monto: Number(campo.value) });

        dibujarSaldos(saldos);
        formulario.reset();
        formulario.classList.remove('was-validated');
        mostrarAlerta('alerta-deposito', 'Saldo cargado.', 'success');
    } catch (error) {
        const errorDelCampo = error.erroresPorCampo?.Monto?.[0];
        if (errorDelCampo) {
            document.getElementById('error-monto-deposito').textContent = errorDelCampo;
            campo.setCustomValidity(errorDelCampo);
            formulario.classList.add('was-validated');
        } else {
            mostrarAlerta('alerta-deposito', error.message);
        }
        return;
    } finally {
        alternarCargando('boton-depositar', 'spinner-depositar', false);
    }

    await refrescarMovimientos();
}

async function refrescarMovimientos() {
    try {
        const pagina = await api.get(`/wallets/me/entries?pagina=1&tamano=${TAMANO_PAGINA}`);
        paginaMovimientos = 1;
        dibujarMovimientos(pagina, false);
    } catch {
        mostrarAlerta('alerta-billetera',
            'El saldo se cargó, pero no se pudieron actualizar los movimientos. Recargá la página.',
            'warning');
    }
}
