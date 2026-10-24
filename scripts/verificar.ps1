<#
.SYNOPSIS
    Verificacion automatica de punta a punta contra la API.

.DESCRIPTION
    Comprueba en una sola corrida lo que antes se probaba a mano:
    codigos HTTP, montoMinimo desde el back, oferta valida e invalida,
    saldo retenido y el efecto de cada movimiento de la billetera.

    Crea su propio usuario de prueba con saldo simulado y oferta sobre una subasta
    activa. No borra nada. Hace falta la base levantada (docker compose up -d)
    y la base sembrada.

    Desde la raiz del repo:

        powershell -ExecutionPolicy Bypass -File scripts/verificar.ps1

    Con -Completo tambien compila, corre los tests y levanta la API sola:

        powershell -ExecutionPolicy Bypass -File scripts/verificar.ps1 -Completo

.PARAMETER Api
    URL base de la API.

.PARAMETER Completo
    Compila, corre dotnet test y levanta la API antes de verificar.
#>
param(
    [string] $Api = 'http://localhost:5093/api/v1',
    [switch] $Completo
)

$ErrorActionPreference = 'Stop'
$script:fallas = 0
$procesoApi = $null

function Resultado([string] $Nombre, [bool] $Ok, [string] $Detalle = '') {
    if ($Ok) { Write-Host "  OK     $Nombre" -ForegroundColor Green }
    else {
        Write-Host "  FALLA  $Nombre  $Detalle" -ForegroundColor Red
        $script:fallas++
    }
}

# Devuelve el codigo HTTP y el cuerpo ya convertido, sin cortar el script en los 4xx.
function Llamar([string] $Metodo, [string] $Ruta, $Cuerpo = $null, [string] $Token = $null) {
    $parametros = @{ Method = $Metodo; Uri = "$Api$Ruta"; ContentType = 'application/json'; UseBasicParsing = $true }
    if ($null -ne $Cuerpo) { $parametros.Body = ($Cuerpo | ConvertTo-Json -Compress) }
    if ($Token) { $parametros.Headers = @{ Authorization = "Bearer $Token" } }
    try {
        $respuesta = Invoke-WebRequest @parametros
        $codigo = [int]$respuesta.StatusCode
        $texto = $respuesta.Content
    }
    catch {
        if ($null -eq $_.Exception.Response) { throw }
        $codigo = [int]$_.Exception.Response.StatusCode
        $texto = ''
        if ($_.ErrorDetails -and $_.ErrorDetails.Message) { $texto = $_.ErrorDetails.Message }
    }
    $datos = $null
    if ($texto) { try { $datos = $texto | ConvertFrom-Json } catch { } }
    [pscustomobject]@{ Codigo = $codigo; Datos = $datos }
}

function Esperar-Api {
    for ($i = 0; $i -lt 60; $i++) {
        try { if ((Llamar GET '/auctions?tamano=1').Codigo -eq 200) { return $true } } catch { }
        Start-Sleep -Seconds 2
    }
    return $false
}

try {
    if ($Completo) {
        Write-Host 'Compilacion y tests' -ForegroundColor Cyan
        dotnet build --nologo -v q
        Resultado 'dotnet build' ($LASTEXITCODE -eq 0)
        dotnet test --nologo -v q
        Resultado 'dotnet test' ($LASTEXITCODE -eq 0)
        if ($script:fallas -gt 0) { exit 1 }

        $registro = Join-Path $env:TEMP 'subastaya-api.log'
        $procesoApi = Start-Process dotnet -PassThru -WindowStyle Hidden `
            -ArgumentList 'run', '--project', 'src/SubastaYa.Api', '--launch-profile', 'http', '--no-build' `
            -RedirectStandardOutput $registro -RedirectStandardError "$registro.err"
    }

    Write-Host 'API' -ForegroundColor Cyan
    $arriba = Esperar-Api
    Resultado 'La API responde (la base tiene que estar levantada)' $arriba "Registro: $env:TEMP\subastaya-api.log"
    if (-not $arriba) { exit 1 }

    Write-Host 'Codigos HTTP' -ForegroundColor Cyan
    $lista = Llamar GET '/auctions?estado=Activa&tamano=100'
    Resultado 'GET /auctions devuelve 200' ($lista.Codigo -eq 200)
    Resultado 'GET /auctions/999999 devuelve 404' ((Llamar GET '/auctions/999999').Codigo -eq 404)
    Resultado 'GET /wallets/me sin token devuelve 401' ((Llamar GET '/wallets/me').Codigo -eq 401)

    Write-Host 'Subasta de prueba (autogenerada, no depende del seed)' -ForegroundColor Cyan
    $sufijoV = [guid]::NewGuid().ToString('N').Substring(0, 6)
    $passV = 'Verif' + (Get-Random -Minimum 1000 -Maximum 9999)
    $emailVend = "vendedor-$sufijoV@prueba.com"
    $altaVend = Llamar POST '/users' @{ email = $emailVend; password = $passV; nombre = "Vendedor $sufijoV"; seudonimo = "vend-$sufijoV" }
    Resultado 'POST /users crea el vendedor (2xx)' ($altaVend.Codigo -ge 200 -and $altaVend.Codigo -lt 300) "codigo $($altaVend.Codigo)"
    $tokenVend = (Llamar POST '/auth/sessions' @{ email = $emailVend; password = $passV }).Datos.token

    $categorias = (Llamar GET '/categories').Datos
    $categoriaId = if ($categorias) { $categorias[0].id } else { 1 }

    $inicio = (Get-Date).AddMinutes(-1).ToString('o')
    $fin = (Get-Date).AddMinutes(30).ToString('o')
    $pub = Llamar POST '/auctions' @{
        titulo = "Subasta de verificacion $sufijoV"
        descripcion = 'Creada por el script para no depender del seed.'
        urlImagen = 'https://example.com/x.jpg'
        categoriaId = $categoriaId
        precioBase = 1000
        incrementoMinimo = 100
        fechaInicio = $inicio
        fechaFin = $fin
    } $tokenVend
    Resultado 'POST /auctions crea la subasta (201)' ($pub.Codigo -eq 201) "codigo $($pub.Codigo)"
    if ($pub.Codigo -ne 201) { exit 1 }
    $subastaId = $pub.Datos.id
    Resultado 'La subasta nace Activa' ($pub.Datos.estado -eq 'Activa') "estado $($pub.Datos.estado)"

    Write-Host 'Reglas de negocio rechazadas por el back (422)' -ForegroundColor Cyan
    function PublicarMal([string] $desc, [hashtable] $cambios) {
        $cuerpo = @{
            titulo = "Mala $sufijoV"; descripcion = 'prueba'; urlImagen = 'https://example.com/x.jpg'
            categoriaId = $categoriaId; precioBase = 1000; incrementoMinimo = 100
            fechaInicio = (Get-Date).AddMinutes(-1).ToString('o')
            fechaFin = (Get-Date).AddMinutes(30).ToString('o')
        }
        foreach ($k in $cambios.Keys) { $cuerpo[$k] = $cambios[$k] }
        $r = Llamar POST '/auctions' $cuerpo $tokenVend
        Resultado $desc ($r.Codigo -eq 422) "codigo $($r.Codigo)"
    }
    PublicarMal 'Incremento mayor al precio base -> 422' @{ incrementoMinimo = 2000 }
    PublicarMal 'Fecha inicio en el pasado -> 422' @{ fechaInicio = (Get-Date).AddMinutes(-30).ToString('o') }
    PublicarMal 'Fecha fin anterior a la de inicio -> 422' @{ fechaInicio = (Get-Date).AddMinutes(30).ToString('o'); fechaFin = (Get-Date).AddMinutes(10).ToString('o') }
    PublicarMal 'Fecha fin mas alla de 365 dias -> 422' @{ fechaFin = (Get-Date).AddDays(400).ToString('o') }

    Write-Host 'Configuracion de publicacion (API)' -ForegroundColor Cyan
    $cfg = Llamar GET '/config/auctions'
    Resultado 'GET /config/auctions devuelve 200' ($cfg.Codigo -eq 200)
    Resultado 'Trae duracionMaximaDias y toleranciaInicioMinutos' ($cfg.Datos.duracionMaximaDias -gt 0 -and $cfg.Datos.toleranciaInicioMinutos -ge 0)

    Write-Host "Subasta $subastaId" -ForegroundColor Cyan
    $detalle = Llamar GET "/auctions/$subastaId"
    $minimo = [decimal]$detalle.Datos.montoMinimo
    $esperado = [decimal]$detalle.Datos.pujaActual + [decimal]$detalle.Datos.incrementoMinimo
    Resultado 'montoMinimo viene del back y es pujaActual + incrementoMinimo' ($minimo -eq $esperado) "recibido $minimo, esperado $esperado"
    Resultado 'El detalle trae fechaServidor (hora del server)' ($null -ne $detalle.Datos.fechaServidor)

    Write-Host 'Usuario de prueba' -ForegroundColor Cyan
    $sufijo = [guid]::NewGuid().ToString('N').Substring(0, 6)
    $password = 'Verif' + (Get-Random -Minimum 1000 -Maximum 9999)
    $email = "verificar-$sufijo@prueba.com"
    $alta = Llamar POST '/users' @{ email = $email; password = $password; nombre = "Verificador $sufijo"; seudonimo = "verif-$sufijo" }
    Resultado 'POST /users crea el usuario (2xx)' ($alta.Codigo -ge 200 -and $alta.Codigo -lt 300) "codigo $($alta.Codigo)"
    $sesion = Llamar POST '/auth/sessions' @{ email = $email; password = $password }
    Resultado 'POST /auth/sessions inicia sesion (2xx)' ($sesion.Codigo -ge 200 -and $sesion.Codigo -lt 300)
    $token = $sesion.Datos.token

    $deposito = Llamar POST '/wallets/me/deposits' @{ monto = 10000000 } $token
    Resultado 'Deposito acreditado (2xx)' ($deposito.Codigo -ge 200 -and $deposito.Codigo -lt 300) "codigo $($deposito.Codigo)"
    Resultado 'Deposito con monto 0 devuelve 400' ((Llamar POST '/wallets/me/deposits' @{ monto = 0 } $token).Codigo -eq 400)

    Write-Host 'Ofertas' -ForegroundColor Cyan
    $debajo = Llamar POST "/auctions/$subastaId/bids" @{ monto = $minimo - 1 } $token
    Resultado 'Oferta por debajo del minimo devuelve 422' ($debajo.Codigo -eq 422) "codigo $($debajo.Codigo)"
    $oferta = Llamar POST "/auctions/$subastaId/bids" @{ monto = $minimo } $token
    Resultado 'Oferta igual al montoMinimo devuelve 201' ($oferta.Codigo -eq 201) "codigo $($oferta.Codigo)"
    Resultado 'La puja registrada trae cantidadOfertas del back' ($oferta.Datos.cantidadOfertas -ge 1) "valor $($oferta.Datos.cantidadOfertas)"

    $despues = Llamar GET "/auctions/$subastaId"
    Resultado 'La puja actual quedo en el monto ofertado' ([decimal]$despues.Datos.pujaActual -eq $minimo)
    Resultado 'El lider es el usuario de prueba' ($despues.Datos.seudonimoLider -eq "verif-$sufijo")
    Resultado 'El nuevo montoMinimo subio un incremento' ([decimal]$despues.Datos.montoMinimo -eq $minimo + [decimal]$despues.Datos.incrementoMinimo)
    Resultado 'Ofertar siendo lider devuelve 4xx' ((Llamar POST "/auctions/$subastaId/bids" @{ monto = [decimal]$despues.Datos.montoMinimo } $token).Codigo -ge 400)

    Write-Host 'Billetera' -ForegroundColor Cyan
    $saldos = Llamar GET '/wallets/me' $null $token
    Resultado 'El saldo retenido es igual a la oferta lider' ([decimal]$saldos.Datos.saldoRetenido -eq $minimo)
    Resultado 'Disponible = total - retenido' ([decimal]$saldos.Datos.saldoDisponible -eq [decimal]$saldos.Datos.saldoTotal - [decimal]$saldos.Datos.saldoRetenido)

    $movimientos = (Llamar GET '/wallets/me/entries?pagina=1&tamano=10' $null $token).Datos.items
    $dep = $movimientos | Where-Object { $_.tipo -eq 'Deposito' } | Select-Object -First 1
    $ret = $movimientos | Where-Object { $_.tipo -eq 'Retencion' } | Select-Object -First 1
    Resultado 'El deposito trae efecto = suma' ($dep -and $dep.efecto -eq 'suma')
    Resultado 'La retencion trae efecto = reserva' ($ret -and $ret.efecto -eq 'reserva')

    Write-Host 'Frontend (sin logica propia)' -ForegroundColor Cyan
    $pujaJs = Get-Content -Raw 'frontend/js/puja.js'
    $subastaJs = Get-Content -Raw 'frontend/js/subasta.js'
    Resultado 'puja.js usa detalle.montoMinimo' ($pujaJs -match 'detalle\.montoMinimo')
    Resultado 'Ni puja.js ni subasta.js suman pujaActual + incremento' (-not (($pujaJs + $subastaJs) -match 'pujaActual\s*\+'))
    $uiJs = Get-Content -Raw 'frontend/js/ui.js'
    $salaJs = Get-Content -Raw 'frontend/js/sala.js'
    $contadorJs = Get-Content -Raw 'frontend/js/contador.js'
    $publicarJs = Get-Content -Raw 'frontend/js/publicar.js'
    Resultado 'El front ya no decide vencimiento (sin estaVencida)' (-not ($uiJs -match 'estaVencida'))
    Resultado 'sala.js no suma ofertas (usa el conteo del back)' (-not ($salaJs -match 'cantidadOfertas\+\+'))
    Resultado 'contador.js descuenta contra la hora del servidor' ($contadorJs -match 'ahoraServidor')
    Resultado 'publicar.js lee los limites de /config/auctions' ($publicarJs -match 'config/auctions')
}
finally {
    if ($procesoApi -and -not $procesoApi.HasExited) {
        taskkill /PID $procesoApi.Id /T /F | Out-Null
    }
}

Write-Host ''
if ($script:fallas -eq 0) {
    Write-Host 'TODO OK' -ForegroundColor Green
    exit 0
}
Write-Host "$($script:fallas) verificacion(es) fallaron" -ForegroundColor Red
exit 1
