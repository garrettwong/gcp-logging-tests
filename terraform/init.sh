#!/usr/bin/env bash

PROJECT_ID="${1:-$(gcloud config get-value project)}"

# enable audit logs


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
    --source-ranges=107.139.105.0/24 \
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
    --source-ranges=107.139.105.0/24 \
    --enable-logging

regions=( "us-central1" "us-west1" )
for REGION in "${regions[@]}"
do
    gcloud compute networks subnets update default --project $PROJECT_ID \
        --region $REGION \
        --enable-private-ip-google-access

    gcloud compute networks subnets update default --project $PROJECT_ID \
        --region $REGION \
        --enable-flow-logs
done


# create vm
PROJECT_NUMBER=$(gcloud projects describe $PROJECT_ID --format="value(projectNumber)")
gcloud beta compute \
    --project=$PROJECT_ID instances create apache \
    --zone=us-central1-a \
    --machine-type=e2-medium \
    --subnet=default \
    --network-tier=PREMIUM \
    --metadata=startup-script=apt-get\ update\ -y$'\n'apt-get\ install\ -y\ apache2 --maintenance-policy=MIGRATE \
    --service-account=${PROJECT_NUMBER}-compute@developer.gserviceaccount.com \
    --scopes=https://www.googleapis.com/auth/cloud-platform \
    --image-family=debian-10 \
    --image-project=debian-cloud \
    --boot-disk-size=10GB \
    --boot-disk-type=pd-balanced \
    --boot-disk-device-name=apache --shielded-secure-boot \
    --shielded-vtpm --shielded-integrity-monitoring \
    --reservation-affinity=any