FROM mcr.microsoft.com/devcontainers/base:ubuntu-24.04

# Remove the legacy Yarn apt source if it is present in the base image.
# Node is installed by a Dev Container Feature and uses Corepack for Yarn.
RUN rm -f /etc/apt/sources.list.d/yarn.list
