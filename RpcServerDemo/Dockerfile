FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["RpcServerDemo/RpcServerDemo.csproj", "RpcServerDemo/"]
COPY ["sofa-bolt-csharp-test/sofa-bolt-csharp-test.csproj", "sofa-bolt-csharp-test/"]
COPY ["sofa-bolt-csharp/sofa-bolt-csharp.csproj", "sofa-bolt-csharp/"]
COPY ["src/DotNetty.Handlers/DotNetty.Handlers.csproj", "src/DotNetty.Handlers/"]
COPY ["src/DotNetty.Buffers/DotNetty.Buffers.csproj", "src/DotNetty.Buffers/"]
COPY ["src/DotNetty.Common/DotNetty.Common.csproj", "src/DotNetty.Common/"]
COPY ["src/DotNetty.Codecs/DotNetty.Codecs.csproj", "src/DotNetty.Codecs/"]
COPY ["src/DotNetty.Transport/DotNetty.Transport.csproj", "src/DotNetty.Transport/"]
COPY ["src/DotNetty.Transport.Libuv/DotNetty.Transport.Libuv.csproj", "src/DotNetty.Transport.Libuv/"]
RUN dotnet restore "RpcServerDemo/RpcServerDemo.csproj"
COPY . .
WORKDIR "/src/RpcServerDemo"
RUN dotnet build "RpcServerDemo.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "RpcServerDemo.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RpcServerDemo.dll"]
