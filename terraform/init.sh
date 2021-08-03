#!/usr/bin/env bash

PROJECT_ID="${1:-$(gcloud config get-value project)}"
PROJECT_NUMBER=$(gcloud projects describe $PROJECT_ID --format="value(projectNumber)")
MY_SOURCE_RANGE="107.139.105.0/24" # Update this to your source range
REGIONS=("us-central1" "us-west1") # Subnetwork Regions to enable logging on

function update_subnet_config() {
    SUBNET="$1"
    REGION="$2"

    gcloud compute networks subnets update $SUBNET --project $PROJECT_ID \
        --region $REGION \
        --enable-private-ip-google-access

    gcloud compute networks subnets update $SUBNET --project $PROJECT_ID \
        --region $REGION \
        --enable-flow-logs
}

function create_instance() {
    INSTANCE_NAME="$1"
    ZONE="$2"

    gcloud beta compute \
        --project=$PROJECT_ID instances create "$INSTANCE_NAME" \
        --zone="$ZONE" \
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
        --boot-disk-device-name="$INSTANCE_NAME" --shielded-secure-boot \
        --shielded-vtpm \
        --shielded-integrity-monitoring \
        --reservation-affinity=any
}

function create_gke() {
    gcloud beta container \
        --project "gwc-sandbox" clusters create "elk" --region "us-central1" --no-enable-basic-auth --cluster-version "1.20.8-gke.900" --release-channel "regular" --machine-type "e2-medium" --image-type "COS_CONTAINERD" --disk-type "pd-standard" --disk-size "100" --metadata disable-legacy-endpoints=true --scopes "https://www.googleapis.com/auth/cloud-platform" --max-pods-per-node "110" --num-nodes "1" --enable-stackdriver-kubernetes --enable-ip-alias --network "projects/gwc-sandbox/global/networks/default" --subnetwork "projects/gwc-sandbox/regions/us-central1/subnetworks/default" --enable-intra-node-visibility --default-max-pods-per-node "110" --enable-autoscaling --min-nodes "0" --max-nodes "3" --enable-network-policy --no-enable-master-authorized-networks --addons HorizontalPodAutoscaling,HttpLoadBalancing,GcePersistentDiskCsiDriver --enable-autoupgrade --enable-autorepair --max-surge-upgrade 1 --max-unavailable-upgrade 0 --workload-pool "gwc-sandbox.svc.id.goog" --enable-shielded-nodes --shielded-secure-boot
}

# 1. Create Firewall Rules with Logging Enabled
# for (1) Allow all http and (2) Deny all others
gcloud compute \
    --project=${PROJECT_ID} \
    firewall-rules create allow-all-http \
    --description="Allows HTTP for all instances in VPC" \
    --direction=INGRESS \
    --priority=1000 \
    --network=default \
    --action=ALLOW \
    --rules=tcp:80,tcp:443,tcp:8080 \
    --source-ranges=${MY_SOURCE_RANGE} \
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
    --source-ranges=${MY_SOURCE_RANGE} \
    --enable-logging

for REGION in "${REGIONS[@]}"; do
    update_subnet_config "default" "$REGION"
done

# CREATE GCE VMs for internal traffic testing (VPC Flows/Firewall Logs)
create_instance "apache" "us-central1-a"
create_instance "apache2" "us-central1-a"

# Create SINK: BIGQUERY for Admin Activity LOGS
bq mk admin_logs
gcloud logging sinks create admin_logs \
    "bigquery.googleapis.com/projects/${PROJECT_ID}/datasets/admin_logs" \
    --log-filter='logName="projects/'${PROJECT_ID}'/logs/cloudaudit.googleapis.com%2Factivity"'
WRITER_IDENTITY=$(gcloud logging sinks describe admin_logs --format="value(writerIdentity)")
gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="$WRITER_IDENTITY" \
    --role=roles/bigquery.dataEditor

# Create SINK: BIGQUERY for Data Access LOGS
bq mk data_access
gcloud logging sinks create data_access \
    "bigquery.googleapis.com/projects/${PROJECT_ID}/datasets/data_access" \
    --log-filter='logName="projects/'${PROJECT_ID}'/logs/cloudaudit.googleapis.com%2Fdata_access"'
WRITER_IDENTITY=$(gcloud logging sinks describe data_access --format="value(writerIdentity)")
gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="$WRITER_IDENTITY" \
    --role=roles/bigquery.dataEditor

# Create SINK to BIGQUERY for VPC Flows
bq mk vpc_flows
gcloud logging sinks create vpc_flows \
    "bigquery.googleapis.com/projects/${PROJECT_ID}/datasets/vpc_flows" \
    --log-filter='resource.type="gce_subnetwork"'
WRITER_IDENTITY=$(gcloud logging sinks describe vpc_flows --format="value(writerIdentity)")
gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="$WRITER_IDENTITY" \
    --role=roles/bigquery.dataEditor

# SINK: BIGQUERY for Firewall Logs
bq mk firewall
gcloud logging sinks create firewall \
    "bigquery.googleapis.com/projects/${PROJECT_ID}/datasets/firewall" \
    --log-filter='logName="projects/'${PROJECT_ID}'/logs/compute.googleapis.com%2Ffirewall"'
WRITER_IDENTITY=$(gcloud logging sinks describe firewall --format="value(writerIdentity)")
gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="$WRITER_IDENTITY" \
    --role=roles/bigquery.dataEditor
