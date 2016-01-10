# Esoteric IDE

Esoteric IDE is an interpreter and debugger for some *esoteric programming languages* (or *esolangs*).

## Features

* Allows you to run code in any supported esolang
* Allows you to set breakpoints
* Allows you to debug through code step by step
* Displays the execution state (program state) at every step (kinda like a watch window)
* In some esolangs, displays information about the instruction the cursor is on

## Supported Languages

In chronological order of implementation in Esoteric IDE:

* **[Sclipting](http://esolangs.org/wiki/Sclipting)** — Similar to GolfScript but using Chinese characters.
* **[Ziim](http://esolangs.org/wiki/Ziim)** — 2D language using only arrows (← ↑ → ↓ etc.).
* **[Brainfuck](http://esolangs.org/wiki/Brainfuck)** — The classic. Supports many different flavours, e.g. cells can be byte-size or arbitrary-size; output can be as numbers or as Unicode characters.
* **[Quipu](http://esolangs.org/wiki/Quipu)** — Inspired by the ancient Inca’s [quipu](http://en.wikipedia.org/wiki/Quipu) system of recording information as knots in a thread, thus also known as *talking knots*.
* **[Unreadable](http://esolangs.org/wiki/Unreadable)** — Consists only of apostrophe (`'`) and double-quote (`"`) characters.
* **[Mornington Crescent](http://esolangs.org/wiki/Mornington_Crescent)** — Travel on the London Underground, but remember to always come back to Mornington Crescent.
* **[Hexagony](http://esolangs.org/wiki/Hexagony)** — Program instructions and memory locations are laid out in a 2D hexagonal grid.
* **[Labyrinth](http://esolangs.org/wiki/Labyrinth)** — Two-dimensional stack-based language where the code can self-modify by applying cycling rotations of rows or columns of characters.

## How to compile

You need to check out [RT.Util](https://github.com/RT-Projects/RT.Util) as well. Place it in another folder (called RT.Util) at the same level as Esoteric IDE.