#!/bin/bash

# Exit immediately if any command exits with a non-zero status
set -e

# Custom formatting for status messages
print_status() {
    echo -e "\n\033[1;36m[~] $1...\033[0m"
}

print_success() {
    echo -e "\033[1;32m[✓] $1\033[0m"
}

# 1. Build Frontend
print_status "Building frontend"
cd SIISVueApp
npm run build
cd ..

# 2. Publish Backend
print_status "Publishing backend"
cd SIISMinimalAPI
dotnet publish -c Release -o ../publish
cd ..

print_success "Build and publish process completed successfully!"