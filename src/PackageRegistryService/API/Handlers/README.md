# API handlers

This folder contains request-processing behavior for the programmatic API. Handlers coordinate validation, database queries, service-model conversion, side effects such as download accounting, and typed HTTP results.

Query projections that protect lightweight endpoints from loading executable content also live here because they are specific to API behavior rather than general persistence models.
