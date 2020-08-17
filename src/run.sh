#!/bin/bash
docker run -t --name=JustPlanIt -p 5000:80 -e ASPNETCORE_URLS=http://+:5000 wendero/justplanit:latest