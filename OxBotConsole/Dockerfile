# Use the official Microsoft .NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/runtime:7.0.0

# Set the working directory in the container
WORKDIR /app

# Copy the built application to the container
COPY ./bin/Release/net7.0/publish/ /app/

RUN apt-get update && \
    apt-get upgrade -y && \
    rm -rf /var/lib/apt/lists/*

# Command to run the application
CMD ["dotnet", "OxBotConsole.dll"]
