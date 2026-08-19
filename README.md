# bas-mod-workflows

Shared GitHub Actions workflows for Blade & Sorcery mod CI/CD.

## Repository structure

```
bas-mod-workflows/
├── .github/
│   ├── actions/relative-symlink/
│   │   └── action.yml             # Reusable action to link asset and catalog folders into the SDK
│   └── workflows/
│       └── build-mod.yml          # Reusable workflow (called by mod repos)
├── scripts/
│   └── CIBuildAddressables.cs     # Injected into BasSDK at build time
└── mod-repo-template/
    └── build.yml                  # Copy this into your mod repo
```

## How it works

1. The reusable workflow checks out both the BasSDK and your mod repo
2. Your mod's `Assets/` folder is symlinked into `BasSDK/Assets/Personal/<repo-name>`
3. `CIBuildAddressables.cs` is injected into the BasSDK project at runtime — no file needed in your mod repo
2. Your mod's `Catalogs/` folder is symlinked into `BasSDK/BuildStaging/Catalogs/CI`
4. Windows and Android builds run in parallel via GameCI

## Setting up a new mod repo

1. Copy `mod-repo-template/build.yml` into your mod repo at `.github/workflows/build.yml`
2. Add these secrets to your mod repo (or organisation-wide):
   - `UNITY_LICENSE` — contents of your `.ulf` license file
   - `UNITY_EMAIL` — Unity account email
   - `UNITY_PASSWORD` — Unity account password
3. Trigger the workflow manually from the Actions tab
