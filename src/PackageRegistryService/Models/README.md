# Service models

This folder contains registry-service persistence entities, Entity Framework configuration, and small runtime value objects used by service and page code. It also contains explicit mappings between service-owned storage shapes and the portable validation-package domain.

These types may reflect database needs and are not automatically HTTP contracts. Transport-specific response shapes belong under `API/Contracts`; canonical portable metadata and validation rules belong to `ValidationPackage.Model`.
