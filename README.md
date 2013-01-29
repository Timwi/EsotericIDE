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

## Most wanted implementations

* **[Befunge-98](http://esolangs.org/wiki/Befunge)**
* **[Malbolge](http://esolangs.org/wiki/Malbolge)**
* **[Rail](http://esolangs.org/wiki/Rail)**

## Incomplete implementations

There are incomplete implementations for these included in the source. However, the C# project (the `csproj` file) does not include them, so the IDE doesn’t list them. You will have to include the files in the Esoteric IDE project and recompile it.

* **[Funciton](http://esolangs.org/wiki/Funciton)** — Struggling to support stepwise debugging for this one. For the moment, use [Funciton Interpreter](https://bitbucket.org/Timwi/funciton-interpreter) instead, which includes a Funciton-to-IL compiler.
* **[Mornington Crescent](http://esolangs.org/wiki/Mornington_Crescent)** — No support for the jumpstack.