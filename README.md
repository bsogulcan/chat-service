# Chat Service
Sample project about .Net Asynchronous Socket Programming.
## Logic
> Multiple clients can connect to the server and sending messages to server. If a client send more then one messages in one second to server, server send warning message to client. If client continue sending more than one messages in a second, server thinks that client is dangerous for me and disconnect with it.
## Start a Server
```
dotnet run --project .\src\Server\Server.csproj
```
## Create a client as much as you want
```
dotnet run --project .\src\Client\Client.csproj
```
## Build
```
dotnet build .\ChatService.sln
```

## Test
```
dotnet test .\ChatService.sln
```
https://user-images.githubusercontent.com/56725731/184511715-a1dd2305-b4fc-48e3-8482-768297f1869b.mp4
