#!/usr/bin/env bash

set -euo pipefail

readonly dev_data_dir="${HOME}/.devcontainer-data"
readonly local_bin_dir="${HOME}/.local/bin"

sudo chown -R "$(id -u):$(id -g)" "${dev_data_dir}"
mkdir -p \
    "${dev_data_dir}/aspnet" \
    "${dev_data_dir}/cache" \
    "${dev_data_dir}/config" \
    "${dev_data_dir}/gh" \
    "${dev_data_dir}/share" \
    "${dev_data_dir}/venvs" \
    "${local_bin_dir}"

export PATH="${local_bin_dir}:${PATH}"
export UV_PROJECT_ENVIRONMENT="${UV_PROJECT_ENVIRONMENT:-${dev_data_dir}/venvs/arc-validate-package-registry}"

python -m pip install --user --disable-pip-version-check "uv==0.9.14"
npm install --global --prefix "${HOME}/.local" \
    --allow-scripts=opencode-ai \
    "opencode-ai@latest"

dotnet tool restore
uv sync --locked

printf '\nDevelopment container setup complete.\n'
printf 'Authenticate when needed with: gh auth login\n'
printf 'Start OpenCode with: opencode\n'
