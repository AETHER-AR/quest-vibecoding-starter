# Unity security patch validation

The filming session exposed Unity Hub's security alert on 2022.3.62f1. Unity's September 2025 advisory and 2022.3.62f3 release notes identify the corrected editor/runtime release.

- https://unity.com/security/sept-2025-01
- https://unity.com/releases/editor/whats-new/2022.3.62f3

This isolated branch updates the exact version guards and ProjectVersion together. No runtime, scene or interaction code changes are intended. The original project and installed headset build remain untouched.

Pre-edit GitNexus: Configure has HIGH risk, four impacted callers and three affected entry processes: Prepare, BuildStarter and BuildFinished. Direct callers are Build and Prepare; the two build entry points call Build. Build itself reported LOW with two direct callers. The PowerShell file reported UNKNOWN, which was not treated as unused; text references confirm README and AGENTS invoke it for preflight, building and installation. Existing Unity menu attributes and executeMethod strings were inspected.

Both Android builds and fresh hardware acceptance are required before calling this a verified tutorial release. At branch creation, these patch-specific checks are pending. Historical f1 hardware observations remain historical evidence only.
