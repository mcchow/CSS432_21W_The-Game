FROM mcr.microsoft.com/dotnet/sdk:3.1
WORKDIR /app
ADD . /app
CMD ["dotnet", "run", "--project", "TriviaGameServer"]
