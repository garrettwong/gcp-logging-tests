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


# SINK: BIGQUERY for ADMIN LOGS
bq mk admin_logs
gcloud logging sinks create admin_logs \
    "bigquery.googleapis.com/projects/${PROJECT_ID}/datasets/admin_logs" \
    --log-filter='logName="projects/'${PROJECT_ID}'/logs/cloudaudit.googleapis.com%2Factivity"'
WRITER_IDENTITY=$(gcloud logging sinks describe admin_logs --format="value(writerIdentity)")
gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="$WRITER_IDENTITY" \
    --role=roles/bigquery.dataEditor

bq mk data_access
gcloud logging sinks create data_access \
    "bigquery.googleapis.com/projects/${PROJECT_ID}/datasets/data_access" \
    --log-filter='logName="projects/'${PROJECT_ID}'/logs/cloudaudit.googleapis.com%2Fdata_access"'
WRITER_IDENTITY=$(gcloud logging sinks describe data_access --format="value(writerIdentity)")
gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="$WRITER_IDENTITY" \
    --role=roles/bigquery.dataEditor

# SINK to BIGQUERY for VPC Flows
bq mk vpc_flows
gcloud logging sinks create vpc_flows \
    "bigquery.googleapis.com/projects/${PROJECT_ID}/datasets/vpc_flows" \
    --log-filter='resource.type="gce_subnetwork"'
WRITER_IDENTITY=$(gcloud logging sinks describe vpc_flows --format="value(writerIdentity)")
gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="$WRITER_IDENTITY" \
    --role=roles/bigquery.dataEditor

# SINK: BIGQUERY for FIREWALL
bq mk firewall
gcloud logging sinks create firewall \
    "bigquery.googleapis.com/projects/${PROJECT_ID}/datasets/firewall" \
    --log-filter='logName="projects/'${PROJECT_ID}'/logs/compute.googleapis.com%2Ffirewall"'
WRITER_IDENTITY=$(gcloud logging sinks describe firewall --format="value(writerIdentity)")
gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="$WRITER_IDENTITY" \
    --role=roles/bigquery.dataEditor

# Create second VM
gcloud beta compute \
    --project=$PROJECT_ID instances create apache2 \
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
    --boot-disk-device-name=apache \
    --shielded-secure-boot \
    --shielded-vtpm \
    --shielded-integrity-monitoring \
    --reservation-affinity=any

# generate vpc flow logs
for i in {1..5000}; do 
    curl -H 'Cache-Control: no-cache' "http://34.134.129.29?i=$i"; 
done

# generate vpc flow logs - from internal ip
for i in {1..2500}; do 
    curl -H 'Cache-Control: no-cache' "http://10.128.0.23?i=$i"; 
done

# generate firewall logs
for i in {1..55000}; do 
    port="$(($i + 10000))"
    echo $port
    curl -m 0.5 "34.134.129.29:${port}"; 
done
