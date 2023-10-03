# gcp-logging-tests

`gcp-logging-tests` focuses on the continuous verification of the end to end processes of generating events and their associated Google Cloud Platform logs.  We want to identify log diffs, new log generations in common cloud workflows.  

## Getting Started

We recommend ensuring that you familiarize yourself with the different types of [Google Cloud Audit Logs](https://cloud.google.com/logging/docs/audit).

### Prerequisites

1. A Google Cloud Project with an associated Billing Account
1. The default role of `roles/owner` will work.  If you do not have owner, you will need the following roles: (`roles/compute.admin`, `roles/storage.admin`, `roles/resourcemanager.projectIamAdmin`) - the service account will create GCE VMs, GCS buckets, FW rules, Subnets, and SetIAMPolicy

### Running init.sh

Running `bash/init.sh` will configure your environment for testing.  Running `bash/teardown.sh` will teardown your environment.

### Local

1. Create a Service Account with the IAM Roles (`roles/compute.admin, roles/storage.admin, roles/resourcemanager.projectIamAdmin, roles/iam.securityReviewer, roles/viewer`)
2. Download a Service Account Key JSON file from GCP (assume its called `sa-key.json`).
3. Set an env var, `export GOOGLE_APPLICATION_CREDENTIALS=sa-key.json`
4. To unset the env var after dev, use `unset GOOGLE_APPLICATION_CREDENTIALS`

### Github Actions Setup

Refer to: https://github.com/garrettwong/gcp-logging-tests/settings/secrets/actions

1. Create a Service Account JSON Key, `sa-key.json` that is used locally
2. On Mac OSX, run `base64 -i sa-key.json`
3. Copy the contents of that and create a GITHUB Secret in the Repository called `GCP_CREDENTIALS`
4. Create another GITHUB Secret called `GCP_PROJECT_ID` with the value of the GCP Project ID
5. Commit to Github using
```bash
git add . 
git commit -m "My Commit Message"
git push -u origin main

# OR...
GITUSER=""
GITTOKEN=""
git push -u   "https://$GITUSER":"$GITTOKEN@github.com/$GITUSER/gcp-logging-tests.git"
```

## Running this Solution

```bash
dotnet build
dotnet test
```

## Contributing

See [CONTRIBUTING.md](./CONTRIBUTING.md) for details around contributing.