# GCP LOGGING TESTS

GCP Logging Tests is a security focused solution.  It focuses specifically the end to end process of event generation and the resulting, generated GCP logs.  There are various types of [Google Cloud Audit Logs](https://cloud.google.com/logging/docs/audit).

## Getting Started

### Local

1. Download a Service Account Key JSON file from GCP (assume its called `sa-key.json`).
2. Set an env var, `export GOOGLE_APPLICATION_CREDENTIALS=sa-key.json`

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
