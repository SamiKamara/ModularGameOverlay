# Moduulien parity-matriisi

Tämä matriisi määrittelee toiminnallisuuden, jonka uusi host säilyttää
lähdesovellusten varmennetusta baselinesta. Pääikkuna näyttää vain yleiset
togglet ja tärkeän Light Enhancement -hotkeyn; kaikki alla olevat säädöt ovat
edelleen moduulien laajoissa asetusikkunoissa.

## SuperLighter

- Enhancement enabled ja live preview
- gamma 0.50–6.00
- kontrasti 50–200 %
- saturaatio 0–300 %
- brightness-overlay 0–60 %
- DDC/CI:tä tukevien näyttöjen fyysinen kirkkaus
- monitorikohtainen asetusten säilytys
- Reset display / neutral-arvot
- Toggle enhancement- ja Open settings -hotkeyt
- gamma-, color matrix- ja overlay-efektien palautus deaktivoinnissa ja Exitissä

Uuden hostin tarkoituksellinen ero: Open settings on oletuksena unbound ja myös
toggle voidaan tyhjentää moduulin omasta ikkunasta. Light Enhancement säilyttää
uuden asennuksen `Ctrl+Alt+B`-oletuksen.

## Aimoro

- reticle päälle/pois
- Steam-pelin näytön automaattinen tunnistus ja manuaalinen näyttö
- näyttöjen kierrätys
- hold-to-show ja valittava hiiren painike
- pää- ja outline-väri
- opacity, scale, arm length, gap ja thickness
- center dot, sen näkyvyys ja koko
- live-tallennus ja reticlen päivitys
- Toggle reticle-, Cycle displays- ja Open settings -hotkeyt

Uuden hostin tarkoituksellinen ero: kaikki kolme hotkeytä ovat oletuksena
unbound. Overlayn ollessa pois embedded-moduuli ei pollaa hiiren painiketta.

## SoundDirectionVisualizer

- oletus- ja valittu audio endpoint
- paras saatavilla oleva 7.1/5.1 process-probe ja stereo-fallback
- valinnainen detected-game process capture
- centered-output automatic game-process fallback
- automaattinen kalibrointi sekä manuaaliset threshold/smoothing/balance-arvot
- loud sound emphasis ja sen threshold
- Steam-pelin automaattinen näyttökohdistus ja manuaalinen näyttö
- overlayn väri, opacity, korkeus, thickness, marker size ja offsetit
- Ambient- ja Loud-markerien erilliset koko-, opacity- ja väriasetukset
- loud outline -asetukset
- ring, ticks, current rays, current markers, listener dot, trail ja labels
- trail duration
- Status-välilehti, capture-event history ja debug channel meter
- Toggle overlay-, Cycle displays- ja Open settings -hotkeyt

SoundDirectionVisualizer.Core-testit säilyttävät stereo front/back -ambiguityn,
multichannel-maskit, kaikkien kanavien analyysin, kalibroinnin, loudnessin,
trailin ja marker-renderöinnin. Uuden hostin tarkoituksellinen ero on kolmen
hotkeyn unbound-oletus ja audio capturen pysäytys moduulin ollessa pois päältä.

## Yhteiset muutokset

- yksi näkyvä tray-kuvake ja yksi prosessi;
- yksi asetustiedosto ja atominen tallennus;
- yksi keskitetty hotkey-rekisteri ja kahdeksan toiminnon editori;
- sama Light Enhancement -sidonta pääikkunassa, keskitetyssä ikkunassa ja
  SuperLighterin laajassa ikkunassa;
- yhteensopiva dark mode -ilme ja uusi nelivärinen SVG/ICO-tuotebrändi;
- moduulien omat EXE-entrypointit eivät kuulu julkaistuun sovellukseen.
