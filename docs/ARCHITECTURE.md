# Arkkitehtuuri

## Prosessi ja elinkaari

`ModularGameOverlay.App` on WinForms- ja .NET 9 Windows -host. Se omistaa:

- single-instance mutexin ja olemassa olevan instanssin avauseventin;
- näkyvän tray-kuvakkeen ja tray-valikon;
- yksinkertaisen pääikkunan ja keskitetyn hotkey-ikkunan;
- yhden `%AppData%\ModularGameOverlay\settings.json`-tiedoston;
- yhden Win32 `RegisterHotKey` -rekisterin;
- kolmen moduulin käynnistyksen, tallennuscallbackit ja deterministisen
  sammutusjärjestyksen.

Moduulit käyttävät portattuja application context -luokkia embedded-tilassa.
Niiden omat tray-kuvakkeet eivät ole näkyviä, omia hotkey-rekisteröintejä ei
tehdä ja asetusten tallennus delegoidaan hostille. Moduulien nykyiset palvelut,
overlay-formit ja laajat SettingsForm-ikkunat säilyvät omissa assemblyissään.

```text
ModularGameOverlay.App
  ├─ SuperLighter.Module
  ├─ Aimoro.Module
  └─ SoundDirectionVisualizer.Module
       └─ SoundDirectionVisualizer.Core
```

SoundDirectionVisualizer.Core säilyy `net9.0`-projektina ilman WinForms- tai
Windows-riippuvuutta. Sen suunta-, audio-, kalibrointi- ja visualisointimallit
ovat deterministisesti testattavia.

## Asetukset ja migraatio

`ModularGameOverlaySettings` sisältää schema-version, kolme moduulien nykyistä
`AppSettings`-mallia ja kanonisen `HotkeyConfiguration`-osion. Moduulien
hotkey-ominaisuudet peilataan kanonisesta osiosta, jotta vanhat SettingsFormit
ja keskitetty ikkuna muokkaavat samaa lopputilaa.

Kun uutta tiedostoa ei ole, `SettingsStore` yrittää deserialisoida jokaisen
vanhan asetustiedoston itsenäisesti. Yhden puuttuva tai virheellinen osio palaa
vain kyseisen moduulin oletuksiin. Migraatio ei kirjoita vanhoihin tiedostoihin,
ja uuden tiedoston olemassaolo estää uuden migraation.

Tallennus tehdään ensin `.tmp`-tiedostoon ja siirretään sitten atomisesti
lopulliseen polkuun. IO- tai käyttöoikeusvirhe ei kaada overlay-hostia.

## Hotkeyt

`GlobalHotkeyManager` omistaa yhden hidden message windown. Tyhjiä sidontoja ei
rekisteröidä. Ennen tallennusta tarkistetaan näppäimen validius ja kaikkien
kahdeksan toiminnon keskinäiset duplikaatit. Windowsin rekisteröintivirhe
ilmoitetaan tray-balloonilla, mutta muut onnistuneet hotkeyt jäävät käyttöön.

Laajan moduuli-ikkunan avaaminen keskeyttää globaalit hotkeyt tekstikenttien
syötön ajaksi. Kun ikkuna sulkeutuu, moduulin asetukset sovitetaan kanoniseen
konfiguraatioon, mahdollinen cross-module-duplikaatti hylätään ja hotkeyt
rekisteröidään uudelleen.

## Moduulien deaktivointi

- SuperLighter piilottaa brightness-overlayn ja palauttaa gamma- sekä
  värimatriisiefektit, kun enhancement poistetaan käytöstä tai host sammuu.
- Aimoro piilottaa reticlen ja embedded-tilassa pysäyttää 25 ms input-pollauksen
  overlayn ollessa pois päältä.
- SoundDirectionVisualizer piilottaa overlayn, pysäyttää audio capturen sekä
  render- ja target-refresh-timerit. Ne käynnistyvät uudelleen moduulin mukana.

## Julkaisu

`scripts\build-and-publish.ps1` on tämän laitteen kanoninen build-polku. Se
varmistaa Release-testit ennen käynnissä olevan varmennetun publish-instanssin
pysäyttämistä, puhdistaa vain tarkistetun repositoryn
`artifacts\publish\win-x64`-kohteen, julkaisee yhden self-contained EXE:n,
päivittää työpöytäpikakuvakkeen ja käynnistää sovelluksen uudelleen vain, jos
juuri sama publish-EXE oli käynnissä ennen päivitystä.
