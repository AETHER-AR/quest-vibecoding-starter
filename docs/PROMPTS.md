# Prompts by chapter

Use these as examples of intent. Codex should inspect the files rather than guess your install paths.

## Setup
Read the README and AGENTS.md. Check the required Unity version, Android tools and my USB-connected Quest. List missing prerequisites and which steps require me personally. Run the preflight without changing project versions.

## First run
Build and install the starter scene using the provided commands. Confirm the build succeeded before installing it. Give me a short checklist for what I should see inside the headset.

## Interaction
In the starter scene, let me aim at the sculpture with my right controller, hold grip to move it, and release to leave it there. Keep its position continuous when grabbing. A changes between three colors while targeted or held; B resets in front of me. Preserve passthrough. The finished scene is available as a reference. Build and install for testing.

## Personalize
Make the sculpture rotate more slowly and use an amber color first. Keep the overall size, controller interaction and reset. Tell me which values changed, then rebuild and install.

## Feedback template
When I [action], I see [actual behavior]. I expected [desired behavior]. Keep [working behavior] unchanged. Find the cause, make the smallest appropriate change, rebuild and reinstall. Give me a test that would expose the original problem.

## Save
Review the exact changes, check that no private files are included, and save a Git checkpoint. Record which APK was tested and which headset checks passed. Don't mark unknown checks as passed.
