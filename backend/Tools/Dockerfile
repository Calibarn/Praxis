# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:e1ffd2a92ae84c1291bc1b6887501f8af98e6331e7af6d4c8d37168c5e87a64c AS build

WORKDIR /src

COPY Praxis.Backend.Application/ Praxis.Backend.Application/
COPY Praxis.Backend.Host/ Praxis.Backend.Host/
RUN dotnet publish Praxis.Backend.Host/Praxis.Backend.Host.csproj \
    --configuration Release --output /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:a4556ed033fa96f984bb7a8d348851cb2d36b1281dd2420070045f664fbb5f94

RUN apt-get update \
    && apt-get install --no-install-recommends --yes curl \
    && rm -rf /var/lib/apt/lists/*

RUN useradd --create-home --uid 1001 appuser
WORKDIR /app
COPY --from=build /app ./
RUN chown -R appuser:appuser /app

USER appuser

EXPOSE 8000
ENV ASPNETCORE_URLS=http://+:8000

HEALTHCHECK --interval=10s --timeout=3s --start-period=10s --retries=3 \
  CMD curl --fail http://127.0.0.1:8000/health || exit 1

ENTRYPOINT ["/bin/sh", "-c", "dotnet Praxis.Backend.Host.dll --migrate && exec dotnet Praxis.Backend.Host.dll"]
