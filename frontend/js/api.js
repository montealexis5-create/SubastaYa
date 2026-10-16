const CLAVE_TOKEN = 'subastaya.token';
const CLAVE_USUARIO = 'subastaya.usuario';

function obtenerToken() {
    return localStorage.getItem(CLAVE_TOKEN);
}

function obtenerUsuario() {
    const crudo = localStorage.getItem(CLAVE_USUARIO);
    if (!crudo) return null;
    try {
        return JSON.parse(crudo);
    } catch {
        borrarSesion();
        return null;
    }
}

function guardarSesion(sesion) {
    localStorage.setItem(CLAVE_TOKEN, sesion.token);
    localStorage.setItem(CLAVE_USUARIO, JSON.stringify(sesion.usuario));
}

function borrarSesion() {
    localStorage.removeItem(CLAVE_TOKEN);
    localStorage.removeItem(CLAVE_USUARIO);
}

class ErrorApi extends Error {
    constructor(status, mensaje, cuerpo) {
        super(mensaje);
        this.name = 'ErrorApi';
        this.status = status;
        this.cuerpo = cuerpo;
    }

    get erroresPorCampo() {
        return this.cuerpo && this.cuerpo.errors ? this.cuerpo.errors : null;
    }
}

async function llamarApi(ruta, opciones = {}) {
    const headers = { ...(opciones.headers || {}) };

    if (opciones.body !== undefined) {
        headers['Content-Type'] = 'application/json';
    }

    const token = obtenerToken();
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    let respuesta;
    try {
        respuesta = await fetch(`${API_BASE}${ruta}`, { ...opciones, headers });
    } catch {
        throw new ErrorApi(0, 'No se pudo conectar con el servidor.', null);
    }

    if (!respuesta.ok) {
        if (respuesta.status === 401 && obtenerToken()) {
            borrarSesion();
            window.location.replace('login.html?vencida=1');
        }

        const cuerpo = await leerCuerpo(respuesta);
        throw new ErrorApi(respuesta.status, mensajeDeError(respuesta.status, cuerpo), cuerpo);
    }

    return leerCuerpo(respuesta);
}

async function leerCuerpo(respuesta) {
    const texto = await respuesta.text();
    if (!texto) return null;
    try {
        return JSON.parse(texto);
    } catch {
        return null;
    }
}

function mensajeDeError(status, cuerpo) {
    if (cuerpo && cuerpo.mensaje) return cuerpo.mensaje;

    if (cuerpo && cuerpo.errors) {
        const primero = Object.values(cuerpo.errors)[0];
        if (Array.isArray(primero) && primero.length) return primero[0];
    }

    if (status === 401) return 'Tenés que iniciar sesión para hacer esto.';
    if (status === 403) return 'No tenés permiso para hacer esto.';
    if (status === 404) return 'No encontramos lo que buscabas.';
    if (status >= 500) return 'El servidor tuvo un problema. Intentá de nuevo en un momento.';
    return 'Ocurrió un error inesperado.';
}

const api = {
    get: (ruta) => llamarApi(ruta),
    post: (ruta, cuerpo) => llamarApi(ruta, { method: 'POST', body: JSON.stringify(cuerpo) })
};
