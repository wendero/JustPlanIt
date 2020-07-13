#!/bin/bash
dotnet publish -c Release -o publish
docker build -t wendero/justplanit:0.1 -t wendero/justplanit:latest .