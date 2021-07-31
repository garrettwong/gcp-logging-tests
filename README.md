# GCP LOGGING TESTS

GCP Logging Tests is a security focused solution.  It focuses specifically the end to end process of event generation and the resulting, generated GCP logs.  There are various types of [Google Cloud Audit Logs](https://cloud.google.com/logging/docs/audit).

## Getting Started

### Prerequisites

* A Google Cloud Project with an associated Billing Account
* The default role of roles/owner will work.  If you do not have owner, you will need the following roles (`roles/compute.admin, roles/storage.admin, roles/resourcemanager.projectIamAdmin`) for:
  * Create GCE VMs
  * Create GCS Buckets
  * Create Firewall Rules
  * Update Subnet Configurations
  * Set IAM Policy

### Local

1. Create a Service Account with the IAM Roles (`roles/compute.admin, roles/storage.admin, roles/resourcemanager.projectIamAdmin, roles/iam.securityReviewer, roles/viewer`)
2. Download a Service Account Key JSON file from GCP (assume its called `sa-key.json`).
3. Set an env var, `export GOOGLE_APPLICATION_CREDENTIALS=sa-key.json`

### Github Actions

1. Create a sa-key.json file that is used locally
2. On Mac OSX, run `base64 sa-key.json`
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