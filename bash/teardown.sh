#!/usr/bin/env bash

# Update these variables
PROJECT_ID="${1:-$(gcloud config get-value project)}"
PROJECT_NUMBER=$(gcloud projects describe $PROJECT_ID --format="value(projectNumber)")
MY_SOURCE_RANGE="107.139.105.0/24" # Update this to your source range
REGIONS=("us-central1" "us-west1") # Subnetwork Regions to enable logging on

function delete_firewall_rule() {
    NAME="$1"

    gcloud compute \
        --project=${PROJECT_ID} \
        firewall-rules delete $NAME \
        --quiet
}

function remove_subnet_config() {
    SUBNET="$1"
    REGION="$2"

    gcloud compute networks subnets update $SUBNET --project $PROJECT_ID \
        --region $REGION \
        --no-enable-private-ip-google-access

    gcloud compute networks subnets update $SUBNET --project $PROJECT_ID \
        --region $REGION \
        --no-enable-flow-logs
}

function delete_instance() {
    INSTANCE_NAME="$1"
    ZONE="$2"

    gcloud beta compute \
        --project=$PROJECT_ID instances delete $INSTANCE_NAME \
        --zone=$ZONE \
        --quiet
}

function delete_sink() {
    SINK="$1"

    bq rm -f $SINK
    WRITER_IDENTITY=$(gcloud logging sinks describe $SINK --format="value(writerIdentity)")
    gcloud projects remove-iam-policy-binding $PROJECT_ID \
        --member="$WRITER_IDENTITY" \
        --role=roles/bigquery.dataEditor --quiet
    gcloud logging sinks delete --project $PROJECT_ID $SINK --quiet
}


delete_firewall_rule "allow-all-http"
delete_firewall_rule "deny-all"

for REGION in "${REGIONS[@]}"; do
    remove_subnet_config "default" $REGION
done

# CREATE GCE VMs for internal traffic testing (VPC Flows/Firewall Logs)
delete_instance "apache" "us-central1-a"
delete_instance "apache2" "us-central1-a"

# Delete Sinks
delete_sink "admin_logs"
delete_sink "data_access"
delete_sink "vpc_flows"
delete_sink "firewall"

gcloud iam service-accounts delete gen-iam-creds-sa --project $PROJECT_ID
gcloud iam service-accounts delete gcp-logging-tests --project $PROJECT_ID
SA_EMAIL="gcp-logging-tests@${PROJECT_ID}.iam.gserviceaccount.com"
SA_ROLES=("roles/compute.admin" "roles/storage.admin" "roles/resourcemanager.projectIamAdmin" "roles/iam.securityReviewer" "roles/viewer")

for SA_ROLE in "${SA_ROLES[@]}"; do
    gcloud projects remove-iam-policy-binding $PROJECT_ID \
        --member="serviceAccount:${SA_EMAIL}" \
        --role="${SA_ROLE}"
done

