# Online Health Consultation Desktop

Desktop client for the Online Health Consultation service-oriented software project.

This WPF app is intentionally scoped for the **doctor workstation** use case. It demonstrates that the NestJS backend exposes a reusable HTTP/JSON service consumed by a second platform and a different language stack:

- Web client: React + TypeScript.
- Desktop client: WPF + C# + .NET 8.
- Shared service: NestJS REST API with JWT authentication.

## Tech Stack

- .NET 8 WPF
- MVVM with Caliburn.Micro
- `HttpClient` for REST API calls
- LiteDB package included for local desktop storage needs

## Features

- Doctor login using `/api/auth/login`
- Doctor profile/dashboard using `/api/doctors/me/profile`
- Doctor appointments using `/api/appointments/doctor/me`
- Confirm and complete appointment actions
- Doctor question inbox using `/api/questions/assigned`
- Answer health questions using `/api/questions/{id}/answers`

## Run on Windows

1. Install .NET 8 SDK.
2. Start the backend service.
3. Open `OnlineHealthConsultation-Desktop.sln` in Visual Studio 2022.
4. Restore NuGet packages.
5. Build and run `OnlineHealthConsultation.Desktop`.

The default API base URL is:

```text
http://localhost:4000/api
```

You can change it on the login screen before signing in.
