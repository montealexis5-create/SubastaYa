document.addEventListener('DOMContentLoaded', () => {
    const formulario = document.getElementById('formulario-registro');

    if (haySesion()) {
        window.location.replace('index.html');
        return;
    }

    formulario.addEventListener('submit', async (evento) => {
        evento.preventDefault();

        if (!formulario.checkValidity()) {
            formulario.classList.add('was-validated');
            return;
        }

        ocultarAlerta('alerta-registro');
        limpiarErroresDeCampo();
        alternarCargando('boton-registrar', 'spinner-registrar', true);

        const datos = {
            email: document.getElementById('email').value.trim(),
            password: document.getElementById('password').value,
            nombre: document.getElementById('nombre').value.trim(),
            seudonimo: document.getElementById('seudonimo').value.trim()
        };

        try {
            await api.post('/users', datos);

            const sesion = await api.post('/auth/sessions', {
                email: datos.email,
                password: datos.password
            });

            guardarSesion(sesion);
            window.location.href = 'index.html';
        } catch (error) {
            const marcados = error.status === 400 && error.erroresPorCampo
                ? mostrarErroresDeCampo(error.erroresPorCampo)
                : 0;

            mostrarAlerta('alerta-registro',
                marcados > 0 ? 'Revisá los datos marcados.' : error.message);
        } finally {
            alternarCargando('boton-registrar', 'spinner-registrar', false);
        }
    });

    function mostrarErroresDeCampo(errores) {
        let marcados = 0;
        Object.entries(errores).forEach(([campo, mensajes]) => {
            const nombre = campo.toLowerCase();
            const entrada = document.getElementById(nombre);
            if (!entrada) return;

            entrada.classList.add('is-invalid');
            const destino = document.getElementById(`error-${nombre}`);
            if (destino) destino.textContent = mensajes[0];
            marcados++;
        });
        return marcados;
    }

    formulario.querySelectorAll('.invalid-feedback').forEach(e => {
        e.dataset.mensajeOriginal = e.textContent;
    });

    function limpiarErroresDeCampo() {
        formulario.querySelectorAll('.is-invalid')
            .forEach(e => e.classList.remove('is-invalid'));
        formulario.querySelectorAll('.invalid-feedback')
            .forEach(e => { e.textContent = e.dataset.mensajeOriginal || ''; });
    }
});
