document.addEventListener('DOMContentLoaded', () => {
    const parametros = new URLSearchParams(location.search);
    const id = parametros.get('id');

    if (!id || !/^\d+$/.test(id)) {
        mostrarEstado('no-encontrada');
        return;
    }

    cargarDetalle(id);
});

const SECCIONES = {
    cargando: 'cargando-subasta',
    detalle: 'detalle-subasta',
    'no-encontrada': 'no-encontrada'
};

const CLASES_ESTADO = {
    Activa: 'text-bg-success',
    Programada: 'text-bg-secondary',
    Finalizada: 'text-bg-primary'
};

function mostrarEstado(estado) {
    mostrarSecciones(SECCIONES, estado, 'alerta-subasta');
}

async function cargarDetalle(id) {
    mostrarEstado('cargando');

    try {
        const detalle = await api.get(`/auctions/${id}`);
        dibujarDetalle(detalle);
        mostrarEstado('detalle');
    } catch (error) {
        if (error.status === 404) {
            mostrarEstado('no-encontrada');
        } else {
            mostrarEstado('error');
            mostrarAlerta('alerta-subasta', error.message);
        }
    }
}

function dibujarDetalle(detalle) {
    sincronizarReloj(detalle.fechaServidor);

    document.title = `${detalle.titulo} — SubastaYa`;

    document.getElementById('categoria').textContent = detalle.nombreCategoria;
    document.getElementById('titulo').textContent = detalle.titulo;
    document.getElementById('vendedor').textContent = detalle.seudonimoVendedor;
    document.getElementById('descripcion').textContent = detalle.descripcion;

    document.getElementById('puja-actual').textContent = formatearPesos(detalle.pujaActual);
    document.getElementById('precio-base').textContent = formatearPesos(detalle.precioBase);
    document.getElementById('incremento').textContent = formatearPesos(detalle.incrementoMinimo);

    document.getElementById('oferta-minima').textContent = formatearPesos(detalle.montoMinimo);

    document.getElementById('ofertas').textContent = textoDeOfertas(detalle.cantidadOfertas);

    document.getElementById('lider').textContent = detalle.seudonimoLider ?? 'Todavía nadie';

    dibujarEstadoYTiempo(detalle);
    dibujarFoto(detalle);

    document.dispatchEvent(new CustomEvent('subasta:cargada', { detail: detalle }));
}

function dibujarEstadoYTiempo(detalle) {
    const badgeEstado = document.getElementById('estado');
    const tiempo = document.getElementById('tiempo');

    const etiquetas = {
        Programada: 'Próxima',
        Activa: 'Activa',
        Finalizada: 'Finalizada',
        Desierta: 'Desierta'
    };
    badgeEstado.textContent = etiquetas[detalle.estado] ?? detalle.estado;
    badgeEstado.className = `badge ${CLASES_ESTADO[detalle.estado] ?? 'text-bg-dark'}`;

    if (detalle.estado === 'Programada') {
        tiempo.textContent = `Comienza el ${formatearFecha(detalle.fechaInicio)}`;
    } else if (detalle.estado === 'Activa') {
        tiempo.innerHTML = '';
        const span = document.createElement('span');
        span.className = 'contador';
        span.dataset.fechaFin = detalle.fechaFin;
        tiempo.appendChild(span);
    } else if (detalle.estado === 'Finalizada') {
        tiempo.textContent = `Ganó ${detalle.seudonimoLider}`;
    } else if (detalle.estado === 'Desierta') {
        tiempo.textContent = 'Terminó sin ofertas';
    }
}

function dibujarFoto(detalle) {
    const fondo = document.getElementById('foto-fondo');
    const frente = document.getElementById('foto-frente');

    fondo.src = detalle.urlImagen;
    frente.src = detalle.urlImagen;
    frente.alt = detalle.titulo;

    frente.addEventListener('error', () => {
        document.querySelector('.foto-subasta').hidden = true;
    });
}
