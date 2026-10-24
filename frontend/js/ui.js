function mostrarAlerta(idAlerta, texto, tipo = 'danger') {
    const alerta = document.getElementById(idAlerta);
    if (!alerta) return;
    alerta.textContent = texto;
    alerta.className = `alert alert-${tipo}`;
    alerta.hidden = false;
}

function ocultarAlerta(idAlerta) {
    const alerta = document.getElementById(idAlerta);
    if (alerta) alerta.hidden = true;
}

function alternarCargando(idBoton, idSpinner, activo) {
    const boton = document.getElementById(idBoton);
    const spinner = document.getElementById(idSpinner);
    if (boton) boton.disabled = activo;
    if (spinner) spinner.hidden = !activo;
}

function mostrarSecciones(secciones, estado, idAlerta) {
    Object.entries(secciones).forEach(([nombre, ids]) => {
        [].concat(ids).forEach(id => {
            document.getElementById(id).hidden = nombre !== estado;
        });
    });
    if (estado !== 'error') ocultarAlerta(idAlerta);
}

function textoDeOfertas(cantidad) {
    if (cantidad === 0) return 'Sin ofertas';
    return cantidad === 1 ? '1 oferta' : `${cantidad} ofertas`;
}

let desfaseServidorMs = 0;

// El back manda su hora (fechaServidor). Guardamos la diferencia con el reloj local
// para que el contador descuente contra la hora del servidor y no contra la de la PC.
function sincronizarReloj(fechaServidorIso) {
    if (fechaServidorIso) {
        desfaseServidorMs = Date.parse(fechaServidorIso) - Date.now();
    }
}

function ahoraServidor() {
    return Date.now() + desfaseServidorMs;
}

async function poblarCategorias(select) {
    const categorias = await api.get('/categories');
    categorias.forEach(categoria => {
        const opcion = document.createElement('option');
        opcion.value = categoria.id;
        opcion.textContent = categoria.nombre;
        select.appendChild(opcion);
    });
}

const formatoPesos = new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' });
const formatoFecha = new Intl.DateTimeFormat('es-AR', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit', hourCycle: 'h23'
});

function formatearPesos(monto) {
    return formatoPesos.format(monto);
}

function formatearFecha(fechaIso) {
    return formatoFecha.format(new Date(fechaIso));
}

function limitarADosDecimales(campo) {
    campo.addEventListener('input', () => {
        const [entero, decimales] = campo.value.split('.');
        if (decimales && decimales.length > 2) {
            campo.value = `${entero}.${decimales.slice(0, 2)}`;
        }
    });
}

const formatoHoraExacta = new Intl.DateTimeFormat('es-AR', {
    hour: '2-digit', minute: '2-digit', second: '2-digit', hourCycle: 'h23'
});

function formatearHoraExacta(fechaIso) {
    return formatoHoraExacta.format(new Date(fechaIso));
}

function mostrarAvisoFlotante(mensaje, { tipo = 'success', icono = null, duracionMs = 8000 } = {}) {
    document.querySelector('.aviso-flotante')?.remove();

    const aviso = document.createElement('div');
    aviso.className =
        `alert alert-${tipo} shadow-lg position-fixed bottom-0 end-0 m-3 d-flex ` +
        'align-items-center gap-2 aviso-flotante';
    aviso.setAttribute('role', 'status');

    if (icono) {
        const spanIcono = document.createElement('span');
        spanIcono.className = 'fs-4';
        spanIcono.setAttribute('aria-hidden', 'true');
        spanIcono.textContent = icono;
        aviso.appendChild(spanIcono);
    }

    const texto = document.createElement('span');
    texto.textContent = mensaje;
    aviso.appendChild(texto);

    document.body.appendChild(aviso);
    setTimeout(() => aviso.remove(), duracionMs);
    return aviso;
}
