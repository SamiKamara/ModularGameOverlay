# Testaus

## Yksi varmennuskomento

```powershell
.\scripts\build-and-publish.ps1
```

Komento suorittaa seuraavat portit järjestyksessä:

1. solution restore;
2. Release-build;
3. kaikki automaattiset testit ilman uutta buildia;
4. käynnissä olevan täsmälleen samaan canonical publish-EXE:hen osoittavan
   instanssin hallittu korvaus;
5. puhdas self-contained single-file publish;
6. työpöytäpikakuvakkeen target-, working directory- ja SHA-256-varmennus;
7. aiemmin käynnissä olleen canonical instanssin uudelleenkäynnistys.

## Automaattinen kattavuus

24.8.2026 viimeisin paikallinen Release-ajo läpäisi 152 testiä:

| Suite | Testejä | Keskeinen kattavuus |
| --- | ---: | --- |
| SoundDirectionVisualizer.Core.Tests | 62 | stereo- ja multichannel-analyysi, kalibrointi, loudness, trail ja visualisointimallit |
| SoundDirectionVisualizer.App.Tests | 76 | audio source/fallback/probe, asetukset, UI, näyttö- ja prosessitunnistus |
| ModularGameOverlay.Tests | 14 | uudet oletukset, migraatio, idempotenssi, osittain virheellinen settings, kanoniset/unbound/duplicate-hotkeyt, kahdeksan kentän keskitetty ikkuna, hotkey-tekstin pystykeskitys, pääikkunan oikean reunan linjaus, yhteiset otsikot/ikonit, tumma tray-renderöinti ja SuperLighter-self-test |

Release-buildin on oltava 0 warnings / 0 errors. Testejä ei saa ohittaa tai
heikentää toteutuksen läpiviemiseksi.

## Tässä buildissa paikallisesti varmennettu

- [x] Kolmen lähderepon baseline-buildit/testit läpäisivät.
- [x] Uusi solution rakentui varoituksitta.
- [x] Kaikki automaattiset testit läpäisivät.
- [x] Ensikäynnistys migroi tämän laitteen kolme asetustiedostoa yhteen tiedostoon.
- [x] SuperLighter jäi pois, Aimoro päälle ja SoundDirectionVisualizer päälle.
- [x] Vain `Ctrl+Alt+B` jäi sidotuksi; muut seitsemän toimintoa ovat unbound.
- [x] Pääikkuna renderöitiin direct-window capturella ja kaikki vaaditut
  kontrollit näkyivät.
- [x] Kolme laajaa asetusikkunaa avautuivat hostista ja sulkeutuivat ilman
  tallennusta.
- [x] Keskitetty hotkey-ikkuna renderöi kahdeksan toimintoa oikeilla oletuksilla.
- [x] Pääikkunan ja keskitetyn hotkey-ikkunan hotkey-tekstit ovat pysty- ja
  vaakasuunnassa keskellä; oikean reunan kenttä ja painikkeet ovat samassa
  linjassa.
- [x] Kaikki viisi asetusikkunaa käyttävät ModularGameOverlay-otsikkoa ja
  sovelluskuvaketta, ja tray-valikon check-alue käyttää tummaa renderöintiä.
- [x] Aimoro- ja SoundDirectionVisualizer-togglet testattiin off/on ja molemmat
  tilat tallentuivat välittömästi yhteiseen settings-tiedostoon.
- [x] Toinen käynnistys jätti prosessimäärän yhteen ja aktivoi olemassa olevan
  instanssin.
- [x] Pääikkunan sulkeminen jätti prosessin trayhin; uusi käynnistys näytti
  piilotetun pääikkunan uudelleen.
- [x] Publish-hakemistossa on vain yksi EXE, jonka hash vastaa pikakuvakkeen
  targetia.

## Käyttäjän testattava checklist

Sulje ensin vanhat erilliset overlay-sovellukset, jotta niiden hotkeyt tai
overlayt eivät kilpaile uuden hostin kanssa. Testaa sitten työpöydän
`ModularGameOverlay.lnk`-pikakuvakkeesta:

- [ ] Pääikkuna ja kaikki neljä asetusikkunaa näyttävät oikeilta 100, 125 ja
  150 % Windows-skaalauksilla.
- [ ] Light Enhancement `Ctrl+Alt+B` toimii globaalisti, pääikkunan tila päivittyy
  ja näyttö palautuu oikein off-tilassa sekä Exitissä.
- [ ] SuperLighterin gamma, kontrasti, saturaatio, brightness-overlay ja
  mahdolliset monitor brightness -kontrollit vastaavat vanhaa sovellusta.
- [ ] Aimoron reticle, oikean hiiren hold-to-show, värit, mitat ja center dot
  vastaavat vanhaa sovellusta.
- [ ] Aimoro seuraa Steam-peliä automaattitilassa ja näyttöjen kierrätys toimii
  vähintään kahdella näytöllä.
- [ ] Sound Direction Visualizer näyttää hiljaisuuden, vasen/oikea-, ambience-,
  loud- ja trail-tilat odotetusti oikealla peliaudiolla.
- [ ] Sound Direction Visualizerin Status-välilehti näyttää oikean endpointin,
  capture methodin, layoutin, fallbackin ja event historyn.
- [ ] 5.1/7.1 process-probe ja stereo-fallback toimivat testilaitteilla, joilla
  kyseiset lähteet ovat saatavilla.
- [ ] Kaikki kahdeksan hotkeytä voi asettaa, tyhjentää ja synkronoida pää-,
  keskitetyn ja moduulikohtaisen ikkunan välillä.
- [ ] Duplikaatti ja toisen sovelluksen varaama hotkey tuottavat ymmärrettävän
  ilmoituksen ilman muiden sidontojen rikkoutumista. Sulje varaava sovellus ja
  klikkaa ModularGameOverlay-pikakuvaketta uudelleen; sidonnan tulee rekisteröityä
  olemassa olevaan host-prosessiin.
- [ ] Overlayt eivät vie pelin keyboard/mouse-fokusta ja säilyttävät
  click-through/no-activation/topmost-käyttäytymisen.
- [ ] Näytön irrotus, kytkentä, session unlock ja power resume eivät kaada hostia.
- [ ] Tray Exit poistaa kaikki overlayt, audio capturen, hotkeyt ja taustaresurssit.

Kirjaa poikkeamasta Windows-versio, DPI, näyttöasettelu, audio endpoint/format,
pelin display mode ja toistovaiheet.
