# Music Converter Pro (v2.6 portatile)

Applicazione desktop Windows, gratuita e open source (GPL-3.0), per:

1. **Scaricare ed estrarre l'audio da link web** dei siti supportati da yt-dlp, in MP3 fino a 320 kbps, oppure M4A, FLAC, WAV e OPUS.
2. **Ottimizzare una pendrive USB o una cartella per l'autoradio**: analisi, doppioni, conversione in MP3 320 kbps a 44,1 kHz stereo con tag ID3v2.3, normalizzazione del volume (EBU R128), nomi file puliti.

Pensata per uso personale e senza scopo di lucro, con contenuti di cui si hanno i diritti: leggi la sezione [Uso consentito e scopi legittimi](#uso-consentito-e-scopi-legittimi).

> **English summary.** Free, open-source (GPL-3.0) Windows desktop app, Italian UI. It extracts audio from web links through yt-dlp and FFmpeg, and prepares USB sticks for car stereos: MP3 320 kbps, 44.1 kHz stereo, ID3v2.3 tags, EBU R128 loudness normalisation, duplicate detection and clean file names. Intended for personal, non-commercial use with content you own or are allowed to download; see the "Uso consentito" section. Single-file portable executable: on first run it unpacks the bundled tools to `%LOCALAPPDATA%\MusicConverterPro\runtime`.

---

## Novità della versione 2.6

### 2.6.1
- **Niente più `config.json` accanto all'eseguibile**: le impostazioni stanno in `%LOCALAPPDATA%\MusicConverterPro\config.json`. Un file lasciato accanto all'exe dalla versione 2.6.0 viene spostato lì al primo avvio. Uso portatile: se accanto all'exe esiste la cartella `tools`, il file resta accanto all'exe.
- **Auto-Paste**: suono e notifica compaiono nel momento in cui il link viene preso dagli appunti, non più a lettura finita; la barra di stato mostra quale link si sta leggendo.
- **File già ottimizzati riconosciuti al volo**: l'ottimizzatore scrive nei tag del file un marcatore (`MusicConverterPro`, frame ID3 TXXX, invisibile per autoradio e lettori) con ciò che ha applicato; alla successiva analisi i file risultano subito "Già ottimizzato" e vengono saltati senza riaprirli con FFmpeg né ricodificarli, anche con la normalizzazione attiva. Anche i file scaricati ricevono il marcatore (standard autoradio, senza normalizzazione). Nuovo pulsante **Aggiorna** per rileggere la cartella dopo un'ottimizzazione.
- Pulsanti alti quanto le caselle di testo in tutte le schede.
- Corretto "File non più presente" dopo la conversione di un file non MP3 con "Sostituisci gli originali".
- Doppioni per dimensione: due file con la stessa dimensione esatta sono segnalati come copia solo se anche il contenuto coincide (brani diversi in MP3 320 kbps con la stessa durata non vengono più scambiati per doppioni).

### Interfaccia rinnovata
- Barra laterale con le due sezioni (**Scaricatore**, **Ottimizza USB**), stato del motore (yt-dlp, FFmpeg, Deno) e tema.
- Schede con angoli arrotondati, un solo colore d'accento, interruttori al posto delle caselle di spunta.
- Tabelle con intestazioni a tema, barre di avanzamento per riga e stati colorati (in coda, scaricamento, completato, errore, doppione, da convertire).
- Barra del titolo scura quando il tema è scuro; nuova icona dell'applicazione (`MusicConverterPro.ico`).
- Finestra ridimensionabile senza sovrapposizioni (minimo 1120×720).

### Scaricatore
- **Playlist**: incollando il link di una playlist (o di un video che fa parte di una playlist) l'app la legge e mostra nome, brani e durata; puoi aggiungere tutta la playlist o solo il video. Ogni brano diventa una riga della coda con posizione e durata. Opzioni ricordate: cartella con il nome della playlist e numerazione dei file (`01 - Titolo.mp3`) per mantenere l'ordine in autoradio. Con Auto-Paste una playlist pura entra tutta, un video dentro una playlist entra da solo.
- **Nomi file puliti** (interruttore nella scheda del link, attivo di default): il nome del file scaricato perde "(Official Video)", "[Lyrics]", "(360p)", "256k", "(prod. by ...)", "(1)", hashtag, emoji e caratteri non ammessi, mentre "Remix", "Live", "feat." e le versioni restano. I tag titolo e artista vengono scritti coerenti con il nome ("Artista - Titolo").
- **Auto-Paste** reagisce solo ai link dei siti multimediali conosciuti: un link qualsiasi copiato dal browser non finisce più in coda e non svuota gli appunti.
- Invio nel campo link = "Aggiungi in coda"; tasto Canc nella tabella = rimuovi.
- La qualità (bitrate) scelta viene ora ricordata al riavvio.
- Errori di yt-dlp riassunti in una riga leggibile; doppio clic sulla riga in errore per i dettagli completi.
- Con formato WAV la copertina non viene più richiesta (yt-dlp falliva l'intero download).
- I file `.part` di un download annullato vengono eliminati.

### Ottimizza USB
- Le pendrive collegate compaiono in un elenco a discesa; l'analisi parte da sola quando ne scegli una.
- L'analisi gira in background: la finestra non si blocca più su chiavette piene.
- Le opzioni **44,1 kHz · stereo · tag ID3v2.3** e **Converti in MP3** ora funzionano davvero (prima erano ignorate).
- Nuova opzione **Salta i file già conformi**: i file che portano il marcatore dell'app (vedi 2.6.1) e gli MP3 già a 44,1 kHz stereo con tag ID3v2.3 non vengono ricodificati (nessuna perdita di qualità a ogni passaggio).
- **Rinomina con nomi puliti**: i file sulla pendrive vengono rinominati con la stessa pulizia del downloader ("Andi Shkoza - Alma  Official Lyrics Video  256k.mp3" diventa "Andi Shkoza - Alma.mp3"), evitando collisioni; i file ricodificati ricevono anche i tag titolo/artista. Il percorso si può anche digitare a mano (Invio per analizzare).
- I file temporanei di una conversione interrotta vengono eliminati all'analisi successiva.
- Con **Sostituisci gli originali** disattivato i file ottimizzati vanno nella sottocartella `Ottimizzati` con il nome originale (prima restavano file con nomi casuali).
- Corretto il bug con apostrofi e virgolette nei nomi dei file (es. *L'amore*), che faceva fallire o rinominare la conversione.
- Riepilogo di conferma prima di qualsiasi operazione distruttiva (doppioni da eliminare, originali sostituiti).
- La tabella mostra cartella, formato e dimensione; gli aggiornamenti di stato restano corretti anche dopo l'ordinamento delle colonne.
- Copertine e metadati vengono conservati durante la ricodifica.

### Altro
- L'icona nell'area di notifica sparisce alla chiusura.
- La sincronizzazione con il tema di Windows continua a funzionare anche dopo un'uscita annullata.

---

## Come avviare l'applicazione

Doppio clic su **`publish_single\MusicConverterPro.exe`**: un unico file con tutto dentro. Al primo avvio di ogni versione scompatta yt-dlp, FFmpeg, Deno e i documenti di licenza in `%LOCALAPPDATA%\MusicConverterPro\runtime\v<versione>-<impronta>` mostrando l'avanzamento; agli avvii successivi riparte subito se la versione coincide, altrimenti scompatta di nuovo ed elimina le versioni precedenti.

Se accanto all'exe esiste una cartella `tools` (con `yt-dlp.exe`, `ffmpeg.exe`, `deno.exe`), viene usata quella al posto dei componenti incorporati: utile per aggiornare yt-dlp senza ricompilare.

Le impostazioni sono salvate in `%LOCALAPPDATA%\MusicConverterPro\config.json`, così accanto all'eseguibile non compare nulla. Per un uso portatile (per esempio su una chiavetta) crea la cartella `tools` accanto all'exe: in quel caso `config.json` resta accanto all'eseguibile.

### Impostazioni avanzate (solo in `config.json`)

- `AutoPasteDomains`: elenco di domini (es. `["esempio.org"]`) a cui limitare l'Auto-Paste. Vuoto o assente = qualsiasi link http/https copiato.
- `YtDlpExtraArgs`: argomenti aggiuntivi passati a ogni chiamata di yt-dlp, nella sintassi della sua riga di comando (opzioni per gli estrattori, cookie del browser, proxy). Vuoto = nessuno.

## Uso consentito e scopi legittimi

Music Converter Pro è uno strumento gratuito, senza scopo di lucro, pensato per uso personale. Esempi di uso legittimo:

- scaricare e archiviare contenuti di cui sei l'autore o il titolare dei diritti (i tuoi video, le tue registrazioni, i tuoi podcast, le tue lezioni);
- contenuti pubblicati con licenze che consentono il download e la copia (Creative Commons, pubblico dominio, archivi aperti, materiale distribuito con autorizzazione esplicita);
- copie private nei limiti consentiti dalla legge del tuo paese e dai termini del servizio da cui provengono;
- riorganizzare, convertire, normalizzare e rinominare file audio che già possiedi (CD estratti, acquisti digitali, registrazioni) per ascoltarli in auto.

Non è consentito usare il programma per violare i termini di servizio dei siti, aggirare protezioni tecnologiche, ridistribuire o vendere contenuti protetti dal diritto d'autore, né per qualsiasi uso commerciale dei contenuti scaricati. Prima di scaricare qualcosa sei tu a dover verificare di averne il diritto: l'autore non fornisce supporto per usi illeciti e non è responsabile dell'uso che viene fatto del programma.

Il programma non raccoglie dati e non contatta server dell'autore: comunica solo con i siti che indichi tu. Windows SmartScreen può mostrare un avviso al primo avvio perché l'eseguibile non è firmato digitalmente.

## Licenza e note legali

- Music Converter Pro © 2026 packjc è **software libero** distribuito secondo la **GNU General Public License v3 o successiva**: vedi `LICENSE`. È fornito **senza alcuna garanzia**.
- Il pacchetto include yt-dlp, FFmpeg (GPL v3, build gyan.dev), Deno, Newtonsoft.Json e il runtime .NET: versioni, licenze e sorgenti sono elencati in `THIRD-PARTY-NOTICES.md` e nella cartella `licenses`. Chi ridistribuisce il pacchetto deve mantenere questi file e rendere disponibili i sorgenti di FFmpeg (vedi la nota nel file degli avvisi).
- **Uso responsabile**: usa il programma solo con contenuti di cui possiedi i diritti o che sono espressamente concessi in licenza per il download, per uso personale e nel rispetto dei termini di servizio dei siti di provenienza. L'autore non è responsabile dell'uso che viene fatto del programma.
- I nomi dei servizi web sono marchi dei rispettivi proprietari; il programma non è affiliato né approvato da nessuno di essi.

## Compilare dal sorgente

Serve il .NET SDK 8 o successivo. Gli eseguibili degli strumenti non sono nel repository (sono grandi e hanno licenze proprie): prima di compilare crea la cartella `tools` accanto al file di progetto con questi tre file, scaricati dalle fonti ufficiali.

| File | Dove scaricarlo | Versione usata |
|---|---|---|
| `yt-dlp.exe` | https://github.com/yt-dlp/yt-dlp/releases | 2026.08.19 |
| `ffmpeg.exe` | https://www.gyan.dev/ffmpeg/builds/ (build "essentials", licenza GPL v3) | 9.0.1 |
| `deno.exe` | https://github.com/denoland/deno/releases (archivio `deno-x86_64-pc-windows-msvc.zip`) | 2.9.7 |

Se aggiorni una versione, aggiorna anche `THIRD-PARTY-NOTICES.md` e il relativo file in `licenses`.

```bash
dotnet build MusicConverterPro.csproj -c Release
```

Singolo eseguibile con gli strumenti incorporati (`SingleExe=true`):

```bash
dotnet publish MusicConverterPro.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:SingleExe=true -o publish_single
```

In alternativa, versione a cartella (exe autonomo + cartella `tools` copiata accanto, senza componenti incorporati):

```bash
dotnet publish MusicConverterPro.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o publish_portable
```
