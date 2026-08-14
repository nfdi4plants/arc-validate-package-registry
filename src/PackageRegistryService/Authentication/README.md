# Authentication

This folder contains the registry service's authentication boundary. It owns the API-key endpoint filter and the configuration/header names used to protect write operations.

Route registration decides which endpoints require the filter; handlers should not duplicate credential parsing.
