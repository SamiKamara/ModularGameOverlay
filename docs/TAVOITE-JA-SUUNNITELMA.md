# ModularGameOverlay – tavoite ja toteutussuunnitelma

## 1. Dokumentin asema

Tämä dokumentti on ModularGameOverlay-projektin ensisijainen tuote- ja
toteutussopimus. Se ohjaa arkkitehtuuria, ominaisuuksia, testejä ja projektin
vaiheistusta, kunnes hyväksytty muutos päivittää sitä.

Kaikkien projektia työstävien ihmisten ja agenttien on:

- luettava tämä dokumentti ja tehtävän kannalta olennainen muu dokumentaatio
  ennen käyttäytymiseen vaikuttavia muutoksia;
- kunnioitettava dokumentoituja vaatimuksia ja olemassa olevia testejä;
- päivitettävä dokumentaatio ja testit samassa muutoksessa, jos hyväksytty
  toiminnallisuus, käyttöliittymä, rajoite tai rajapinta muuttuu;
- lisättävä regressiotesti aina, kun korjataan toistettavissa oleva virhe;
- oltava poistamatta, ohittamatta tai heikentämättä testiä vain siksi, että
  toteutus saataisiin läpi. Jos vaatimus todella muuttuu, muutos ja sen syy
  kirjataan ensin dokumentaatioon ja testit päivitetään vastaamaan hyväksyttyä
  uutta käyttäytymistä;
- jätettävä työ luovutuskuntoon: soveltuvat testit ajettuina, dokumentaatio
  ajan tasalla ja tunnetut puutteet avoimesti kirjattuina.

Jos toteutus, testi ja dokumentaatio ovat ristiriidassa eikä käyttäjän tarkoitus
ole yksiselitteinen, ristiriitaa ei ratkaista hiljaisesti. Se selvitetään tai
kirjataan avoimeksi päätökseksi ennen käyttäytymisen muuttamista.

## 2. Tuotetavoite

Projektin tavoitteena on yhdistää kolme aikaisempaa itsenäistä Windows-
sovellusta yhdeksi sovellukseksi nimeltä **ModularGameOverlay**:

1. **SuperLighter** – näytön light enhancement, gamma, kontrasti, saturaatio,
   brightness-overlay ja näyttöjen fyysisen kirkkauden ohjaus;
2. **Aimoro** – pelin tähtäin-overlay ja sen näyttökohdistus;
3. **SoundDirectionVisualizer** – äänen suunnan analysointi ja suunta-overlay.

Lopputulos on yksi itsenäisesti julkaistava sovellus, yksi prosessi, yksi
notification area -kuvake, yksi asetustiedosto ja yksi keskitetty
pikanäppäinrekisteri. Sovellus ei tuotantokäytössä käynnistä kolmea vanhaa
ohjelmaa lapsiprosesseina eikä riipu niiden viereisistä repository-hakemistoista.
Vanhat repositoryt toimivat toteutuksen lähteenä ja regressiovertailuna.

## 3. Käyttökokemus

### 3.1 Käynnistys ja pääikkuna

Sovelluksen käynnistyessä avautuu yksinkertainen, nopeasti hahmotettava dark
mode -pääikkuna. Sen ensisijaiset kontrollit ovat:

| Moduuli | Pääikkunan kontrollit |
| --- | --- |
| SuperLighter | moduulin/effectin päälle–pois-toggle, **Light Enhancement** -pikanäppäinkenttä ja painike laajoihin asetuksiin |
| Aimoro | tähtäin-overlayn päälle–pois-toggle ja painike laajoihin asetuksiin |
| SoundDirectionVisualizer | suunta-overlayn päälle–pois-toggle ja painike laajoihin asetuksiin |
| Kaikki moduulit | painike keskitettyyn pikanäppäinikkunaan |

Pääikkuna ei kopioi moduulien kaikkia asetuksia. Se näyttää tilan, mahdollistaa
yleisimmät päälle–pois-toiminnot ja toimii reittinä laajoihin asetuksiin.
Light Enhancement -pikanäppäin näkyy sekä pääikkunassa että keskitetyssä
pikanäppäinikkunassa, koska se on tärkein nopea toiminto. Molemmat kentät
muokkaavat samaa asetusta ja päivittyvät aina keskenään.

Sovellus säilyttää nykyisten sovellusten tray-tyyppisen elinkaaren: pääikkunan
sulkeminen piilottaa ikkunan, mutta sovellus ja käyttöön jätetyt moduulit
jatkavat notification area -kuvakkeen kautta. Varsinainen **Exit** sammuttaa
kaikki moduulit hallitusti. Toinen käynnistys ei luo rinnakkaista instanssia,
vaan aktivoi jo käynnissä olevan sovelluksen pääikkunan.

### 3.2 Moduulien käynnistystilat

Versionoidut ensimmäisen käynnistyksen oletustilat lukitaan tämän laitteen
nykyisestä tilanteesta:

| Moduuli/toiminto | Oletustila |
| --- | --- |
| SuperLighter / Enhancement enabled | pois päältä |
| Aimoro / reticle overlay | päällä |
| SoundDirectionVisualizer / direction overlay | päällä |

Toggle tarkoittaa käyttäjälle näkyvän moduulitoiminnon tilaa, ei asetusten
menettämistä. Pois kytketty moduuli säilyttää asetuksensa ja sen laaja
asetusikkuna voidaan yhä avata. Moduulin tulee vapauttaa tai keskeyttää
tarpeettomat aktiiviset resurssit silloin, kun se voidaan tehdä muuttamatta
toiminnon merkitystä. Erityisesti äänen kaappaus ei saa jäädä turhaan aktiiviseksi
SoundDirectionVisualizerin ollessa pois päältä, ja SuperLighterin on palautettava
muuttamansa näyttötila hallitusti.

### 3.3 Moduulikohtaiset laajat asetukset

Jokaisella moduulilla on oma painike, joka avaa sen laajan asetusikkunan.
Ikkunoiden on toiminnallisesti vastattava kyseisen lähdesovelluksen nykyistä
oletusavausikkunaa:

- yhtään nykyistä käyttäjän säädettävissä olevaa asetusta ei saa kadottaa;
- asetusten rajat, normalisointi, live preview, Save/Cancel-käyttäytyminen ja
  saatavuusehdot säilytetään, ellei tässä dokumentissa erikseen muuteta niitä;
- pikanäppäimet ovat edelleen muokattavissa myös moduulien omissa laajoissa
  ikkunoissa;
- moduuli-ikkunan, keskitetyn pikanäppäinikkunan ja pääikkunan samaa asetusta
  esittävät kontrollit käyttävät yhtä ja samaa asetustilaa;
- visuaalinen kieli yhdenmukaistetaan yhteiseen dark mode -teemaan, vaikka
  toiminnallinen sisältö perustuu vanhaan ikkunaan.

Ennen kunkin moduulin porttausta tehdään lähdeikkunasta kontrolli- ja
käyttäytymisinventaario. Se toimii kyseisen moduulin parity-tarkistuslistana ja
liitetään myöhemmin arkkitehtuuri- tai testidokumentaatioon.

### 3.4 Keskitetyt pikanäppäimet

Yksi sovellustason hotkey-palvelu rekisteröi kaikki globaalit pikanäppäimet.
Kolmea erillistä rekisteröijää ei jätetä kilpailemaan samoista Win32-
rekisteröinneistä.

Keskitetty ikkuna sisältää kaikki lähdesovellusten nykyiset toiminnot:

| Moduuli | Toiminto | Uuden asennuksen oletus |
| --- | --- | --- |
| SuperLighter | Toggle Light Enhancement | `Ctrl+Alt+B` |
| SuperLighter | Open detailed settings | ei sidottu |
| Aimoro | Toggle reticle | ei sidottu |
| Aimoro | Cycle displays | ei sidottu |
| Aimoro | Open detailed settings | ei sidottu |
| SoundDirectionVisualizer | Toggle direction overlay | ei sidottu |
| SoundDirectionVisualizer | Cycle displays | ei sidottu |
| SoundDirectionVisualizer | Open detailed settings | ei sidottu |

Näin ollen **kaikki muut pikanäppäimet paitsi Light Enhancement -toggle ovat
oletuksena unbound**, riippumatta lähdesovellusten vanhoista oletuksista tai
tämän laitteen vanhoista sidonnoista.

Pikanäppäinjärjestelmän on lisäksi:

- tuettava sidonnan tyhjentämistä näppäimistöltä;
- estettävä tai selkeästi ilmoitettava sovelluksen sisäinen
  kaksoissidonta ennen tallennusta;
- ilmoitettava, jos Windows tai toinen sovellus estää globaalin rekisteröinnin;
- jätettävä muut, onnistuneet sidonnat käyttöön yhden sidonnan epäonnistuessa;
- päivitettävä kaikki auki olevat samaa sidontaa näyttävät ikkunat;
- rekisteröitävä vain ei-tyhjät sidonnat.

## 4. Visuaalinen linja ja kuvake

Kaikki ikkunat, dialogit, valikot ja notification area -valikot käyttävät yhtä
dark mode -teemaa. Yhteisiä teema- ja kontrollikomponentteja käytetään
moduulikohtaisen kopioinnin sijaan. Teeman tulee säilyä selkeänä vähintään 100,
125 ja 150 prosentin Windows-skaalauksilla, ja näppäimistöfokuksen sekä
disabled/hover/selected-tilojen on erotuttava.

Sovellukselle luodaan uusi tunnistettava kuvake seuraavin reunaehdoin:

- rajattu paletti, tavoitteenaan 3–4 tasaista ja toisistaan erottuvaa väriä;
- yksinkertainen, tiukka vektorigrafiikkatyyli;
- ei valokuvamaisuutta, liukuvärejä, pehmeitä varjoja tai pientä koristeellista
  yksityiskohtaa;
- symboli yhdistää modulaarisuuden ja overlayn ilman, että kolmen vanhan
  kuvakkeen pienoiskuvia vain asetetaan vierekkäin;
- kuvake on tunnistettava myös 16×16-koossa dark- ja light-taustoilla;
- master-omaisuus säilytetään SVG-muodossa ja siitä tuotetaan vähintään
  monikokoinen ICO sekä tarvittavat PNG-koot sovellusta, pikakuvaketta,
  notification area -käyttöä ja dokumentaatiota varten.

Kuvakkeen lopullinen vaihtoehto hyväksytään visuaalisten luonnosten perusteella
ennen sen lukitsemista release-omaisuudeksi.

## 5. Asetukset ja ensimmäisen käynnistyksen migraatio

Uusi ensisijainen asetustiedosto on:

```text
%AppData%\ModularGameOverlay\settings.json
```

Asetukset ryhmitellään sovellustason ja moduulikohtaisiin osiin. Tallennus on
atominen, tuntemattomista tai virheellisistä arvoista palaudutaan dokumentoituihin
oletuksiin, ja yhden moduulin virheellinen osio ei saa nollata muiden moduulien
asetuksia.

Kun uutta asetustiedostoa ei vielä ole, ensimmäisen käynnistyksen migraatio lukee
mahdollisuuksien mukaan:

```text
%AppData%\SuperLighter\settings.json
%AppData%\Aimoro\settings.json
%AppData%\SoundDirectionVisualizer\settings.json
```

Migraatio:

1. tuo moduulien yksityiskohtaiset toiminnalliset ja visuaaliset asetukset;
2. säilyttää yllä kohdassa 3.2 määritellyt nykyisen laitteen päälle–pois-tilat;
3. käyttää pikanäppäimille kohdassa 3.4 määriteltyjä uusia oletuksia vanhojen
   sidontojen tuomisen sijaan;
4. normalisoi arvot uuden sovelluksen samoihin tai erikseen dokumentoituihin
   sallittuihin rajoihin;
5. ei muuta eikä poista vanhoja asetustiedostoja, jotta paluu erillisiin
   sovelluksiin on mahdollinen;
6. on idempotentti: olemassa olevaa uuden sovelluksen asetustiedostoa ei
   ylikirjoiteta seuraavilla käynnistyksillä.

Migraatiosta tehdään automaattiset testit sekä täydellisille, puuttuville,
osittaisille ja vioittuneille lähdeasetuksille. Henkilökohtaisia `%AppData%`-
tiedostoja ei lisätä versionhallintaan testifixtureinä sellaisenaan, vaan niistä
tehdään tarkoitukseen rajatut, anonymisoidut fixturet.

## 6. Tekninen tavoitearkkitehtuuri

Kaikki kolme lähdesovellusta käyttävät tällä hetkellä C#:a, WinFormsia ja
`.NET 9` Windows -kohdistusta. Ensimmäinen toteutus säilyttää tämän teknisen
pohjan, jotta yhdistämisessä voidaan keskittyä elinkaaren ja tilan
yhtenäistämiseen tarpeettoman UI-framework-migraation sijaan.

Tavoiterakenne on vähintään käsitteellisesti seuraava:

```text
src/
  ModularGameOverlay.App/                 # käynnistys, tray, pää- ja yhteiset ikkunat
  ModularGameOverlay.Core/                # moduulisopimukset, asetukset ja hotkey-malli
  ModularGameOverlay.Modules.SuperLighter/
  ModularGameOverlay.Modules.Aimoro/
  ModularGameOverlay.Modules.SoundDirectionVisualizer/
tests/
  ...                                     # core-, moduuli-, migraatio- ja UI-testit
docs/
```

Projektirajat voidaan tarkentaa toteutusvaiheen inventaariossa, mutta seuraavat
periaatteet ovat sitovia:

- yksi host omistaa sovelluksen elinkaaren, single-instance-mekanismin, tray-
  kuvakkeen, yhteisen teeman, asetusten tallennuksen ja hotkey-rekisteröinnin;
- moduuleilla on selkeä käynnistys-, aktivointi-, deaktivointi-, asetusten
  päivitys- ja dispose-sopimus;
- moduulin vika ilmoitetaan käyttäjälle ja eristetään mahdollisuuksien mukaan;
  se ei saa tarpeettomasti kaataa muita moduuleja;
- `SoundDirectionVisualizer.Core`-logiikka säilytetään käyttöjärjestelmästä
  riippumattomana ja deterministisesti testattavana;
- overlay-ikkunoiden click-through-, no-activation- ja topmost-käyttäytyminen
  säilytetään;
- SuperLighterin gamma-, värimatriisi- ja brightness-muutoksille säilytetään
  hallittu palautus deaktivoinnissa ja normaalissa sulkemisessa;
- sovelluksen sulkeminen lopettaa äänen kaappauksen, overlayt, hotkeyt,
  background watcherit ja muut resurssit deterministisesti;
- vanhojen repoiden lähdekoodi tuodaan uuteen itsenäiseen repositoryyn
  jäljitettävästi. Tuotantokoodi ei käytä absoluuttisia polkuja vanhoihin
  hakemistoihin.

## 7. Testaus- ja dokumentointistrategia

### 7.1 Lähtötason varmistus

Ennen porttausta:

1. kirjataan kunkin lähderepon commit, build-ohje, asetusskeema, nykyiset
   kontrollit ja tunnetut rajoitteet;
2. ajetaan lähderepojen olemassa olevat Release-buildit ja testit/self-testit;
3. kirjataan mahdolliset jo lähtötilanteessa epäonnistuvat testit erilleen;
4. SoundDirectionVisualizerin nykyinen automaattinen testikattavuus siirretään
   ilman hiljaista kaventamista;
5. SuperLighterin self-testit säilytetään ja muutetaan tarvittaessa osaksi
   yhteistä automaattista testiajoa;
6. Aimoron puuttuville kriittisille käyttäytymisille lisätään porttauksen
   yhteydessä regression suojaavat testit.

### 7.2 Automaattiset testit

Vähimmäiskattavuuteen kuuluvat:

- uudet oletukset ja asetusten normalisointi;
- kolmesta vanhasta formaatista tehtävä migraatio ja sen idempotenssi;
- hotkeyn tyhjennys, validointi, kaksoissidonnat, synkronointi ja
  rekisteröintivirheet;
- moduulien elinkaari sekä resurssien käynnistys ja vapautus toggleilla;
- pääikkunan kontrollit ja reitit kaikkiin asetusikkunoihin;
- yhden asetuksen synkronointi pääikkunan, keskitetyn ikkunan ja
  moduuli-ikkunan välillä;
- single-instance-käyttäytymisen testattava osa;
- moduulien nykyiset laskenta-, audio-, näyttö-, kohdistus- ja
  renderöintiregressiot;
- yhteisen dark-teeman olennaiset kontrollit ja saavutettavuustilat.

Lopullinen ratkaisu tarjoaa yhden dokumentoidun komennon koko Release-buildin
ja testisarjan ajamiseen. Build- tai testiajosta ei raportoida onnistunutta, jos
olennainen testi on jätetty ajamatta ilman kirjattua syytä.

### 7.3 Manuaaliset smoke-testit

Automaation lisäksi tarkistetaan oikealla Windows-laitteella vähintään:

- pääikkunan ja kolmen laajan asetusikkunan toiminta 100/125/150 % DPI:llä;
- kaikkien overlayden click-through, no-activation, topmost ja moninäyttötoiminta;
- pelin fokuksen säilyminen;
- Light Enhancementin käyttöönotto, poiskytkentä ja näytön tilan palautus;
- Aimoron hold-to-show, näyttökohdistus ja tähtäimen live-muutokset;
- SoundDirectionVisualizerin audio capture-, fallback-, monikanava- ja
  overlay-käyttäytyminen sen oman testidokumentaation tasolla;
- hotkeyt, unbound-tila, ristiriitailmoitukset ja muokkausten synkronointi;
- asetusten säilyminen uudelleenkäynnistyksessä ja migraatio vanhoista
  asetuksista;
- moduulin poiskytkeminen ilman muiden moduulien häiriötä;
- hallittu Exit ja kaikkien muutosten/resurssien palautuminen;
- sovelluskuvakkeen luettavuus ikkunassa, tray-kuvakkeena, EXE:ssä ja
  työpöydän pikakuvakkeessa.

Yksityiskohtainen, rastitettava smoke-testilista luodaan tiedostoon
`docs/TESTING.md` viimeistään ensimmäisen toimivan integroidun buildin yhteydessä.

## 8. Toteutusvaiheet ja portit

### Vaihe 0 – baseline ja jäljitettävyys

- inventoi lähderepot, niiden commitit, ominaisuudet, asetukset ja testit;
- aja lähtötason buildit/testit;
- tee parity-matriisi jokaiselle moduulille;
- varmista, että vanhoja repoja ei tarvitse muuttaa yhdistämistä varten.

**Portti:** lähtötaso ja tunnetut poikkeamat on dokumentoitu.

### Vaihe 1 – host ja yhteinen perusta

- luo solution, App/Core-projektit ja testiprojektit;
- toteuta single-instance-host, tray-elinkaari ja yksinkertainen pääikkuna;
- toteuta yhteinen moduulisopimus, dark-teema ja asetusskeema;
- toteuta migraatio fixtureineen.

**Portti:** tyhjä host käynnistyy, tallentaa asetukset ja läpäisee core- sekä
migraatiotestit.

### Vaihe 2 – moduulien porttaus

- porttaa SuperLighter, Aimoro ja SoundDirectionVisualizer yksi kerrallaan;
- säilytä kunkin moduulin testit ja lisää elinkaaren integraatiotestit;
- varmista parity-matriisi ja manuaalinen smoke-testi ennen seuraavaan moduuliin
  siirtymistä.

**Portti:** jokainen moduuli toimii hostissa yksinään ilman vanhan EXE:n
käynnistämistä.

### Vaihe 3 – asetukset ja keskitetyt hotkeyt

- viimeistele pääikkunan moduulitogglet ja laajojen ikkunoiden avaus;
- toteuta yksi hotkey-malli, rekisteröijä ja keskitetty hotkey-ikkuna;
- synkronoi Light Enhancement -kenttä kaikkiin kolmeen esityspaikkaan;
- varmista uudet unbound-oletukset ja ristiriitojen käsittely.

**Portti:** kaikki asetuspinnat ovat yhdenmukaisia ja hotkey-testit läpäisevät.

### Vaihe 4 – visuaalinen yhtenäistys ja kuvake

- yhdenmukaista ikkunat, kontrollit ja tray-valikko;
- tee 3–4 värin vektorikuvakeluonnokset, hyväksy yksi ja generoi assetit;
- tarkista DPI, näppäimistökäyttö ja eri taustat.

**Portti:** visuaalinen QA ja kuvakekokojen tarkistus on tehty.

### Vaihe 5 – paketointi ja stabilointi

- tee yksi toistettava Release-build/publish-komento Windows x64:lle;
- aja koko testisarja ja yhdistetty manuaalinen smoke-testi;
- viimeistele README, arkkitehtuuri-, testaus-, julkaisu- ja changelog-dokumentit;
- varmista git-ohitetun paikallisohjeen mukainen työpöydän pikakuvake;
- testaa käyttöönotto puhtaalla asetushakemistolla ja tämän laitteen
  migraatiolla.

**Portti:** Definition of Done täyttyy ja build voidaan luovuttaa yhtenä
sovelluksena.

## 9. Riskit ja hallintakeinot

| Riski | Hallinta |
| --- | --- |
| Kolme itsenäistä `ApplicationContext`- ja tray-elinkaarta törmäävät | yksi host ja eksplisiittiset moduulien lifecycle-rajapinnat |
| Globaalit hotkeyt törmäävät keskenään tai muihin sovelluksiin | yksi rekisteri, unbound-oletukset, esivalidointi ja näkyvä virheilmoitus |
| SuperLighter jättää näytön muokattuun tilaan virheen tai sulkemisen jälkeen | säilytetään alkuperäisen tilan capture/restore, idempotentti deaktivointi ja smoke-testit |
| Audio capture käyttää resursseja overlayn ollessa pois | moduulin deaktivointi pysäyttää capture-ketjun ja testaa uudelleenkäynnistyksen |
| Asetusmigraatio kadottaa henkilökohtaiset arvot | vanhoja tiedostoja ei muuteta, atominen uusi tallennus ja fixture-pohjaiset testit |
| UI-parity heikkenee visuaalisen yhtenäistyksen yhteydessä | kontrolli- ja käyttäytymismatriisi sekä moduulikohtainen hyväksyntäportti |
| SoundDirectionVisualizerin audioalgoritmi muuttuu vahingossa | Core säilyy deterministisenä ja nykyiset testit siirretään ennen refaktorointia |
| Jaetut Win32-resurssit vuotavat tai sulkeutuvat väärässä järjestyksessä | eksplisiittinen omistajuus, dispose-järjestys ja lifecycle-integraatiotestit |

## 10. Definition of Done

Ensimmäinen yhdistetty julkaisu on valmis vasta, kun kaikki seuraavat täyttyvät:

- nimi, EXE, prosessi, asetushakemisto, tray-kuvake ja käyttäjälle näkyvä
  tuotebrändi ovat `ModularGameOverlay`;
- sovellus toimii yhdestä EXE:stä ilman kolmen vanhan sovelluksen binäärejä;
- yksinkertainen pääikkuna avautuu käynnistyksessä ja sisältää vaaditut togglet,
  moduuliasetuspainikkeet, Light Enhancement -hotkeyn ja keskitetyn
  hotkey-painikkeen;
- moduulien oletustilat ovat SuperLighter pois, Aimoro päällä ja
  SoundDirectionVisualizer päällä;
- kaikki vanhojen laajojen asetusikkunoiden säädöt ja keskeinen käyttäytyminen
  ovat parity-matriisin mukaan tallella;
- vain `Ctrl+Alt+B` / Light Enhancement on oletuksena sidottu ja kaikki muut
  olemassa olleet toiminnot ovat unbound;
- sama hotkey-asetus pysyy synkronissa kaikissa ikkunoissa ja ristiriidat
  käsitellään selkeästi;
- nykyiset asetukset migroituvat turvallisesti ja vanhat tiedostot säilyvät;
- dark mode -ilme on yhdenmukainen ja hyväksytyt kuvakeassetit ovat käytössä;
- koko Release-build ja automatisoitu testisarja läpäisevät;
- yhdistetty manuaalinen smoke-testilista on suoritettu ilman avoimia
  release-estäviä havaintoja;
- dokumentaatio ja testit vastaavat toimitettua käyttäytymistä;
- työpöydän `ModularGameOverlay.lnk` osoittaa paikallisohjeen mukaisesti
  viimeisimpään onnistuneeseen julkaistuun buildiin.

## 11. Työoletukset, jotka voidaan muuttaa ennen toteutusta

Seuraavia oletuksia käytetään, jotta toteutus voi edetä ilman tarpeettomia
aukkoja. Niitä voidaan muuttaa päivittämällä tämä dokumentti ennen vastaavaa
toteutusta:

- käyttöliittymän tekstit säilyvät ensimmäisessä versiossa englanninkielisinä,
  koska lähdesovellukset ovat englanninkielisiä;
- Windows 10/11 x64 ja self-contained single-file publish säilyvät ensisijaisena
  jakelutapana;
- vanhoja asetuksia luetaan vain migraatiossa eikä niihin kirjoiteta takaisin;
- vanhat sovellukset jäävät erillisiksi rollback-vaihtoehdoiksi, mutta niitä ei
  ole tarkoitus ajaa samanaikaisesti uuden hostin kanssa;
- pääikkunalle ei lisätä uutta oletuksena sidottua hotkeytä, koska käyttäjä on
  määritellyt Light Enhancementin ainoaksi oletuksena sidotuksi toiminnoksi.

## 12. Suunnitteluvaiheen lähtöinventaario

Seuraava lähtötilanne tarkistettiin 23.8.2026 ennen tämän dokumentin luomista.
Kaikki kolme lähderepositorya olivat tarkistushetkellä puhtaita ja niiden
paikallinen `main` vastasi `origin/main`-haaraa.

| Lähde | Tarkistettu commit | Nykyinen testipohja |
| --- | --- | --- |
| SuperLighter | `71638eb` (`v1.3.0`) | Release-build ja sovelluksen `--self-test` |
| Aimoro | `fc734ba` | build-ohje on dokumentoitu; erillistä testiprojektia ei havaittu |
| SoundDirectionVisualizer | `cfbcf28` | laajat Core- ja App-xUnit-testit sekä manuaalinen smoke-testiohje |

Kaikki käyttävät WinFormsia ja `.NET 9` Windows -kohdistusta. Tämän laitteen
olemassa olevat asetustiedostot löytyivät kaikille kolmelle moduulille, ja
kohdan 3.2 päälle–pois-oletukset on johdettu niistä. Yksityiskohtaisia
henkilökohtaisia asetuksia ei kopioitu tähän dokumenttiin, koska migraation tulee
lukea ne lähdetiedostoista ja testien tulee käyttää rajattuja fixturejä.

Tämä inventaario ei vielä tarkoita, että lähderepojen buildit ja testit olisi
ajettu tässä uudessa projektissa. Se tehdään vaiheen 0 hyväksyntäporttina ennen
lähdekoodin porttausta.
