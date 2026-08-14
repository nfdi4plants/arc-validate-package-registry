# API contracts

This folder contains service-owned request and response shapes that form part of the HTTP API contract. These types describe what a caller receives or submits and should remain independent of database mapping details and executable package content unless an endpoint explicitly exposes it.

Contracts are intentionally distinct from `Models`: models represent service persistence and runtime state, while contracts represent stable transport shapes. Portable validation-package domain types continue to be owned by `ValidationPackage.Model`.
