# cerbi-undeniable-demo

A .NET 9 demo showing Cerbi source logging governance: bad code logs sensitive values, Cerbi Scanner finds them, the pipeline fails, the developer fixes the log shape, and the pipeline passes. Optional runtime governance evaluates and redacts the same pattern locally with no remote control-plane dependency in the hot path.

## Required demo files

| Path | Purpose |
| --- | --- |
| `src/UnsafeApi` | Intentionally bad API that logs email, bearer token, SSN-like value, and raw customer ID. |
| `src/SafeApi` | Fixed API that logs reviewed fields and demonstrates local runtime redaction. |
| `cerbi-policy.yml` | Shared governance policy used by scanner examples and runtime demo. |
| `.azure-pipelines/cerbi-scan.yml` | Azure Pipelines CI proof. |
| `.github/workflows/cerbi-scan.yml` | GitHub Actions CI proof. |
| `examples/findings.json` | Example Cerbi Scanner findings. |
| `examples/findings.sarif` | Example SARIF output for code scanning systems. |
| `examples/build-summary.md` | Before/fail/fix/pass narrative. |

## Quick start

```bash
dotnet test cerbi-undeniable-demo.sln
```

> This repo has no production Cerbi service dependency. The demo governance runtime loads cached local rules and evaluates in process.

## Demo flow

1. **Bad code logs sensitive data**: `src/UnsafeApi/Program.cs` logs an email, token, SSN-like value, and customer ID.
2. **Cerbi Scanner finds it**: sample outputs are in `examples/findings.json` and `examples/findings.sarif`.
3. **Pipeline fails**: CI scanner steps intentionally fail when blocked patterns are present in `src/UnsafeApi`.
4. **Developer fixes it**: `src/SafeApi/Program.cs` logs `customerRef`, `tokenPresent`, `ssnPresent`, and `failureCode` instead.
5. **Pipeline passes**: CI verifies the fixed API has no blocked source logging patterns.
6. **Optional runtime governance**: `POST /runtime/redact` evaluates unsafe candidate data locally and returns redacted values.

## Run the APIs

Unsafe sample:

```bash
dotnet run --project src/UnsafeApi
```

Safe sample with local runtime governance:

```bash
dotnet run --project src/SafeApi
curl -s -X POST http://localhost:5000/runtime/redact
```

## What this does not claim

- It is not a production scanner implementation.
- It is not an observability pipeline.
- It does not send application logs through a public control-plane service.
- It does not contain real personal data, credentials, or customer identifiers.
