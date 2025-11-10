# RagApi + RagMaui käynnistysskripti
# Tämä skripti käynnistää molemmat sovellukset samanaikaisesti

Write-Host "🚀 Käynnistetään RagApi-järjestelmä..." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Yellow

# Varmista että olemme oikeassa hakemistossa
$currentPath = Get-Location
Write-Host "📂 Nykyinen sijainti: $currentPath" -ForegroundColor Cyan

# Tarkista että RagApi ja RagMaui kansiot löytyvät
if (-not (Test-Path "RagApi")) {
    Write-Host "❌ RagApi kansiota ei löydy!" -ForegroundColor Red
 exit 1
}

if (-not (Test-Path "RagMaui")) {
    Write-Host "❌ RagMaui kansiota ei löydy!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Projektikansiot löytyvät" -ForegroundColor Green

# Funktio sovellusten käynnistämiseksi
function Start-Application {
    param(
      [string]$Name,
        [string]$Path,
        [string]$Framework = "",
        [string]$Color = "White"
    )
    
    Write-Host "🔄 Käynnistetään $Name..." -ForegroundColor $Color
    
  $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "dotnet"
    $psi.WorkingDirectory = $Path
    
    if ($Framework) {
    $psi.Arguments = "run -f $Framework"
    } else {
        $psi.Arguments = "run"
    }
    
    $psi.UseShellExecute = $true
    $psi.CreateNoWindow = $false
    
    try {
        $process = [System.Diagnostics.Process]::Start($psi)
      Write-Host "✅ $Name käynnistetty (PID: $($process.Id))" -ForegroundColor Green
     return $process
  }
    catch {
        Write-Host "❌ Virhe käynnistettäessä $Name`: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# Käynnistä sovellukset
Write-Host "`n🎯 Käynnistetään sovellukset..." -ForegroundColor Yellow

# 1. Käynnistä RagApi (Backend)
$apiPath = Join-Path $currentPath "RagApi"
$apiProcess = Start-Application -Name "RagApi (Backend)" -Path $apiPath -Color "Blue"

# Odota hetki että API käynnistyy
Start-Sleep -Seconds 3

# 2. Käynnistä RagMaui (Frontend)
$mauiPath = Join-Path $currentPath "RagMaui"
$mauiProcess = Start-Application -Name "RagMaui (Frontend)" -Path $mauiPath -Framework "net9.0-windows10.0.19041.0" -Color "Magenta"

# Tulosta tiedot
Write-Host "`n🎉 Sovellukset käynnistetty!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "🌐 RagApi Backend: https://localhost:7296" -ForegroundColor Blue
Write-Host "📱 RagMaui Frontend: Windows Application" -ForegroundColor Magenta
Write-Host "📖 Swagger UI: https://localhost:7296/swagger" -ForegroundColor Cyan
Write-Host "`n⏹️  Paina Ctrl+C lopettaaksesi tämän skriptin" -ForegroundColor Yellow
Write-Host "   (Sovellukset jatkavat toimintaa omissa ikkunoissaan)" -ForegroundColor Gray

# Odota että käyttäjä keskeyttää skriptin
try {
    while ($true) {
      Start-Sleep -Seconds 1
    }
}
catch {
    Write-Host "`n👋 Skripti lopetettu. Sovellukset jatkavat toimintaa." -ForegroundColor Yellow
}

Write-Host "`n📝 Muista lopettaa sovellukset manuaalisesti kun lopetat kehityksen!" -ForegroundColor Gray