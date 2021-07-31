#!/usr/bin/env bash

PROJECT_ID="${1:-$(gcloud config get-value project)}"

# create fw rule to allow all http
gcloud compute \
    --project=${PROJECT_ID} \
    firewall-rules create allow-all-http \
    --description="Allows HTTP for all instances in VPC" \
    --direction=INGRESS \
    --priority=1000 \
    --network=default \
    --action=ALLOW \
    --rules=tcp:80,tcp:443,tcp:8080 \
    --source-ranges=107.139.0.0/24 \
    --enable-logging

gcloud compute \
    --project=${PROJECT_ID} \
    firewall-rules create deny-all \
    --description="Allows HTTP for all instances in VPC" \
    --direction=INGRESS \
    --priority=10000 \
    --network=default \
    --action=DENY \
    --rules=all \
    --source-ranges=107.139.105.69/32 \
    --enable-logging

regions=( "us-west1" "us-central1" )
for REGION in "${regions[@]}"
do
    gcloud compute networks subnets update default --project $PROJECT_ID \
        --region $REGION \
        --enable-private-ip-google-access

    gcloud compute networks subnets update default --project $PROJECT_ID \
        --region $REGION \
        --enable-flow-logs
done
