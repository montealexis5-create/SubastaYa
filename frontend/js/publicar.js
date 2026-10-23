document.addEventListener('DOMContentLoaded', () => {
    cargarCategorias();
    configurarVistaPrevia();
    limitarADosDecimales(document.getElementById('precio-base'));
    limitarADosDecimales(document.getElementById('incremento'));
    configurarEnvio();
});

async function cargarCategorias() {
    try {
        await poblarCategorias(document.getElementById('categoria'));
    } catch {
        mostrarAlerta('alerta-publicar', 'No se pudieron cargar las categorías. Recargá la página.', 'danger');
    }
}

function configurarVistaPrevia() {
    const campoUrl = document.getElementById('url-imagen');
    const vistaPrevia = document.getElementById('vista-previa');

    campoUrl.addEventListener('input', () => {
        const url = campoUrl.value.trim();
        if (!url) {
            vistaPrevia.hidden = true;
            return;
        }
        vistaPrevia.src = url;
        vistaPrevia.hidden = false;
    });

    vistaPrevia.addEventListener('error', () => {
        vistaPrevia.hidden = true;
    });
}

function valor(id) {
    return document.getElementById(id).value;
}

const CAMPOS = {
    Titulo: 'titulo', Descripcion: 'descripcion', UrlImagen: 'url-imagen',
    CategoriaId: 'categoria', PrecioBase: 'precio-base',
    IncrementoMinimo: 'incremento', FechaInicio: 'fecha-inicio', FechaFin: 'fecha-fin'
};

function limpiarErrores(formulario) {
    formulario.querySelectorAll('.form-control, .form-select').forEach(campo => {
        campo.setCustomValidity('');
        campo.classList.remove('is-invalid');
    });
}

function marcarCampoConError(idCampo, mensaje) {
    const campo = document.getElementById(idCampo);
    campo.setCustomValidity(mensaje);
    campo.classList.add('is-invalid');
    document.getElementById(`error-${idCampo}`).textContent = mensaje;
}

function mostrarErroresDeCampo(erroresPorCampo) {
    Object.entries(erroresPorCampo).forEach(([clave, mensajes]) => {
        const idCampo = CAMPOS[clave];
        const mensaje = Array.isArray(mensajes) ? mensajes[0] : mensajes;
        if (idCampo) {
            marcarCampoConError(idCampo, mensaje);
        } else {
            mostrarAlerta('alerta-publicar', mensaje, 'danger');
        }
    });
}

function configurarEnvio() {
    const formulario = document.getElementById('formulario-publicar');

    formulario.addEventListener('submit', async (evento) => {
        evento.preventDefault();
        ocultarAlerta('alerta-publicar');
        limpiarErrores(formulario);

        formulario.classList.add('was-validated');
        if (!formulario.checkValidity()) {
            formulario.querySelector(':invalid')?.focus();
            return;
        }

        const datos = {
            titulo: valor('titulo').trim(),
            descripcion: valor('descripcion').trim(),
            urlImagen: valor('url-imagen').trim(),
            categoriaId: Number(valor('categoria')),
            precioBase: Number(valor('precio-base')),
            incrementoMinimo: Number(valor('incremento')),
            fechaInicio: new Date(valor('fecha-inicio')).toISOString(),
            fechaFin: new Date(valor('fecha-fin')).toISOString()
        };

        alternarCargando('boton-publicar', 'spinner-publicar', true);

        try {
            const creada = await api.post('/auctions', datos);
            window.location.href = `subasta.html?id=${creada.id}`;
        } catch (error) {
            // Las reglas de negocio las valida el back; el front solo muestra lo que devuelve.
            if (error.status === 400 && error.erroresPorCampo) {
                mostrarErroresDeCampo(error.erroresPorCampo);
            } else {
                mostrarAlerta('alerta-publicar', error.message, 'danger');
            }
        } finally {
            alternarCargando('boton-publicar', 'spinner-publicar', false);
        }
    });
}
