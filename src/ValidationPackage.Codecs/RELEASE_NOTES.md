## 0.1.0-preview.4 - 2026-08-14

- Add portable frontmatter extraction and YAML/JSON codecs for validation-package metadata.
- Add schema-directed canonical and legacy frontmatter/configuration decoding
  with a strict JSON-compatible YAML scalar profile.
- Narrow the AVPR CWL scalar contract and reject unknown declaration fields.
- Ship the reviewed frontmatter and configuration Draft 2020-12 schemas in
  NuGet, npm, and wheel artifacts.
- Use YAMLicious for YAML writing with the Fable 5.11 Python compiler fix.
- Organize JSON and YAML encoders and decoders into separate source trees.
- Build installable npm and Python packages with an explicit native Model dependency.
- Publish one versioned artifact set through independently retriable NuGet,
  npm, and PyPI trusted-publishing jobs.
