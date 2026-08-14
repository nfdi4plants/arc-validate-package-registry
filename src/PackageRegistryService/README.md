# Package registry service

This project hosts the AVPR HTTP service and its small server-rendered website. The application entry point composes database access, API routes, page routes, documentation generation, authentication, and service-level infrastructure.

The subfolders separate transport concerns from persistence and presentation. API contracts describe wire responses, models describe service-owned state, services provide reusable application capabilities, and pages render the human-facing website. Cross-target validation-package domain behavior continues to live in the dedicated portable model and codec projects.

Runtime content copied into the application, such as documentation, schemas, release notes, and staged packages, is owned at repository level or by its source project rather than by this folder structure.
