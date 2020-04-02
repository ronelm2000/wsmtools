WSMTools (Weiss Schwarz Montage Tools)
===========
![.NET Core](https://github.com/ronelm2000/wsmtools/workflows/.NET%20Core/badge.svg)

This a tool intended to parse through and export data regarding to Weiss Schwarz cards; specifically, this tool's intention is to make querying, parsing, and exporting cards from various sources easier, as
well as provide APIs to expand on that functionality.

For now this tool is barebones (in its alpha stage, so expect some bugs), but I'm hoping it will be able to perform the following:
* Parse cards into a local database for querying.
* Parse decks using the local database.
* Export decks for use into another format.

### Supported Exporters ###
* [Tabletop Simulator](https://steamcommunity.com/sharedfiles/filedetails/?id=1321170886)

### Supported Deck Parsers ###
* [Encore Decks](https://www.encoredecks.com/)

---------

*I know you're probably just here just so that you can quickly netdeck into TTS; so how do you do it?*
1. Execute this lovely command.
  ```ps
  ./wstools export your_encore_decks_deck_link_here
  ```
2. You will get the following on your Exports folder: `deck.your deck name.png`, `Deck Generator (your deck name).png`,  `Deck Generator (your deck name).png`, and `Deck Generator (your deck name).json`.
3. The `Deck Generator` files go into your Save Objects folder (typically `%HOMEDRIVE%%HOMEPATH%\Documents\My Games\Tabletop Simulator\Saves\Saved Objects`)
4. The `deck.your deck name.png` should be uploaded (imgur or Steam Cloud using Tabletop Simulator)
5. Open Tabletop Simulator (Single-Player will do)
5. Load the Saved Object (and make sure no other Deck Generators are loaded!)
6. In the chat command, type:
  ```ps
  +generate url_of_your_deck url_of_your_sleeves
  ```
