#!/bin/bash

apt update
apt install libfreetype6
dotnet publish TriviaGame -o PublishedTriviaGame -r win-x64
