# API

This folder contains the programmatic HTTP surface of the registry service. It separates the shapes sent over the wire, route registration, and request-processing behavior so each concern can evolve without turning the application entry point into an endpoint implementation.

Website routes belong under `Pages`; persistence entities and database configuration belong under `Models`.
