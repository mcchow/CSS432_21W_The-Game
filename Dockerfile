FROM mcr.microsoft.com/dotnet/sdk:3.1
WORKDIR /app
ADD TriviaGame.db /app/TriviaGame.db
ADD TriviaGameServer /app/TriviaGameServer
ADD TriviaGameProtocol /app/TriviaGameProtocol
RUN dotnet dotnet build --project TriviaGameServer
CMD ["dotnet", "run", "--project", "TriviaGameServer"]
