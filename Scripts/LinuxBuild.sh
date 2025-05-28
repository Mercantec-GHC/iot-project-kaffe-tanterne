# Remove package-lock.json if it exists in AppHost
if [ -f "../KaffeMaskineProjekt.AppHost/package-lock.json" ]; then
    rm ../KaffeMaskineProjekt.AppHost/package-lock.json
fi

# If publish folder exists, run docker compose down in it
if [ -d "../publish" ]; then
    (cd ../publish && docker compose down)
fi

# Run aspire publish -o publish in KaffeMaskineProjekt
(cd ../ && aspire publish -o publish)

# Run docker compose up in the publish folder
(cd ../publish && docker compose up)