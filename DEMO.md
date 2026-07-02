# Demo script

## 1. Show the unsafe log

```bash
sed -n '1,40p' src/UnsafeApi/Program.cs
```

Expected bad values:

- `alex.customer@example.invalid`
- `Bearer demo-token-not-real`
- `123-45-6789`
- `customerId-78441`

## 2. Show scanner findings

```bash
cat examples/findings.json
cat examples/findings.sarif
```

Expected rules:

- `PII.EMAIL.VALUE`
- `AUTH.BEARER_TOKEN.FIELD_OR_VALUE`
- `PII.SSN.VALUE`
- `PII.CUSTOMER_ID.FIELD`

## 3. Show pipeline fail/pass gates

```bash
cat .github/workflows/cerbi-scan.yml
cat .azure-pipelines/cerbi-scan.yml
```

The scanner step confirms `src/UnsafeApi` fails and `src/SafeApi` passes.

## 4. Show the developer fix

```bash
sed -n '1,80p' src/SafeApi/Program.cs
```

Expected safe fields:

- `customerRef`
- `tokenPresent`
- `ssnPresent`
- `failureCode`

## 5. Run local proof

```bash
dotnet test cerbi-undeniable-demo.sln
```

Expected result: all tests pass.

## 6. Optional runtime governance

```bash
dotnet run --project src/SafeApi
curl -s -X POST http://localhost:5000/runtime/redact
```

Expected behavior: the local governance engine reports violations and returns `[REDACTED]` for unsafe fields without calling a remote service.
