#!/bin/bash
dotnet publish -c Release -o publish
docker build -t wender/justplanit:0.1 -t wender/justplanit:latest .