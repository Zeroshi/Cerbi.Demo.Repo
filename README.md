# cerbi-undeniable-demo

A small .NET 8 repository that demonstrates one thing: unsafe application log fields are stopped in-process before they can be exported.

This is intentionally local and boring. There is no Azure, Docker, Kubernetes, queue, database, Splunk, Datadog, or paid service. The demo uses a small in-repo governance runtime so a reviewer can inspect every decision. In a real Cerbi integration, `src/Cerbi.Demo.Governance` is the seam where the Cerbi runtime package would replace the local demo engine.

## Quick start

```bash
dotnet test
```

Expected result:

```text
Passed!  - Failed: 0, Passed: 3, Skipped: 0
```

## What the evidence shows

Every unsafe finding includes the items a security reviewer needs:

| Evidence item | Example |
| --- | --- |
| Sensitive field | `cardNumber` |
| Why it is risky | `Payment account numbers in logs increase PCI scope...` |
| Rule that caught it | `PCI.PAN.VALUE` |
| Outcome | `blocked` |
| Developer action | `Remove cardNumber from the log and keep only cardLast4...` |

## What to inspect

| Area | File | Why it matters |
| --- | --- | --- |
| Unsafe log | `src/Cerbi.Demo.Api/PaymentLogExamples.cs` | Shows the exact fields a developer should not log. |
| Safe log | `src/Cerbi.Demo.Api/PaymentLogExamples.cs` | Shows the corrected structured replacement. |
| Policy | `policy/governance-profile.json` | Defines the rule, risk, outcome, and developer action. |
| Runtime | `src/Cerbi.Demo.Governance/CerbiGovernanceEngine.cs` | Evaluates log fields locally with no network call in the hot path. |
| Tests | `tests/Cerbi.Demo.Tests/GovernanceTests.cs` | Proves block, pass, and evidence behavior. |

## Before: unsafe log shape

The unsafe example attempts to log:

```text
customerEmail  = demo.customer@example.invalid
cardNumber     = 4111 1111 1111 1111
authorization  = Bearer demo-token-not-real
debugPayload   = { ...free-form request body... }
```

The policy blocks the event. It is not added to the accepted log sink.

## After: safe log shape

The corrected example logs stable, reviewed fields:

```text
customerRef            = cust_demo_001
paymentInstrumentType  = card
cardLast4              = 1111
tokenPresent           = true
failureCode            = issuer_declined
```

The policy accepts the event.

## Optional API run

Start the API:

```bash
dotnet run --project src/Cerbi.Demo.Api
```

Fetch a single before/after evidence payload:

```bash
curl -s http://localhost:5000/demo/evidence
```

The unsafe section includes `fieldName`, `risk`, `ruleId`, `outcome`, and `developerAction` for each finding. The safe section has `outcome: allowed` and no violations.

Call each path separately:

```bash
curl -i -X POST http://localhost:5000/demo/unsafe
curl -i -X POST http://localhost:5000/demo/safe
```

Expected behavior:

- `/demo/unsafe` returns `400 Bad Request` and rule evidence.
- `/demo/safe` returns `200 OK` and `isAllowed: true`.
- `/demo/evidence` returns both attempts in one payload.

## What this does not claim

- It is not a production Cerbi package.
- It is not an observability pipeline.
- It does not require an external control plane at runtime.
- It does not contain real personal data, secrets, connection strings, or API keys.
