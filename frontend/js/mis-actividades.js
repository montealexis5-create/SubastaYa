const NOMBRES_ESTADO = {
    Programada: 'Próxima',
    Activa: 'Activa',
    Finalizada: 'Finalizada',
    Desierta: 'Desierta'
};

const COLORES_RESULTADO = {
    'Liderando': 'text-bg-success',
    'Superado':  'text-bg-warning',
    'Ganada':    'text-bg-primary',
    'No ganada': 'text-bg-secondary'
};

const COLORES_ADJUDICACION = {
    'Adjudicada': 'text-bg-success',
    'En curso':   'text-bg-info',
    'Programada': 'text-bg-light border',
    'Desierta':   'text-bg-secondary'
};

const PESTANAS = {
    compras:       { ruta: '/users/me/bids',     crearFila: crearFilaCompra,      cargada: false },
    publicaciones: { ruta: '/users/me/auctions', crearFila: crearFilaPublicacion, cargada: false }
};

document.addEventListener('DOMContentLoaded', () => {
    if (!haySesion()) return;

    cargarPestana('compras');

    document.getElementById('pestana-publicaciones').addEventListener('shown.bs.tab', () => {
        if (!PESTANAS.publicaciones.cargada) cargarPestana('publicaciones');
    });
});

function mostrarEstado(prefijo, estado) {
    const secciones = {
        cargando: `cargando-${prefijo}`,
        vacio: `vacio-${prefijo}`,
        exito: `tabla-${prefijo}`
    };
    mostrarSecciones(secciones, estado, `alerta-${prefijo}`);
}

async function cargarPestana(prefijo) {
    const pestana = PESTANAS[prefijo];
    mostrarEstado(prefijo, 'cargando');

    try {
        const filas = await api.get(pestana.ruta);
        pestana.cargada = true;

        if (filas.length === 0) {
            mostrarEstado(prefijo, 'vacio');
            return;
        }

        const cuerpo = document.getElementById(`cuerpo-${prefijo}`);
        cuerpo.replaceChildren(...filas.map(pestana.crearFila));
        mostrarEstado(prefijo, 'exito');
    } catch (error) {
        mostrarEstado(prefijo, 'error');
        mostrarAlerta(`alerta-${prefijo}`, error.message);
    }
}

function crearFilaCompra(participacion) {
    return crearFila([
        celdaSubasta(participacion.subastaId, participacion.titulo),
        celdaTexto(formatearPesos(participacion.miOfertaMaxima), 'text-end text-nowrap'),
        celdaTexto(formatearPesos(participacion.pujaActual), 'text-end text-nowrap'),
        celdaTexto(formatearFecha(participacion.fechaFin), 'text-nowrap'),
        celdaEtiqueta(participacion.resultado, COLORES_RESULTADO[participacion.resultado])
    ]);
}

function crearFilaPublicacion(publicacion) {
    const valorAdjudicacion = publicacion.estadoAdjudicacion;
    const adjudicacion = celdaEtiqueta(
        NOMBRES_ESTADO[valorAdjudicacion] ?? valorAdjudicacion,
        COLORES_ADJUDICACION[valorAdjudicacion]);

    if (publicacion.seudonimoGanador) {
        const ganador = document.createElement('div');
        ganador.className = 'small text-muted mt-1';
        ganador.textContent = publicacion.seudonimoGanador;
        adjudicacion.appendChild(ganador);
    }

    return crearFila([
        celdaSubasta(publicacion.id, publicacion.titulo),
        celdaTexto(NOMBRES_ESTADO[publicacion.estado] ?? publicacion.estado),
        celdaTexto(publicacion.cantidadOfertas, 'text-end'),
        celdaTexto(formatearPesos(publicacion.pujaActual), 'text-end text-nowrap'),
        celdaTexto(formatearPesos(publicacion.recaudado), 'text-end text-nowrap'),
        adjudicacion
    ]);
}

function crearFila(celdas) {
    const fila = document.createElement('tr');
    fila.append(...celdas);
    return fila;
}

function celdaTexto(texto, clases = '') {
    const celda = document.createElement('td');
    celda.className = clases;
    celda.textContent = texto;
    return celda;
}

function celdaSubasta(id, titulo) {
    const celda = document.createElement('td');
    const enlace = document.createElement('a');
    enlace.href = `subasta.html?id=${id}`;
    enlace.textContent = titulo;
    celda.appendChild(enlace);
    return celda;
}

function celdaEtiqueta(texto, clases) {
    const celda = document.createElement('td');
    const etiqueta = document.createElement('span');
    etiqueta.className = `badge ${clases ?? 'text-bg-secondary'}`;
    etiqueta.textContent = texto;
    celda.appendChild(etiqueta);
    return celda;
}
