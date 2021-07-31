# CONTRIBUTING

Contribution guidelines.

## Overview

1. If you are interested in contributing to this REPO, create a FORK of this repository.  
2. Clone the local repository using `git clone git@github.com:${YOUR_USER}/gcp-logging-tests.git`
3. For more information regarding adding a test, refer to the [Add a Test Example] below. 
4. Ensure that the `dotnet build` builds correctly.  Ensure that all tests pass when running `dotnet test`.  Ensure that `dotnet format` passes.
5. Commit code back to the repository using
```bash
git add . 
git commit -m "Your Commit Message"
git push -u origin main
```
6. Ensure that the Github Actions builds complete successfully.
7. Submit a PR request back to the origin repository.

## Reporting a Vulnerability

Use this section to tell people how to report a vulnerability.

Tell them where to go, how often they can expect to get an update on a
reported vulnerability, what to expect if the vulnerability is accepted or
declined, etc.
