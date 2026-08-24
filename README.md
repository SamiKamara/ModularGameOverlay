# ModularGameOverlay

<p align="center">
  <img src="src/ModularGameOverlay.App/Assets/ModularGameOverlay.png" alt="ModularGameOverlay icon" width="112" height="112">
</p>

ModularGameOverlay yhdistää SuperLighterin, Aimoron ja
SoundDirectionVisualizerin yhdeksi Windows-overlay-apusovellukseksi. Yksi
host-prosessi tarjoaa yhden tray-kuvakkeen, yksinkertaisen pääikkunan, kolme
moduulikohtaista laajaa asetusikkunaa ja keskitetyt globaalit pikanäppäimet.

## Paikallinen testibuild

Uusin self-contained Windows x64 -build löytyy tämän repositoryn paikallisesta
polusta:

```text
artifacts\publish\win-x64\ModularGameOverlay.exe
```

Tällä kehityslaitteella työpöydän `ModularGameOverlay.lnk` osoittaa aina samaan
uusimpaan varmennettuun buildiin. Sovellus jää pääikkunan sulkemisen jälkeen
trayhin; lopeta se tray-valikon **Exit**-toiminnolla.

## Käyttö

Pääikkunassa voi:

- kytkeä SuperLighterin enhancementin, Aimoron reticlen ja äänen suunnan
  overlayn päälle tai pois;
- säätää tärkeän Light Enhancement -toiminnon hotkeyn suoraan;
- avata jokaisen moduulin alkuperäistä vastaavat laajat asetukset;
- avata kaikki kahdeksan nykyistä hotkey-toimintoa keskitettyyn ikkunaan.

Ensimmäisellä käynnistyksellä sovellus migroi yksityiskohtaiset asetukset
vanhoista `%AppData%`-tiedostoista uuteen tiedostoon:

```text
%AppData%\ModularGameOverlay\settings.json
```

Vanhat asetustiedostot säilyvät muuttumattomina. Vain Light Enhancementin
`Ctrl+Alt+B` on oletuksena sidottu; muut seitsemän hotkeytä ovat unbound.

## Build ja testit

Kaikki paikallisen buildin, testien, publishin ja työpöytäpikakuvakkeen vaiheet
ajetaan yhdellä komennolla:

```powershell
.\scripts\build-and-publish.ps1
```

Putki ajaa Release-restoren, varoituksettoman buildin, kaikki kolme testisuitea,
self-contained single-file publishin, publish-hakemiston puhdistuksen sekä
pikakuvakkeen target- ja SHA-256-varmennuksen.

Kuvakeassetit voi generoida master-SVG:n geometriasta Pillowlla:

```powershell
python .\scripts\generate-icon-assets.py
```

## Dokumentaatio

- [Tavoite ja suunnitelma](docs/TAVOITE-JA-SUUNNITELMA.md)
- [Arkkitehtuuri](docs/ARCHITECTURE.md)
- [Lähdesovellusten parity](docs/PARITY.md)
- [Testaus ja käyttäjän smoke-testi](docs/TESTING.md)
- [Lähtötason varmennus](docs/BASELINE.md)

Kaikkien projektiin osallistuvien ihmisten ja agenttien on luettava
[AGENTS.md](AGENTS.md) ennen muutosten tekemistä. Dokumentaatio ja testit ovat
ylläpidettävä osa toteutusta.
