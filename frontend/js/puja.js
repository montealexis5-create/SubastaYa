(() => {
    'use strict';

    let detalle = null;
    let montoMinimo = 0;
    let situacionDibujada = null;
    let montoTocado = false;
    let recienOferto = false;
    let estadoAnterior = null;

    document.addEventListener('subasta:cargada', evento => {
        const arranco = estadoAnterior === 'Programada' && evento.detail.estado === 'Activa';
        estadoAnterior = evento.detail.estado;

        detalle = evento.detail;
        montoMinimo = detalle.montoMinimo;
        dibujar();

        if (arranco) avisarInicio(situacionDibujada);
    });

    document.addEventListener('sala:actualizada', evento => {
        if (!detalle) return;
        detalle = { ...detalle, ...evento.detail };
        montoMinimo = evento.detail.montoMinimo;
        dibujar();
    });

    function puedeRecibirOfertas() {
        return detalle.estado === 'Activa' && !estaVencida(detalle);
    }

    function situacionActual() {
        if (!puedeRecibirOfertas()) return 'cerrada';

        const usuario = obtenerUsuario();
        if (!usuario) return 'anonimo';
        if (usuario.seudonimo === detalle.seudonimoVendedor) return 'vendedor';
        if (usuario.seudonimo === detalle.seudonimoLider) return 'lider';
        return 'puede';
    }

    function dibujar() {
        const situacion = situacionActual();

        if (situacion === situacionDibujada) {
            if (situacion === 'puede') actualizarMinimo();
            return;
        }

        situacionDibujada = situacion;
        montoTocado = false;

        const contenedor = document.getElementById('formulario-puja');
        contenedor.replaceChildren(construir(situacion));
    }

    function construir(situacion) {
        if (situacion === 'cerrada') return document.createDocumentFragment();
        if (situacion === 'anonimo') return enlaceDeIngreso();
        if (situacion === 'vendedor') {
            return aviso('Es tu subasta: no podés ofertar en lo que publicaste.');
        }
        if (situacion === 'lider') {
            const texto = recienOferto
                ? 'Oferta registrada: vas ganando. Esperá a que alguien te supere para volver a ofertar.'
                : 'Vas ganando. Esperá a que alguien te supere para volver a ofertar.';
            recienOferto = false;
            return aviso(texto, 'success');
        }
        return formulario();
    }

    function aviso(texto, tipo = 'secondary') {
        const bloque = document.createElement('p');
        bloque.className = `alert alert-${tipo} py-2 mb-2`;
        bloque.textContent = texto;
        return bloque;
    }

    function enlaceDeIngreso() {
        const bloque = document.createElement('div');
        bloque.className = 'mb-2';

        const texto = document.createElement('p');
        texto.className = 'mb-2';
        texto.textContent = 'Para ofertar necesitás iniciar sesión.';

        const enlace = document.createElement('a');
        enlace.className = 'btn btn-primary';
        enlace.href = `login.html?volverA=${encodeURIComponent('subasta.html' + location.search)}`;
        enlace.textContent = 'Ingresar para ofertar';

        bloque.append(texto, enlace);
        return bloque;
    }

    function formulario() {
        const bloque = document.createElement('div');

        const etiqueta = document.createElement('label');
        etiqueta.className = 'form-label';
        etiqueta.htmlFor = 'monto-puja';
        etiqueta.textContent = 'Tu oferta';

        const grupo = document.createElement('div');
        grupo.className = 'input-group';

        const simbolo = document.createElement('span');
        simbolo.className = 'input-group-text';
        simbolo.textContent = '$';

        const campo = document.createElement('input');
        campo.type = 'number';
        campo.className = 'form-control';
        campo.id = 'monto-puja';
        campo.step = '0.01';
        campo.min = String(montoMinimo);
        campo.value = String(montoMinimo);
        campo.addEventListener('input', () => { montoTocado = true; });
        limitarADosDecimales(campo);

        const boton = document.createElement('button');
        boton.type = 'submit';
        boton.className = 'btn btn-primary';
        boton.id = 'boton-ofertar';

        const spinner = document.createElement('span');
        spinner.className = 'spinner-border spinner-border-sm me-1';
        spinner.id = 'spinner-ofertar';
        spinner.hidden = true;

        boton.append(spinner, document.createTextNode('Ofertar'));
        grupo.append(simbolo, campo, boton);

        const ayuda = document.createElement('p');
        ayuda.className = 'form-text mb-2';
        ayuda.id = 'ayuda-puja';
        ayuda.textContent = `Mínimo: ${formatearPesos(montoMinimo)}`;

        const error = document.createElement('p');
        error.className = 'text-danger small mb-2';
        error.id = 'error-puja';
        error.hidden = true;

        const conflicto = document.createElement('div');
        conflicto.id = 'conflicto-puja';

        const formulario = document.createElement('form');
        formulario.noValidate = true;
        formulario.addEventListener('submit', evento => {
            evento.preventDefault();
            ofertar(campo.value);
        });
        formulario.append(etiqueta, grupo, ayuda, error, conflicto);

        bloque.append(formulario);
        return bloque;
    }

    function actualizarMinimo() {
        const campo = document.getElementById('monto-puja');
        const ayuda = document.getElementById('ayuda-puja');
        if (!campo || !ayuda) return;

        campo.min = String(montoMinimo);
        ayuda.textContent = `Mínimo: ${formatearPesos(montoMinimo)}`;

        if (!montoTocado) {
            campo.value = String(montoMinimo);
            return;
        }

        if (Number(campo.value) < montoMinimo) {
            mostrarError(`Otra oferta subió el mínimo a ${formatearPesos(montoMinimo)}.`);
        }
    }

    function avisarInicio(situacion) {
        const textos = {
            puede: 'La subasta comenzó: ya podés ofertar.',
            vendedor: 'Tu subasta comenzó: ya puede recibir ofertas.',
            anonimo: 'La subasta comenzó. Ingresá para ofertar.'
        };

        mostrarAvisoFlotante(textos[situacion] ?? 'La subasta comenzó.', {
            tipo: 'success',
            icono: '🔔'
        });
    }

    function avisarOfertaRegistrada(puja) {
        if (puja.tiempoExtendido) return;

        mostrarAvisoFlotante(`Tu oferta de ${formatearPesos(puja.monto)} quedó registrada.`, {
            tipo: 'success',
            icono: '✅'
        });
    }

    function mostrarError(texto) {
        const error = document.getElementById('error-puja');
        if (!error) return;
        error.textContent = texto;
        error.hidden = false;
    }

    function limpiarMensajes() {
        const error = document.getElementById('error-puja');
        if (error) error.hidden = true;
        document.getElementById('conflicto-puja')?.replaceChildren();
    }

    async function ofertar(valor) {
        limpiarMensajes();

        const monto = Number(valor);
        if (!valor || Number.isNaN(monto)) {
            mostrarError('Escribí cuánto querés ofertar.');
            return;
        }
        if (monto < montoMinimo) {
            mostrarError(`La oferta debe ser de al menos ${formatearPesos(montoMinimo)}.`);
            return;
        }

        alternarCargando('boton-ofertar', 'spinner-ofertar', true);

        recienOferto = true;

        try {
            const puja = await api.post(`/auctions/${detalle.id}/bids`, { monto });

            registrarPujaEnSala(puja);
            avisarOfertaRegistrada(puja);
            montoTocado = false;
        } catch (error) {
            recienOferto = false;
            manejarError(error);
        } finally {
            alternarCargando('boton-ofertar', 'spinner-ofertar', false);
        }
    }

    function manejarError(error) {
        if (error.status === 409 && error.cuerpo && error.cuerpo.montoMinimo) {
            mostrarConflicto(error.cuerpo);
            return;
        }

        if (error.status === 400 && error.erroresPorCampo) {
            const mensajes = Object.values(error.erroresPorCampo)[0];
            mostrarError(Array.isArray(mensajes) ? mensajes[0] : String(mensajes));
            return;
        }

        mostrarError(error.message);
    }

    function mostrarConflicto(conflicto) {
        const contenedor = document.getElementById('conflicto-puja');
        if (!contenedor) return;

        montoMinimo = conflicto.montoMinimo;

        const bloque = document.createElement('div');
        bloque.className = 'alert alert-warning py-2';

        const texto = document.createElement('p');
        texto.className = 'mb-2';
        texto.textContent =
            `Alguien ofertó antes que vos. La oferta actual es de ${formatearPesos(conflicto.pujaActual)}.`;

        const reintentar = document.createElement('button');
        reintentar.type = 'button';
        reintentar.className = 'btn btn-primary btn-sm me-2';
        reintentar.textContent = `Ofertar ${formatearPesos(conflicto.montoMinimo)}`;
        reintentar.addEventListener('click', () => ofertar(String(conflicto.montoMinimo)));

        const cancelar = document.createElement('button');
        cancelar.type = 'button';
        cancelar.className = 'btn btn-outline-secondary btn-sm';
        cancelar.textContent = 'Cancelar';
        cancelar.addEventListener('click', () => limpiarMensajes());

        bloque.append(texto, reintentar, cancelar);
        contenedor.replaceChildren(bloque);

        const campo = document.getElementById('monto-puja');
        if (campo) {
            campo.min = String(conflicto.montoMinimo);
            campo.value = String(conflicto.montoMinimo);
            montoTocado = false;
        }
        const ayuda = document.getElementById('ayuda-puja');
        if (ayuda) ayuda.textContent = `Mínimo: ${formatearPesos(conflicto.montoMinimo)}`;
    }
})();
