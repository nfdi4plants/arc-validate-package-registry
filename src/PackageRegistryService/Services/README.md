# Application services

This folder contains reusable service-level capabilities and their interfaces. Current responsibilities include loading repository documentation, rendering Markdown, and exposing build and release identity to API and page callers.

Services hide filesystem, rendering-library, and assembly/configuration details from handlers. HTTP-specific result construction remains at the API or page boundary.
