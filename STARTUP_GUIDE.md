# Ohjeet koko RagApi-järjestelmän ajamiseen (.NET 9)

## 🚀 NOPEIN TAPA - Skriptit

### **Windows PowerShell**
```powershell
.\start-both.ps1
```

### **Windows Command Prompt**
```cmd
start-both.bat
```

Molemmat skriptit:
- ✅ Käynnistävät RagApi:n (Backend)
- ✅ Käynnistävät RagMaui:n (Frontend) 
- ✅ Avaavat erilliset terminaalit molemmille
- ✅ Näyttävät oikeat URL:t

---

## 🎯 Visual Studio - Multiple Startup Projects

### 1. Aseta Multiple Startup Projects

1. **Solution Explorer** → Klikkaa hiiren oikealla **Solution 'RagApi'**
2. Valitse **Set Startup Projects...**
3. Valitse **Multiple startup projects**
4. Aseta seuraavat projektit **Start**-tilaan:
   - ✅ **RagApi** → Action: **Start**
   - ✅ **RagMaui** → Action: **Start**
   - ❌ **RagApi.Tests** → Action: **None**
   - ❌ **RagMaui.Tests** → Action: **None**

### 2. Korjaa Visual Studio Platform Asetukset

**TÄRKEÄÄ:** Varmista että RagMaui käyttää oikeaa platformia:

1. **Debug dropdown** (Start-napin vieressä)
2. Valitse **"Windows (Unpackaged)"** tai **"Windows Machine"**
3. **EI** Android Device/Emulator

### 3. Käynnistä sovellukset

- Paina **F5** tai **Start Debugging**
- Tai **Ctrl+F5** ilman debuggausta

---

## 🖥️ Manuaalinen käynnistys - Terminaalissa

### 1. Backend (RagApi)

```bash
# Terminaali 1 - Backend
cd RagApi
dotnet run
```

### 2. Frontend (RagMaui) 

```bash
# Terminaali 2 - Frontend
cd RagMaui

# Windows (suositeltu)
dotnet run -f net9.0-windows10.0.19041.0

# Android (vaatii emulaattorin/laitteen)
dotnet run -f net9.0-android

# iOS (vain macOS)
dotnet run -f net9.0-ios

# macCatalyst (vain macOS)
dotnet run -f net9.0-maccatalyst
```

---

## 🔧 Konfiguraatio

### API Portti
- Backend pyörii: **https://localhost:7296**
- RagMaui käyttää tätä osoitetta: `AuthConfig.cs`

### CORS Asetukset
API hyväksyy pyynnöt MAUI-sovellukselta automaattisesti.

---

## 🔍 Testaus

### 1. API Toimivuus
- Avaa: `https://localhost:7296/swagger`
- Testaa: `GET /version` endpoint

### 2. MAUI Yhteys
- Avaa MAUI-sovellus
- Testaa kirjautuminen
- Testaa API-kutsut (hakuehdot, dokumentit)

---

## 🛠️ Ongelmanratkaisu

### API ei käynnisty
1. Tarkista SQL Server yhteys
2. Tarkista Azure-palveluiden avaimet
3. Aja: `dotnet ef database update`

### MAUI deployment-virheet

#### DEP1700 (Android recipe file)
- **Syy:** Visual Studio yrittää deploytata Androidille
- **Ratkaisu:** Vaihda Debug dropdown → **"Windows (Unpackaged)"**

#### DEP1560 (AppxManifest.xml)
- **Syy:** Windows packaging -ongelma
- **Ratkaisu:** Käytä skriptejä tai command line -käynnistystä

### HTTPS sertifikaatti-ongelmat
```bash
dotnet dev-certs https --trust
```

### .NET 9 Workload -ongelmat
Jos MAUI workloadit puuttuvat .NET 9:lle:
```bash
dotnet workload install maui
dotnet workload install maui-android
dotnet workload install maui-ios
dotnet workload install maui-maccatalyst
dotnet workload install maui-windows
```

---

## 📱 Platform Debugging

### Windows ✅
- **Toimii:** Command line, skriptit
- **Visual Studio:** Voi vaatia platform-asetusten korjaamista

### Android
- Yhdistä Android-laite tai käynnistä emulator
- Deploy & Run

### iOS
- Tarvitset macOS:n ja Xcode:n
- Apple Developer Account (deployment)

---

## 🆕 .NET 9 Päivitykset

Projekti on päivitetty .NET 9:ään:
- ✅ **Parempi suorituskyky** - .NET 9 on nopeampi
- ✅ **Uudempi MAUI** - Tuki uusimmille platform-versioille
- ✅ **Turvallisuuspäivitykset** - .NET 8 MAUI workloadit eivät enää saa päivityksiä
- ✅ **Yhteensopivuus** - Kaikki paketit päivitetty .NET 9:lle

### Tarvittavat Workloadit (.NET 9)
```bash
dotnet --list-workloads
# Asenna puuttuvat:
dotnet workload install maui
```

---

## ⚡ Suositellut käynnistystavat järjestyksessä:

1. **🥇 Skriptit** (start-both.ps1 tai start-both.bat) - Toimii aina
2. **🥈 Command line** - Luotettava ja nopea  
3. **🥉 Visual Studio** - Vaatii platform-asetusten korjaamista

---

## 🎉 Onnistunut käynnistys

Kun molemmat palvelut pyörivät:
- ✅ API: `https://localhost:7296/swagger`
- ✅ MAUI: Käynnistyy Windows-sovelluksena
- ✅ Autentikointi: Azure AD kautta
- ✅ Tietokannan yhteys: SQL Server
- ✅ Azure-palvelut: OpenAI, Cognitive Search, Blob Storage
- ✅ .NET 9: Paras suorituskyky ja turvallisuus