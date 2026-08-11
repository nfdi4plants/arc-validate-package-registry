FROM mcr.microsoft.com/devcontainers/dotnet:2.0.4-10.0-noble

# The base image currently carries this obsolete source without its signing key.
# Node is installed by a Dev Container Feature and uses Corepack for Yarn.
RUN rm -f /etc/apt/sources.list.d/yarn.list
