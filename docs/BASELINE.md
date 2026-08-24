# Lähdesovellusten baseline

Baseline varmistettiin 23.8.2026 ennen moduulien porttausta. Vanhoihin
repositoryihin ei tehty muutoksia.

| Sovellus | Commit | Varmennus | Tulos |
| --- | --- | --- | --- |
| SuperLighter | `71638eb` / `v1.3.0` | `dotnet build .\SuperLighter.sln -c Release` ja `--self-test` | 0 varoitusta, 0 virhettä, self-test läpi |
| Aimoro | `fc734ba` | `dotnet build .\Aimoro.sln -c Release` | 0 varoitusta, 0 virhettä; lähteessä ei ollut testiprojektia |
| SoundDirectionVisualizer | `cfbcf28` | `dotnet test .\SoundDirectionVisualizer.sln --configuration Release` | Core 62/62 ja App 76/76 testiä läpi |

Portattu lähdekoodi sijaitsee uuden repositoryn `src`-hakemistossa. Alkuperäiset
entrypointit säilyvät lähdevertailua varten tiedostoina, mutta ne on poistettu
moduuliprojektien compile-listasta. `ModularGameOverlay.App` on solutionin ainoa
ajettava entrypoint.

SoundDirectionVisualizerin kaikki 138 automaattista testiä siirrettiin uuden
solutionin osaksi ilman poistettuja tai ohitettuja testejä. SuperLighterin
self-test ajetaan lisäksi uuden host-testisuiten kautta STA-threadillä. Aimoron
kriittiset uudet unbound- ja migraatiokäyttäytymiset on suojattu host-testeillä.
