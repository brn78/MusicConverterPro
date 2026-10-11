# Componenti di terze parti

Music Converter Pro © 2026 packjc è distribuito secondo i termini della **GNU General Public License v3 o successiva** (file `LICENSE`).

L'eseguibile incorpora (e al primo avvio scompatta nella cartella `tools` del runtime) i programmi elencati qui sotto. Vengono eseguiti come processi separati e non sono modificati. Per ciascuno sono indicati versione, licenza, dove trovare i sorgenti e il file con il testo completo della licenza (cartella `licenses`).

| Componente | Versione | Licenza | Sorgenti | Testo licenza |
|---|---|---|---|---|
| yt-dlp | 2026.08.19 | Unlicense (pubblico dominio) | https://github.com/yt-dlp/yt-dlp | `licenses/yt-dlp-2026.08.19-LICENSE.txt` |
| FFmpeg (`ffmpeg.exe`) | 9.0.1, build "essentials" di gyan.dev | **GNU GPL v3** (build configurata con `--enable-gpl --enable-version3`) | https://ffmpeg.org · https://github.com/FFmpeg/FFmpeg/tree/n9.0.1 · build: https://www.gyan.dev/ffmpeg/builds/ | `LICENSE` (GPL v3) e `licenses/ffmpeg-9.0.1-LICENSE.md` |
| Deno | 2.9.7 | MIT | https://github.com/denoland/deno | `licenses/deno-v2.9.7-LICENSE.md` |
| Newtonsoft.Json | 13.0.3 | MIT | https://github.com/JamesNK/Newtonsoft.Json | `licenses/Newtonsoft.Json-LICENSE.md` |
| Runtime .NET 8 e Windows Desktop (inclusi nell'eseguibile autonomo) | 8.0 | MIT; alcuni componenti nativi Microsoft sono ridistribuibili con le applicazioni .NET | https://github.com/dotnet/runtime · https://github.com/dotnet/winforms · https://github.com/dotnet/core/blob/main/license-information.md | `licenses/dotnet-runtime-LICENSE.txt` |

## Nota sulla GPL per FFmpeg

La build di FFmpeg inclusa è coperta dalla GPL v3. Chi ridistribuisce questo pacchetto deve:

1. lasciare questo file, il file `LICENSE` e la cartella `licenses` accanto al programma;
2. rendere disponibili i sorgenti corrispondenti di FFmpeg 9.0.1 per almeno tre anni (archivio ufficiale: https://ffmpeg.org/releases/ffmpeg-9.0.1.tar.xz; le librerie aggiuntive della build provengono dai pacchetti MSYS2 documentati da gyan.dev). Il modo più semplice è conservare una copia dell'archivio dei sorgenti insieme a ogni versione pubblicata.

## Marchi

I nomi dei servizi web da cui yt-dlp può scaricare sono marchi dei rispettivi proprietari. Music Converter Pro non è affiliato né approvato da nessuno di essi e non ne usa nomi o loghi nell'interfaccia.

## Icona e grafica

L'icona dell'applicazione e la grafica dell'interfaccia sono opera dell'autore e seguono la stessa licenza del programma.
