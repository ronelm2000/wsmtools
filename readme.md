WSMTools (Weiss Schwarz Montage Tools)
===========
[![Nightly](https://github.com/ronelm2000/wsmtools/actions/workflows/nightly.yml/badge.svg)](https://github.com/ronelm2000/wsmtools/actions/workflows/nightly.yml)
[![CodeFactor](https://www.codefactor.io/repository/github/ronelm2000/wsmtools/badge)](https://www.codefactor.io/repository/github/ronelm2000/wsmtools)
[![Discord](https://img.shields.io/discord/831048458608705627?label=Discord)](https://discord.gg/9T55jJGHJD)
[![Downloads](https://img.shields.io/github/downloads/ronelm2000/wsmtools/total.svg)](https://tooomm.github.io/github-release-stats/?username=ronelm2000&repository=wsmtools)

Weiss Schwarz ©bushiroad All Rights Reserved.

This a CLI (Command Line Interface) tool intended to parse through, process, and export data regarding to Weiss Schwarz cards; specifically, this tool's intention is to make querying, parsing, and exporting cards from various sources easier, as
well as provide APIs to expand on that functionality.

For now this tool is barebones (in its alpha stage, so expect some bugs), but I'm hoping it will be able to perform the following:
* Parse cards into a local database for querying.
* Parse decks using the local database.
* Export decks for use into another format.
* Export the entire local database (or part of it) into another format.

Somewhat stable releases are on the [appropriate link](https://github.com/ronelm2000/wsmtools/releases), but if you're having some issues with them, you can also try the [latest build](https://github.com/ronelm2000/wsmtools/actions) by
registering on GitHub. 

[If you're seeing this and just want the instructions to netdeck, click here.](#gui)

#### Supported Deck Exporters
* [Tabletop Simulator](https://steamcommunity.com/sharedfiles/filedetails/?id=1321170886)
  ```ps
  ./wstools export https://www.encoredecks.com/deck/CW5ThLgZ7 --with sendtcp
  ```
* Weiss Schwarz Card Game Simulator by Blake Thoennes
  ```ps
  ./wstools export https://www.encoredecks.com/deck/CW5ThLgZ7 --exporter bws
  ```
* [DeckLog](https://decklog-en.bushiroad.com)
  ```ps
  ./wstools export https://www.encoredecks.com/deck/CW5ThLgZ7 --exporter decklog
  ```
* Local Deck JSON
  ```ps
  ./wstools export https://www.encoredecks.com/deck/CW5ThLgZ7 --exporter local
  ```
* Proxy Document (A4 Landscape)
  ```ps
  ./wstools export https://www.encoredecks.com/deck/CW5ThLgZ7 --exporter doc
  ```

#### Supported Database Exporters
* [Cockatrice](https://github.com/longagofaraway/Cockatrice)
* Local (via `.ws-set` format)

#### Supported Deck Parsers
* [Encore Decks](https://www.encoredecks.com/)
  ```ps
  ./wstools export https://www.encoredecks.com/deck/CW5ThLgZ7 --with sendtcp
  ```
* [Deck Log](https://decklog.bushiroad.com/)
  ```ps
  ./wstools export https://decklog-en.bushiroad.com/view/EQ53 --with sendtcp
  ```
* Local Deck JSON
  ```ps
  ./wstools export ./my_cool_deck.json --with sendtcp
  ```

#### Supported Card Set Parsers
* HOTC
* Encore Decks (through their API)
* [English Weiss Schwarz Official Website](https://en.ws-tcg.com/)
* Local (via `.ws-set` format)

---------

*I know you're probably just here just so that you can quickly netdeck; so how do you do it?*
* [GUI (New!)](#gui)
* [GUI For Custom Sets (New!)](#custom-sets)
* [For Tabletop Simulator (TTS)](#tabletop-simulator)
* [For Weiss Schwarz Card Game Simulator by Blake Thoennes (BlakeWS)](#weiss-schwarz-card-game-simulator-by-blake-thoennes)

#### GUI
01. Install [ShareX](https://getsharex.com/) (Do not use the portable edition).
02. Extract [the binaries](https://github.com/ronelm2000/wsmtools/releases) into the folder of your choice.
03. Open `wsm-gui.exe` on that folder. (For Linux users, be sure to `chmod` it.)
    This will then give you access to the GUI.
04. Go to `File` and click on `Import Deck...`
    01. Copy / Paste the URL of the deck you're netdecking (Both EncoreDecks and DeckLog are supported!)
    02. Wait for the deck to load in.
05. Hover over each image to check the translated text. If there are none, you can right-click on them to change / add in translations.
06. Open Tabletop Simulator, and create any game. (Single-Player / Multi-Player / Hotseat)
07. Go to `Export` and click on `Tabletop Simulator`.
    - You will be warned if there are any cards that have no translations (or are considered to have not been translated properly.)
08. Check the status on the Status Bar below. Once it says that the deck has been exported, a pop-up UI will open up on TTS.
09. If ShareX was installed properly, your deck image should also be uploaded.
10. *(Optional)* You will get the following on your Exports folder if 07 fails for any reason and/or you need to re-upload images: `deck_your deck name.jpg`, `Deck Generator (your deck name).png`,  `Deck Generator (your deck name).png`, and `Deck Generator (your deck name).json`.
    01. Put the `Deck Generator` files into your Save Objects folder (typically `%HOMEDRIVE%%HOMEPATH%\Documents\My Games\Tabletop Simulator\Saves\Saved Objects`)
    02. Upload `deck_your deck name.jpg` to your image hoster of choice.
    03. Load the Saved Object (and make sure no other Deck Generators are loaded!)
11. In the loaded GUI, place the Deck Image URL Link where provided. *(Optional)* Place your Character Sleeves URL Link where provided.
    - In ShareX, obtain the Deck Image Link by right-click on the uploaded task, and go to Copy > URL.
12. You should be able to create decks like this:
    ![Tho why do you need effects for decks with English art?](https://i.imgur.com/WuRpf9I.png)

#### Custom Sets
00. Prepare your Custom Set.
    01. Make sure your [custom set in MSE](https://github.com/ronelm2000/weiss-mse-plugin) has the correct Set ID: `WC`, `SC` or `WSC` corresponds to Weiss, Schwarz, and Both, respectively. (eg. `SC563`).
    02. Be sure to follow the [guide here](https://github.com/ronelm2000/weiss-mse-plugin/wiki/Exporting-to-wsmtools#exporting-your-set) for exporting the set for use in `wsm-gui`.
    03. Do not delete the images, as they will be needed to automatically copy them to wsmtools.
01. Install [ShareX](https://getsharex.com/) (Do not use the portable edition).
02. Extract [the binaries](https://github.com/ronelm2000/wsmtools/releases) into the folder of your choice.
03. Open `wsm-gui.exe` on that folder. (For Linux users, be sure to `chmod` it.)
    This will then give you access to the GUI.
04. Go to `File` and click on `Import Local Set`, then select the the `.ws-set` from your custom set.
05. Once loaded, you should be able to search for the cards and create the deck manually through the GUI.
06. Follow Step #06 onwards from the [GUI Guide](#gui) to export your finished deck into TTS. For obvious reasons, you cannot export the deck to DeckLog or EncoreDecks, but other export options like Deck Proxy will work as normal.

#### Tabletop Simulator
01. Install [ShareX](https://getsharex.com/) (Do not use the portable edition).
02. Extract [the binaries](https://github.com/ronelm2000/wsmtools/releases) into the folder of your choice.
03. Open PowerShell on that folder. This can be done by holding [Shift] then Right-Click while on the folder.
    ![I like dark mode Windows Explorer](https://i.imgur.com/MBc4zzr.png)
04. Open Tabletop Simulator, and create any game. (Single-Player / Multi-Player / Hotseat)
05. Execute this command.
   ```ps
   ./wstools export your_encore_decks_deck_link_here --with sendtcp --out sharex
   ```
06. You will be warned if any of the following are true.
    * Your deck contains cards without an English translation. (Proceed to *Known Issues* before continuing.)
    * Your deck contains cards which have no saved image link. You will be prompted for an image link if you continue.
07. A UI should open up in your TTS game, with the Deck Name as indicated.
08. If ShareX was installed properly, your Deck PNG should be uploaded.
    ![I like ShareX this way; it's so convenient](https://i.imgur.com/Sw2H9qm.png)
07. *(Optional)* You will get the following on your Exports folder if 07 fails for any reason and/or you need to re-upload images: `deck_your deck name.jpg`, `Deck Generator (your deck name).png`,  `Deck Generator (your deck name).png`, and `Deck Generator (your deck name).json`.
    01. Put the `Deck Generator` files into your Save Objects folder (typically `%HOMEDRIVE%%HOMEPATH%\Documents\My Games\Tabletop Simulator\Saves\Saved Objects`)
    02. Upload `deck_your deck name.jpg` to your image hoster of choice.
    03. Load the Saved Object (and make sure no other Deck Generators are loaded!)
08. In the loaded GUI, place the Deck Image URL Link where provided. *(Optional)* Place your Character Sleeves URL Link where provided.
    - In ShareX, obtain the Deck Image Link by right-click on the uploaded task, and go to Copy > URL.
09. You should be able to create decks like this:
    ![Tho why do you need effects for decks with English art?](https://i.imgur.com/WuRpf9I.png)

#### Weiss Schwarz Card Game Simulator by Blake Thoennes
01. Run the application.
02. Create a deck named `[wstools import]` and put any legal deck in it.
03. Extract [the binaries](https://github.com/ronelm2000/wsmtools/releases) into the folder of your choice.
04. Open PowerShell on that folder. This can be done by holding [Shift] then Right-Click while on the folder.
05. Execute this command.
   ```ps
   ./wstools export your_encore_decks_deck_link_here --exporter blake
   ```
06. The deck which was named `[wstools import]` should now have been replaced with the exported deck; open it to check for any missing sets.
07. Once you've verified all the cards are complete without any missing sets/cards, save it as a new deck of your choice.
    ![Nothing like an anti-memory memory deck](https://i.imgur.com/9svzkYz.png)
---------

### Known Issues
* The English Weiss Schwarz exporter is currently not working due to the new website design. This will be fixed in a new version.
* Some decks from Encore Decks will be untranslated. This is true for all sets without a community translation.
  In order to resolve this, you must (begrudgingly) use HOTC translations by running the following command first.
  Personally, I discourage anyone with a 10-foot pole to use it because they don't like people using their translations
  at all, but the tools are there if you really want to.
  ```ps
  ./wstools parse url_of_translations_summary_html_page
  ```
  The HTML page is usually in this link.

  ![Yes, that one link which is printed](https://i.imgur.com/FkukMso.png)
* HOTC's Weiss Promos and Schwarz Promos do not use YYT (yuyutei) images by default, but instead utilizes DeckLog API image links. If you require the YYT's image links for any reason, they will be included if you parse a
  set from HOTC of the same Release ID. There are also some PRs that are report to have neither DeckLog images nor YYT images. For these instances, if you try to create a deck with them, you will be warned and be asked
  to include them manually.

* Some HOTC pages may not parse successfully due to translation errors, like this from Magia Record.
  Please report them if seen.

  ![Why!?!](https://i.imgur.com/NdpGGp0.png)

* There are alot of missing serials for BlakeWS, including cards that may already have an English card serial but not Japanese.

### Building from Source
1. Install Visual Studio 2022 17.4.1.
2. Install .NET 9.0 SDK
3. Go to `Tools` > `Options`, then go to `Environment` > `Preview Features".
4. Check `Use Previews ot the .NET SDK`, then restart VS 2022. 
5. Open the `.sln` file.
6. Build all projects as necessary. (The startup project is `Monatage.Weiss.Tools`.)