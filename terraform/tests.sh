#!/usr/bin/env bash

PROJECT_ID="${1:-$(gcloud config get-value project)}"
PROJECT_NUMBER=$(gcloud projects describe $PROJECT_ID --format="value(projectNumber)")
MY_SOURCE_RANGE="107.139.105.0/24" # Update this to your source range
REGIONS=("us-central1" "us-west1") # Subnetwork Regions to enable logging on

# Generate VPC Flow Logs
for i in {1..5000}; do
    curl -H 'Cache-Control: no-cache' "http://34.134.129.29?i=$i"
done

# Generate VPC Flow Logs - from internal ip
for i in {1..2500}; do
    curl -H 'Cache-Control: no-cache' "http://10.128.0.23?i=$i"
done

# Generate Firewall Logs
for i in {1..55000}; do
    port="$(($i + 10000))"
    echo $port
    curl -m 0.5 "34.134.129.29:${port}"
done
