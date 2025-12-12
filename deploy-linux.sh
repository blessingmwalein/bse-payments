#!/bin/bash

echo "========================================"
echo "BSE Payment Service - Build for Deploy"
echo "========================================"
echo ""

echo "[1/3] Cleaning previous build..."
rm -rf ./publish

echo "[2/3] Building Release version..."
dotnet publish -c Release -o ./publish --self-contained false

if [ $? -ne 0 ]; then
    echo ""
    echo "ERROR: Build failed!"
    exit 1
fi

echo "[3/3] Build complete!"
echo ""
echo "========================================"
echo "Deployment files ready in: ./publish"
echo "========================================"
echo ""
echo "NEXT STEPS:"
echo "1. Edit appsettings.Production.json with your database server details"
echo "2. Copy the 'publish' folder to /var/www/bse-payments"
echo "3. Set up systemd service (see DEPLOYMENT.md)"
echo ""
echo "Connection String Format:"
echo "  Server=YOUR_SERVER;Database=BSEPayments;User Id=USERNAME;Password=PASSWORD;TrustServerCertificate=True"
echo ""
