document.addEventListener('DOMContentLoaded', () => {
    const formulario = document.getElementById('formulario-login');
    const parametros = new URLSearchParams(window.location.search);

    if (parametros.get('vencida')) {
        mostrarAlerta('alerta-login', 'Tu sesión expiró. Volvé a ingresar.', 'warning');
    }

    if (haySesion()) {
        window.location.replace(destinoPosterior());
        return;
    }

    formulario.addEventListener('submit', async (evento) => {
        evento.preventDefault();

        if (!formulario.checkValidity()) {
            formulario.classList.add('was-validated');
            return;
        }

        ocultarAlerta('alerta-login');
        alternarCargando('boton-ingresar', 'spinner-ingresar', true);

        try {
            const sesion = await api.post('/auth/sessions', {
                email: document.getElementById('email').value.trim(),
                password: document.getElementById('password').value
            });

            guardarSesion(sesion);
            window.location.href = destinoPosterior();
        } catch (error) {
            const texto = error.status === 401
                ? 'Email o contraseña incorrectos.'
                : error.message;
            mostrarAlerta('alerta-login', texto);
        } finally {
            alternarCargando('boton-ingresar', 'spinner-ingresar', false);
        }
    });

    function destinoPosterior() {
        const volverA = parametros.get('volverA');
        return volverA && /^[a-z0-9-]+\.html(\?.*)?$/i.test(volverA) ? volverA : 'index.html';
    }
});
