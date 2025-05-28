# Determine script directory and set project root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$SCRIPT_DIR/.."

# Change to repo root if not already there
if [ ! -f "$REPO_ROOT/KaffeMaskineProjekt.sln" ]; then
    echo "Could not find repo root. Exiting."
    exit 1
fi
cd "$REPO_ROOT"

# git pull with submodules before everything
git pull --recurse-submodules

# Remove package-lock.json if it exists in AppHost
if [ -f "KaffeMaskineProjekt.AppHost/package-lock.json" ]; then
    rm KaffeMaskineProjekt.AppHost/package-lock.json
fi

# If publish folder exists, run docker compose down in it
if [ -d "publish" ]; then
    (cd publish && docker compose down)
fi

# Run aspire publish -o publish in KaffeMaskineProjekt
aspire publish -o publish

# Run docker compose up in the publish folder with KAFFEDBSERVER_PASSWORD env variable
(cd publish && KAFFEDBSERVER_PASSWORD=callmeal docker compose up)