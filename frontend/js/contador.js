const UN_MINUTO = 60 * 1000;
const CINCO_MINUTOS = 5 * UN_MINUTO;
const COLORES = ['contador-normal', 'contador-alerta', 'contador-critico'];

function pintar(elemento, clase) {
    elemento.classList.remove(...COLORES);
    elemento.classList.add(clase);
}

function actualizarContadores() {
    document.querySelectorAll('[data-fecha-fin]').forEach(elemento => {
        const fin = new Date(elemento.dataset.fechaFin);
        const restante = fin - ahoraServidor();

        if (restante <= 0) {
            elemento.textContent = 'Finalizando…';
            pintar(elemento, 'contador-critico');
            return;
        }

        elemento.textContent = formatear(restante);
        pintar(elemento, restante <= UN_MINUTO ? 'contador-critico' :
                          restante <= CINCO_MINUTOS ? 'contador-alerta' :
                          'contador-normal');
    });
}

function formatear(milisegundos) {
    const total = Math.floor(milisegundos / 1000);
    const dias = Math.floor(total / 86400);
    const horas = Math.floor((total % 86400) / 3600);
    const minutos = Math.floor((total % 3600) / 60);
    const segundos = total % 60;

    if (dias > 0) return `${dias}d ${horas}h`;
    return `${dos(horas)}:${dos(minutos)}:${dos(segundos)}`;
}

const dos = numero => String(numero).padStart(2, '0');

document.addEventListener('DOMContentLoaded', () => {
    actualizarContadores();
    setInterval(actualizarContadores, 1000);
});
