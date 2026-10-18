const filtros = {
    busqueda: '', estado: '', categoriaId: '', precioMin: '', precioMax: '',
    orden: '', pagina: 1, tamano: 9
};

document.addEventListener('DOMContentLoaded', () => {
    conectarFiltros();
    cargarCategorias();
    cargarCatalogo();
});

const SECCIONES = {
    cargando: 'cargando-catalogo',
    exito: ['grilla-catalogo', 'paginacion-catalogo'],
    vacio: 'vacio-catalogo'
};

function mostrarEstado(estado) {
    mostrarSecciones(SECCIONES, estado, 'alerta-catalogo');

    if (estado === 'vacio') {
        const hayFiltros = ['busqueda','estado','categoriaId','precioMin','precioMax']
            .some(clave => filtros[clave] !== '');

        document.querySelector('#vacio-catalogo p').textContent = hayFiltros
            ? 'No hay subastas que coincidan con el filtro.'
            : 'No hay subastas publicadas todavía.';
    }
}

async function cargarCatalogo() {
    mostrarEstado('cargando');

    try {
        const datos = await api.get(`/auctions?${armarQuery()}`);

        if (datos.items.length === 0) {
            mostrarEstado('vacio');
            return;
        }

        dibujarGrilla(datos.items);
        mostrarEstado('exito');
        dibujarPaginacion(datos);
    } catch (error) {
        mostrarEstado('error');
        mostrarAlerta('alerta-catalogo', error.message);
    }
}

function crearTarjeta(subasta) {
    const columna = document.createElement('div');
    columna.className = 'col';

    columna.innerHTML = `
        <div class="card h-100 tarjeta-subasta">
            <div class="foto-subasta">
                <img class="foto-subasta__fondo" alt="" aria-hidden="true">
                <img class="foto-subasta__frente" alt="">
            </div>
            <div class="card-body d-flex flex-column">
                <span class="badge text-bg-secondary align-self-start mb-2 js-categoria"></span>
                <h2 class="h6 card-title js-titulo"></h2>
                <p class="mb-1 small text-muted js-ofertas"></p>
                <p class="fs-5 fw-semibold mb-2"><span class="js-puja"></span></p>
                <p class="mb-3 js-tiempo"></p>
                <a class="btn btn-primary mt-auto js-detalle">Ver subasta</a>
            </div>
        </div>`;

    columna.querySelector('.js-titulo').textContent    = subasta.titulo;
    columna.querySelector('.js-categoria').textContent = subasta.nombreCategoria;
    columna.querySelector('.js-ofertas').textContent = textoDeOfertas(subasta.cantidadOfertas);
    columna.querySelector('.js-puja').textContent      = formatearPesos(subasta.pujaActual);

    const fotoFondo = columna.querySelector('.foto-subasta__fondo');
    const fotoFrente = columna.querySelector('.foto-subasta__frente');
    fotoFondo.src = subasta.urlImagen;
    fotoFrente.src = subasta.urlImagen;
    fotoFrente.alt = subasta.titulo;

    const placeholder = 'data:image/svg+xml,' + encodeURIComponent(
        '<svg xmlns="http://www.w3.org/2000/svg" width="400" height="180">' +
        '<rect width="100%" height="100%" fill="#dee2e6"/></svg>');
    fotoFondo.addEventListener('error', () => { fotoFondo.src = placeholder; });
    fotoFrente.addEventListener('error', () => { fotoFrente.src = placeholder; });

    dibujarTiempoTarjeta(columna.querySelector('.js-tiempo'), subasta);
    columna.querySelector('.js-detalle').href = `subasta.html?id=${subasta.id}`;

    return columna;
}

function dibujarTiempoTarjeta(elemento, subasta) {
    if (subasta.estado === 'Programada') {
        elemento.textContent = `Próxima · ${formatearFecha(subasta.fechaInicio)}`;
        return;
    }

    if (estaVencida(subasta)) {
        elemento.textContent = 'Finalizando…';
        return;
    }

    if (subasta.estado === 'Activa') {
        const span = document.createElement('span');
        span.className = 'contador js-contador';
        span.dataset.fechaFin = subasta.fechaFin;
        elemento.appendChild(span);
        return;
    }

    elemento.textContent = subasta.estado === 'Finalizada' ? 'Finalizada' : 'Desierta';
}

function armarQuery() {
    const parametros = new URLSearchParams();
    Object.entries(filtros).forEach(([clave, valor]) => {
        if (valor !== '' && valor !== null && valor !== undefined) {
            parametros.append(clave, valor);
        }
    });
    return parametros.toString();
}

function dibujarGrilla(items) {
    const grilla = document.getElementById('grilla-catalogo');
    grilla.replaceChildren();

    items.forEach(subasta => grilla.appendChild(crearTarjeta(subasta)));
}

function dibujarPaginacion({ paginaActual, totalPaginas }) {
    const barra = document.getElementById('paginacion-catalogo');

    if (totalPaginas <= 1) { barra.hidden = true; return; }

    document.getElementById('pagina-actual').textContent = `${paginaActual} de ${totalPaginas}`;
    document.getElementById('pagina-anterior').parentElement
        .classList.toggle('disabled', paginaActual <= 1);
    document.getElementById('pagina-siguiente').parentElement
        .classList.toggle('disabled', paginaActual >= totalPaginas);
}

document.getElementById('pagina-anterior').addEventListener('click', () => {
    if (filtros.pagina > 1) { filtros.pagina--; cargarCatalogo(); }
});
document.getElementById('pagina-siguiente').addEventListener('click', () => {
    filtros.pagina++; cargarCatalogo();
});
function conectarFiltros() {
    const controles = {
        'filtro-busqueda': 'busqueda',
        'filtro-estado': 'estado',
        'filtro-categoria': 'categoriaId',
        'filtro-precio-min': 'precioMin',
        'filtro-precio-max': 'precioMax',
        'filtro-orden': 'orden'
    };

    Object.entries(controles).forEach(([id, clave]) => {
        document.getElementById(id).addEventListener('change', evento => {
            filtros[clave] = evento.target.value;
            aplicarFiltros();
        });
    });

    document.getElementById('limpiar-filtros').addEventListener('click', () => {
        document.getElementById('filtros-catalogo').reset();
        Object.keys(controles).forEach(id => { filtros[controles[id]] = ''; });
        aplicarFiltros();
    });
}

function aplicarFiltros() {
    filtros.pagina = 1;
    cargarCatalogo();
}

async function cargarCategorias() {
    try {
        await poblarCategorias(document.getElementById('filtro-categoria'));
    } catch {
        // Si falla, queda solo la opcion "Todas" y el catalogo se puede usar igual.
        return;
    }
}
