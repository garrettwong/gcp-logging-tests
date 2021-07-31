# GCP LOGGING TESTS

GCP Logging Tests is a security focused solution.  It focuses specifically the end to end process of event generation and the resulting, generated GCP logs.  There are various types of [Google Cloud Audit Logs](https://cloud.google.com/logging/docs/audit).

## Getting Started

Preparing your Github Action Build

1. Create a sa-key.json file that is used locally
2. On Mac OSX, run `base64 sa-key.json`
3. Copy the contents of that and create a GITHUB Secret in the Repository called `GCP_CREDENTIALS`
4. Create another GITHUB Secret called `GCP_PROJECT_ID` with the value of the GCP Project ID
5. Commit to Github using
```bash
git add . 
git commit -m "test this commit"
git push -u origin main
```

## Running this Solution

```bash
dotnet build
dotnet test
```

