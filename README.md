# GCP LOGGING TESTS

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


