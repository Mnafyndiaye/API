param(
    [string]$BaseUrl = "http://localhost:5158",
    [switch]$KeepApiRunning
)

$ErrorActionPreference = "Stop"

function Get-StatusCodeFromException {
    param([System.Exception]$Exception)

    if ($Exception.PSObject.Properties.Name -contains "Response" -and $Exception.Response) {
        if ($Exception.Response.PSObject.Properties.Name -contains "StatusCode") {
            return [int]$Exception.Response.StatusCode
        }
    }

    return -1
}

function Invoke-RequestWithStatus {
    param(
        [Parameter(Mandatory = $true)][string]$Method,
        [Parameter(Mandatory = $true)][string]$Uri,
        [hashtable]$Headers,
        [string]$Body,
        [string]$ContentType = "application/json"
    )

    try {
        $params = @{
            Method      = $Method
            Uri         = $Uri
            ErrorAction = "Stop"
        }

        if ($Headers) {
            $params.Headers = $Headers
        }

        if ($Body) {
            $params.Body = $Body
            $params.ContentType = $ContentType
        }

        $response = Invoke-WebRequest @params

        return [PSCustomObject]@{
            StatusCode = [int]$response.StatusCode
            Response   = $response
            Error      = $null
        }
    }
    catch {
        return [PSCustomObject]@{
            StatusCode = Get-StatusCodeFromException -Exception $_.Exception
            Response   = $null
            Error      = $_
        }
    }
}

Push-Location $PSScriptRoot
$apiProcess = $null
$startedByScript = $false

try {
    Write-Host "[1/6] Demarrage de MongoDB (Docker Compose)..."
    docker compose up -d mongo | Out-Host

    $apiListening = Get-NetTCPConnection -LocalPort 5158 -State Listen -ErrorAction SilentlyContinue
    if (-not $apiListening) {
        Write-Host "[2/6] Demarrage de l'API..."
        $apiProcess = Start-Process dotnet -ArgumentList "run --launch-profile http" -WorkingDirectory (Get-Location).Path -PassThru
        $startedByScript = $true

        $ready = $false
        for ($i = 0; $i -lt 40; $i++) {
            Start-Sleep -Milliseconds 750
            $health = Invoke-RequestWithStatus -Method "GET" -Uri "$BaseUrl/weatherforecast"
            if ($health.StatusCode -eq 200) {
                $ready = $true
                break
            }
        }

        if (-not $ready) {
            throw "API non joignable sur $BaseUrl apres attente."
        }
    }
    else {
        Write-Host "[2/6] API deja en cours d'execution sur le port 5158."
    }

    Write-Host "[3/6] Login user/admin..."
    $userLoginBody = (@{ username = "user"; password = "User123!" } | ConvertTo-Json)
    $adminLoginBody = (@{ username = "admin"; password = "Admin123!" } | ConvertTo-Json)

    $userLogin = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/auth/login" -ContentType "application/json" -Body $userLoginBody
    $adminLogin = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/auth/login" -ContentType "application/json" -Body $adminLoginBody

    $userToken = $userLogin.token
    $adminToken = $adminLogin.token

    if (-not $userToken -or -not $adminToken) {
        throw "Token JWT manquant apres login."
    }

    Write-Host "[4/6] Tests de securite..."
    $anonGet = Invoke-RequestWithStatus -Method "GET" -Uri "$BaseUrl/api/todoitems"

    $userPostBody = '{"name":"forbidden-by-user","isComplete":false}'
    $userPost = Invoke-RequestWithStatus -Method "POST" -Uri "$BaseUrl/api/todoitems" -Headers @{ Authorization = "Bearer $userToken" } -Body $userPostBody

    $adminPostBody = '{"name":"todo-admin-demo","isComplete":false}'
    $adminPost = Invoke-RequestWithStatus -Method "POST" -Uri "$BaseUrl/api/todoitems" -Headers @{ Authorization = "Bearer $adminToken" } -Body $adminPostBody

    $todoId = $null
    if ($adminPost.Response -and $adminPost.Response.Headers["Location"]) {
        $todoId = ($adminPost.Response.Headers["Location"].TrimEnd("/").Split("/") | Select-Object -Last 1)
    }

    $userGet = Invoke-RequestWithStatus -Method "GET" -Uri "$BaseUrl/api/todoitems" -Headers @{ Authorization = "Bearer $userToken" }

    Write-Host "[5/6] Verification finale..."
    $allGood = $true
    if ($anonGet.StatusCode -ne 401) { $allGood = $false }
    if ($userPost.StatusCode -ne 403) { $allGood = $false }
    if ($adminPost.StatusCode -ne 201) { $allGood = $false }
    if ($userGet.StatusCode -ne 200) { $allGood = $false }

    Write-Host ""
    Write-Host "===== RESULTATS DEMO TP ====="
    Write-Host "Anonyme GET /api/todoitems      -> $($anonGet.StatusCode) (attendu 401)"
    Write-Host "User POST /api/todoitems        -> $($userPost.StatusCode) (attendu 403)"
    Write-Host "Admin POST /api/todoitems       -> $($adminPost.StatusCode) (attendu 201)"
    Write-Host "User GET /api/todoitems         -> $($userGet.StatusCode) (attendu 200)"
    Write-Host "Todo cree (id MongoDB)          -> $todoId"
    Write-Host "Role user                       -> $($userLogin.role)"
    Write-Host "Role admin                      -> $($adminLogin.role)"

    if ($allGood) {
        Write-Host "STATUT GLOBAL: OK"
    }
    else {
        Write-Host "STATUT GLOBAL: ECHEC (verifier la config)"
        exit 1
    }

    Write-Host "[6/6] Termine."
}
finally {
    if ($startedByScript -and -not $KeepApiRunning -and $apiProcess -and -not $apiProcess.HasExited) {
        Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
    }

    Pop-Location
}
