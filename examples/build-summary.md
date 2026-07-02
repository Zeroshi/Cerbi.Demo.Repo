# Cerbi demo build summary

## 1. Bad code logs sensitive values

`src/UnsafeApi/Program.cs` intentionally logs an email address, bearer token, SSN-like value, and raw customer ID. This is the pre-fix developer mistake.

## 2. Cerbi Scanner finds it and the pipeline fails

The Cerbi Scanner step evaluates the unsafe source against `cerbi-policy.yml`. The sample findings are captured in `examples/findings.json` and `examples/findings.sarif` for CI/security tooling review.

Result: **failed** because blocked sensitive data patterns were found before logs could leave the application.

## 3. Developer fixes it

`src/SafeApi/Program.cs` replaces the unsafe values with reviewed, structured fields: `customerRef`, `tokenPresent`, `ssnPresent`, and `failureCode`.

## 4. Pipeline passes

The same scanner expression is run against `src/SafeApi`. No blocked patterns are present, so the fixed sample passes.

## 5. Optional runtime governance blocks/redacts the same pattern

`POST /runtime/redact` in `src/SafeApi` evaluates an unsafe candidate locally with the same governance policy and returns a redacted payload. This keeps the hot path deterministic and avoids remote control-plane dependencies.
