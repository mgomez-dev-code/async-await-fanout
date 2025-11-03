# AsyncAwaitFanOut (.NET 8 + C#)

![.NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet&logoColor=white)
![Language: C#](https://img.shields.io/badge/Language-C%23-239120?logo=csharp&logoColor=white)
![Tests: xUnit](https://img.shields.io/badge/Tests-xUnit-6aa84f)
![.NET CI](https://github.com/mgomez-dev-code/AsyncAwaitFanOut/actions/workflows/dotnet.yml/badge.svg)
![License: MIT](https://img.shields.io/badge/License-MIT-green)

A clean and modern **.NET 8** solution showcasing **async/await**, **bounded concurrency**, **timeouts**, and **partial success handling** using `Task.WhenAll` and `SemaphoreSlim`.  
This project demonstrates resilient orchestration patterns for fan-out scenarios involving multiple I/O-bound services.

## Features
- ⚙️ **Async/Await orchestration** — fully non-blocking (`Task.WhenAll`, no `.Result` / `.Wait()`).
- 🚦 **Bounded concurrency** with `SemaphoreSlim` to limit parallelism.
- ⏱️ **Per-call timeouts** combined with a global `CancellationToken`.
- 💥 **Partial success** handling: collects individual errors without aborting the batch.
- 🧩 **Clean architecture** split into Core / ConsoleApp / Tests.
- 🧪 **Unit tests** with xUnit.
- 🤖 **Continuous Integration** via GitHub Actions.

## Project Structure
```text
AsyncAwaitFanOut/
├─ AsyncAwaitFanOut.sln
├─ AsyncAwaitFanOut.ConsoleApp/  # Console demo (entry point)
├─ AsyncAwaitFanOut.Core/
│  ├─ DTOs/                      # Domain transfer objects
│  ├─ Interfaces/                # Contracts for external services
│  └─ Services/                  # Business logic (OrderSnapshotService, mocks)
└─ AsyncAwaitFanOut.Tests/       # Unit tests with xUnit
```

## Getting Started

**1) Build & Test**
```bash
dotnet build
dotnet test
```

**2) Run the Console App**
```bash
dotnet run --project AsyncAwaitFanOut.ConsoleApp
```

## Example Output
```text
=== Order Snapshots ===
- 23f0a3c5 :: Order? yes, Payment? yes, Shipment? yes :: OK
- 9d8b1e22 :: Order? yes, Payment? no, Shipment? yes :: PaymentService: timeout
- 7fa43b0a :: Order? yes, Payment? yes, Shipment? yes :: OK

Done.
```

## Architecture Notes
- **OrderSnapshotService** orchestrates all external calls, enforcing throttling and timeouts.
- **SafeCall<T>** wrapper merges per-call timeout with the outer cancellation token.
- **Services** simulate I/O latency (1–3s range) to illustrate concurrency patterns.
- **Error handling** follows *partial success* design — failures are logged in each snapshot’s error list.

## License
This project is licensed under the **MIT License**. See `LICENSE` for details.
