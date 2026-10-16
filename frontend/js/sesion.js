function haySesion() {
    return obtenerToken() !== null;
}

let paginaPrivada = false;

function requiereSesion() {
    if (haySesion()) {
        paginaPrivada = true;
        return true;
    }

    const destino = window.location.pathname.split('/').pop() + window.location.search;
    window.location.replace(`login.html?volverA=${encodeURIComponent(destino)}`);
    return false;
}

function cerrarSesion() {
    borrarSesion();
    window.location.href = 'login.html';
}

function ajustarNavegacion() {
    const logueado = haySesion();

    document.querySelectorAll('[data-sesion="privado"]')
        .forEach(el => { el.hidden = !logueado; });

    document.querySelectorAll('[data-sesion="anonimo"]')
        .forEach(el => { el.hidden = logueado; });

    const usuario = obtenerUsuario();
    const etiqueta = document.getElementById('usuario-actual');
    if (etiqueta && usuario) etiqueta.textContent = usuario.seudonimo;

    const boton = document.getElementById('boton-cerrar-sesion');
    if (boton) boton.addEventListener('click', cerrarSesion);
}

document.addEventListener('DOMContentLoaded', ajustarNavegacion);

window.addEventListener('storage', evento => {
    if (evento.key !== CLAVE_TOKEN) return;
    if (!haySesion() && paginaPrivada) {
        window.location.href = 'login.html';
    } else {
        ajustarNavegacion();
    }
});
