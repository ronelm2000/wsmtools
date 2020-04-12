WSMTools (Weiss Schwarz Montage Tools)
===========
![.NET Core](https://github.com/ronelm2000/wsmtools/workflows/.NET%20Core/badge.svg)
[![CodeFactor](https://www.codefactor.io/repository/github/ronelm2000/wsmtools/badge)](https://www.codefactor.io/repository/github/ronelm2000/wsmtools)

Weiss Schwarz ©bushiroad All Rights Reserved.

This a CLI (Command Line Interface) tool intended to parse through, process, and export data regarding to Weiss Schwarz cards; specifically, this tool's intention is to make querying, parsing, and exporting cards from various sources easier, as
well as provide APIs to expand on that functionality.

For now this tool is barebones (in its alpha stage, so expect some bugs), but I'm hoping it will be able to perform the following:
* Parse cards into a local database for querying.
* Parse decks using the local database.
* Export decks for use into another format.

Somewhat stable pre-releases are on the [appropriate link](https://github.com/ronelm2000/wsmtools/releases), but if you're having some issues with them, you can also try the [latest build](https://github.com/ronelm2000/wsmtools/actions) by
registering on GitHub. 

#### Supported Exporters ####
* [Tabletop Simulator](https://steamcommunity.com/sharedfiles/filedetails/?id=1321170886)

#### Supported Deck Parsers ####
* [Encore Decks](https://www.encoredecks.com/)

#### Supported Card Set Parsers ####
* HOTC
* Encore Decks (through their API)
* [English Weiss Schwarz Official Website](https://en.ws-tcg.com/)

---------

*I know you're probably just here just so that you can quickly netdeck into TTS; so how do you do it?*
1. Execute this lovely command on PowerShell.
   ```ps
   ./wstools export your_encore_decks_deck_link_here
   ```
2. You will be warned if any of the following are true.
   * Your deck contains cards without English translations.
   * Your deck contains cards which have no saved image link. You will be prompted for an image link if you continue.
3. You will get the following on your Exports folder: `deck.your deck name.png`, `Deck Generator (your deck name).png`,  `Deck Generator (your deck name).png`, and `Deck Generator (your deck name).json`.
4. The `Deck Generator` files go into your Save Objects folder (typically `%HOMEDRIVE%%HOMEPATH%\Documents\My Games\Tabletop Simulator\Saves\Saved Objects`)
5. The `deck.your deck name.png` should be uploaded (imgur or Steam Cloud using Tabletop Simulator)
6. Open Tabletop Simulator (Single-Player will do)
7. Load the Saved Object (and make sure no other Deck Generators are loaded!)
8. In the chat command, type:
   ```ps
   +generate url_of_your_deck url_of_your_sleeves
   ```
9. You should be able to create decks like this:
   ![Tho why do you need effects for decks with English art?](https://i.imgur.com/WuRpf9I.png)

---------

### Known Issues ### 
* Some decks from Encore Decks will be untranslated. This is true for all sets without a community translation.
  In order to resolve this, you must (begrudgingly) use HOTC translations by running the following command first.
  Personally, I discourage anyone with a 10-foot pole to use it because they don't like people using their translations
  at all, but the tools are there if you really want to.
  ```ps
  ./wstools parse url_of_translations_summary_html_page
  ```
  The HTML page is usually in this link.

  ![Yes, that one link which is printed](https://i.imgur.com/FkukMso.png)
* ~~HOTC's Weiss Promos and Schwarz Promos are currently an unsupported use case as they contain all promos
  from every single set for that particular side. Future support may include parsing that page as well, but
  given their design and their copyright, almost any compromises leads to an uptick in unneeded calls on their site.~~
  Resolved, you should be able to export them, but they will have no images. This is done intentionally so that all image
  post-processors will not load 10 or more images at once. Please add them manually.

* Some HOTC pages have the wierdest problems, like this from Magia Record. Please report them if seen.

  ![Why!?!](https://i.imgur.com/NdpGGp0.png)