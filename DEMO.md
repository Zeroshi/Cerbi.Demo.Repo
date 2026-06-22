# Demo script

Use this script to show the before/after behavior without any external service.

## 1. Run the proof

```bash
dotnet test
```

Expected output includes:

```text
Passed!  - Failed: 0, Passed: 3, Skipped: 0
```

The tests prove:

- `Unsafe_payment_log_is_blocked_and_not_exported`
- `Safe_payment_log_is_accepted_for_export`
- `Blocked_log_includes_actionable_policy_evidence`

## 2. Show the unsafe source log

```bash
sed -n '15,20p' src/Cerbi.Demo.Api/PaymentLogExamples.cs
```

Expected fields:

```text
customerEmail
cardNumber
authorization
debugPayload
```

## 3. Show the safe replacement

```bash
sed -n '22,28p' src/Cerbi.Demo.Api/PaymentLogExamples.cs
```

Expected fields:

```text
customerRef
paymentInstrumentType
cardLast4
tokenPresent
failureCode
```

## 4. Show the policy evidence

```bash
cat policy/governance-profile.json
```

Expected evidence fields on every rule:

```text
"id"
"dataClass"
"severity"
"action": "block"
"evidence"
"risk"
"recommendation"
"developerAction"
```

Expected rule ids:

```text
PII.EMAIL.VALUE
PCI.PAN.VALUE
AUTH.BEARER_TOKEN.FIELD_OR_VALUE
DEBUG.RAW_PAYLOAD.FIELD
```

## 5. Optional API evidence

Start the API:

```bash
dotnet run --project src/Cerbi.Demo.Api
```

Fetch both attempts:

```bash
curl -s http://localhost:5000/demo/evidence
```

Expected output includes:

```text
"eventName":"PaymentAttemptFailed.Unsafe"
"outcome":"blocked"
"fieldName":"customerEmail"
"ruleId":"PII.EMAIL.VALUE"
"risk":"Direct identifiers in logs can expose personal data"
"developerAction":"Replace customerEmail with customerRef"
"fieldName":"cardNumber"
"ruleId":"PCI.PAN.VALUE"
"risk":"Payment account numbers in logs increase PCI scope"
"developerAction":"Remove cardNumber from the log"
"fieldName":"authorization"
"ruleId":"AUTH.BEARER_TOKEN.FIELD_OR_VALUE"
"fieldName":"debugPayload"
"ruleId":"DEBUG.RAW_PAYLOAD.FIELD"
"eventName":"PaymentAttemptFailed.Safe"
"outcome":"allowed"
"isAllowed":true
```

Call the unsafe endpoint directly:

```bash
curl -i -X POST http://localhost:5000/demo/unsafe
```

Expected output includes:

```text
HTTP/1.1 400 Bad Request
Cerbi blocked unsafe log before it reached observability tools.
"fieldName":"cardNumber"
"outcome":"blocked"
"ruleId":"PCI.PAN.VALUE"
"risk":"Payment account numbers in logs increase PCI scope"
"developerAction":"Remove cardNumber from the log"
```

Call the safe endpoint directly:

```bash
curl -i -X POST http://localhost:5000/demo/safe
```

Expected output includes:

```text
HTTP/1.1 200 OK
Safe structured log accepted.
"isAllowed":true
```
