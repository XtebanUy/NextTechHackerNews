# Project Name

A brief description of your project.

## Table of Contents

- [Installation](#installation)
- [Assumptions](#assumptions)

## Installation

Instructions on how to install and run your project:

1. Clone the repository: `git clone https://github.com/XtebanUy/NextTechHackerNews.git`
2. Install node 16.20.0
3. Install dotnet 7.0 sdk
4. Navigate to the project directory: `cd NextTechHackerNews`
5. Start the application: `dotnet run --project src/WebUI/WebUI.csproj `

This app could be run using Docker and VSCode with the devcontaier extension installed without install dotnet and angular

## Assumptions

During a request
1) The newstories endpoint is hit (https://hacker-news.firebaseio.com/v0/topstories.json)
2) Search in the local cache for the story ids retrieved
  1) If if is not present, dowload the story from   https://hacker-news.firebaseio.com/v0/item/{storyid}.json?print=pretty
  2) if existing story is not in the newstories, remove ths story form cache
3) get the updated stories from https://hacker-news.firebaseio.com/v0/updates.json and update any story which is cached

Probably some stories from the updates endpoint have not changed since the last update but it is not possible to determine it

I spent 40 hours to develop this solution. I use docker with vscode and devcontainer extension to develop because it is easy to share the dev environment 
The web ui project was based on the angular template of dotnet templates. 
I used angular material to delevelop the ui
I used swagger to generate the api documentation and proxies to consume the api from the ui
